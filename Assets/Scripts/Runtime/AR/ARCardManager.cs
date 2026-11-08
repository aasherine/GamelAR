using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Pusat logika kartu: menerima laporan "kartu X terlihat di pose ini" dari sumber pelacakan
    /// (ARTrackedImageManager di perangkat, atau EditorCardSimulator di Editor), lalu
    /// menampilkan model 3D, mengatur audio (campur / solo) dan memberi tahu UI.
    /// Kartu dianggap hilang bila tidak dilaporkan selama lostGraceSeconds (mencegah kedip).
    /// </summary>
    public class ARCardManager : MonoBehaviour
    {
        public static ARCardManager Instance { get; private set; }

        [SerializeField] ModelPool pool;
        [SerializeField] float lostGraceSeconds = 0.5f;
        [SerializeField] float hoverHeight = 0.03f;
        [SerializeField] int maxCards = 5;

        public event Action Changed;

        class CardState
        {
            public string key;
            public InstrumentData data;
            public Transform anchor;
            public float lastSeen;
            public float since;
            public bool visible;
            public CardModelView view;
        }

        readonly Dictionary<string, CardState> cards = new Dictionary<string, CardState>();
        readonly List<InstrumentData> detected = new List<InstrumentData>();
        readonly List<InstrumentData> audible = new List<InstrumentData>();
        readonly HashSet<string> warned = new HashSet<string>();
        bool dirty;
        bool merge = true;

        public IReadOnlyList<InstrumentData> Detected => detected;
        public int DetectedCount => detected.Count;
        public InstrumentData Latest { get; private set; }
        public bool MergeMode => merge;
        public int MaxCards => maxCards;

        void Awake()
        {
            Instance = this;
            merge = GameSettings.MergeCards;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetMergeMode(bool value)
        {
            if (merge == value) return;
            merge = value;
            dirty = true;
        }

        /// <summary>Dipanggil tiap frame selama kartu terlihat.</summary>
        public void ReportTracked(string imageName, Transform anchor)
        {
            if (anchor == null || string.IsNullOrEmpty(imageName)) return;
            if (!cards.TryGetValue(imageName, out var c))
            {
                var db = InstrumentDatabase.Load();
                var data = db != null ? db.GetByImageName(imageName) : null;
                if (data == null)
                {
                    if (warned.Add(imageName)) Debug.LogWarning("[GamelAR] Kartu tak dikenal: " + imageName);
                    return;
                }
                int visibleCount = 0;
                foreach (var x in cards.Values) if (x.visible) visibleCount++;
                if (visibleCount >= maxCards) return;
                c = new CardState { key = imageName, data = data };
                cards[imageName] = c;
            }

            c.anchor = anchor;
            c.lastSeen = Time.time;
            if (!c.visible)
            {
                if (c.view == null) c.view = pool != null ? pool.Get(c.data) : null;
                if (c.view != null)
                {
                    c.view.Show();
                    if (!c.view.gameObject.activeSelf) c.view.gameObject.SetActive(true);
                    c.view.transform.SetPositionAndRotation(anchor.position + anchor.up * hoverHeight, anchor.rotation);
                }
                c.visible = true;
                c.since = Time.time;
                dirty = true;
                Debug.Log("[GamelAR] Kartu terdeteksi: " + c.data.displayName);
#if UNITY_ANDROID && !UNITY_EDITOR
                if (GameSettings.VibrateOn) Handheld.Vibrate();
#endif
            }
        }

        void Update()
        {
            float now = Time.time;
            float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f);

            foreach (var c in cards.Values)
            {
                if (c.visible)
                {
                    if (c.anchor == null || now - c.lastSeen > lostGraceSeconds)
                    {
                        c.visible = false;
                        if (c.view != null) c.view.Hide();
                        dirty = true;
                        Debug.Log("[GamelAR] Kartu hilang: " + c.data.displayName);
                    }
                    else if (c.view != null)
                    {
                        var t = c.view.transform;
                        var targetPos = c.anchor.position + c.anchor.up * hoverHeight;
                        t.SetPositionAndRotation(Vector3.Lerp(t.position, targetPos, k),
                                                 Quaternion.Slerp(t.rotation, c.anchor.rotation, k));
                    }
                }
                else if (c.view != null && c.view.FullyHidden)
                {
                    pool.Release(c.view);
                    c.view = null;
                }
            }

            if (dirty) { dirty = false; Recompute(); }
        }

        void Recompute()
        {
            detected.Clear();
            audible.Clear();
            CardState newest = null;
            foreach (var c in cards.Values)
            {
                if (!c.visible) continue;
                detected.Add(c.data);
                if (newest == null || c.since > newest.since) newest = c;
            }
            // urut stabil agar teks UI tidak berganti-ganti
            detected.Sort((a, b) => string.CompareOrdinal(a.displayName, b.displayName));
            Latest = newest != null ? newest.data : null;

            if (merge) audible.AddRange(detected);
            else if (newest != null) audible.Add(newest.data);

            if (AudioMixerManager.Instance != null) AudioMixerManager.Instance.SetTargets(audible);
            Changed?.Invoke();
        }
    }
}
