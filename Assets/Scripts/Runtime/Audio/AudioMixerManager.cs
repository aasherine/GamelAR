using System.Collections.Generic;
using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Mengelola suara loop banyak kartu sekaligus: kumpulan voice (AudioSource) yang dipakai ulang,
    /// fade in/out halus, penyelarasan fase loop (semua loop mulai dari titik waktu yang sama) dan
    /// penurunan volume otomatis agar campuran tidak pecah.
    /// </summary>
    public class AudioMixerManager : MonoBehaviour
    {
        public static AudioMixerManager Instance { get; private set; }

        [SerializeField] int maxVoices = 6;
        [SerializeField] float fadeInSeconds = 0.4f;
        [SerializeField] float fadeOutSeconds = 0.6f;

        class Voice
        {
            public AudioSource src;
            public InstrumentData data;
            public bool wanted;
            public float level;      // 0..1 sebelum dikalikan volume instrumen
        }

        readonly List<Voice> voices = new List<Voice>();
        double masterStartDsp;
        int lastWantedCount;

        public int ActiveVoices { get { int n = 0; foreach (var v in voices) if (v.data != null) n++; return n; } }

        void Awake()
        {
            Instance = this;
            for (int i = 0; i < maxVoices; i++)
            {
                var go = new GameObject("Voice" + i);
                go.transform.SetParent(transform, false);
                var s = go.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.loop = true;
                s.spatialBlend = 0f;
                s.volume = 0f;
                voices.Add(new Voice { src = s });
            }
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnApplicationPause(bool paused) => AudioListener.pause = paused;

        /// <summary>Tetapkan instrumen mana yang harus terdengar. Yang lain di-fade-out.</summary>
        public void SetTargets(IReadOnlyList<InstrumentData> wanted)
        {
            foreach (var v in voices) v.wanted = v.data != null && Contains(wanted, v.data);

            for (int i = 0; i < wanted.Count; i++)
            {
                var d = wanted[i];
                if (d == null || FindVoice(d) != null) continue;
                var v = TakeFreeVoice();
                if (v == null) break;
                StartVoice(v, d);
            }
            lastWantedCount = Mathf.Max(1, wanted.Count);
        }

        public void StopAll()
        {
            foreach (var v in voices) v.wanted = false;
        }

        static bool Contains(IReadOnlyList<InstrumentData> list, InstrumentData d)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == d) return true;
            return false;
        }

        Voice FindVoice(InstrumentData d)
        {
            foreach (var v in voices) if (v.data == d) return v;
            return null;
        }

        Voice TakeFreeVoice()
        {
            foreach (var v in voices) if (v.data == null) return v;
            Voice best = null;
            foreach (var v in voices) if (!v.wanted && (best == null || v.level < best.level)) best = v;
            return best;
        }

        void StartVoice(Voice v, InstrumentData d)
        {
            var clip = d.sampleLoop != null ? d.sampleLoop : PlaceholderAudio.GetLoop(d);
            if (ActiveVoices == 0) masterStartDsp = AudioSettings.dspTime;
            v.src.Stop();
            v.src.clip = clip;
            v.src.volume = 0f;
            v.level = 0f;
            double offset = (AudioSettings.dspTime - masterStartDsp) % clip.length;
            v.src.time = Mathf.Clamp((float)offset, 0f, Mathf.Max(0f, clip.length - 0.01f));
            v.src.Play();
            v.data = d;
            v.wanted = true;
            Debug.Log("[GamelAR] Audio mulai: " + d.displayName + (d.sampleLoop == null ? " (placeholder)" : ""));
        }

        void Update()
        {
            int wantedCount = 0;
            foreach (var v in voices) if (v.data != null && v.wanted) wantedCount++;
            // Semakin banyak suara, semakin kecil volume masing-masing (menghindari clipping)
            float gain = wantedCount <= 1 ? 1f : 1.15f / Mathf.Sqrt(wantedCount);

            foreach (var v in voices)
            {
                if (v.data == null) continue;
                if (v.wanted) v.level = Mathf.MoveTowards(v.level, 1f, Time.unscaledDeltaTime / fadeInSeconds);
                else v.level = Mathf.MoveTowards(v.level, 0f, Time.unscaledDeltaTime / fadeOutSeconds);

                v.src.volume = Mathf.SmoothStep(0f, 1f, v.level) * v.data.loopVolume * gain;

                if (!v.wanted && v.level <= 0.001f)
                {
                    v.src.Stop();
                    v.src.clip = null;
                    v.data = null;
                }
            }
        }
    }
}
