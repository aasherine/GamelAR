using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Object pooling untuk model 3D instrumen. Model dibuat saat dibutuhkan, disembunyikan (bukan dihancurkan)
    /// ketika kartu hilang, dan dipakai ulang. Jumlah model idle dibatasi agar memori tetap kecil.
    /// </summary>
    public class ModelPool : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset tagFont;
        [SerializeField] int maxIdle = 5;

        readonly Dictionary<string, List<CardModelView>> idle = new Dictionary<string, List<CardModelView>>();
        readonly List<CardModelView> idleOrder = new List<CardModelView>();

        public CardModelView Get(InstrumentData data)
        {
            bool detail = GameSettings.UseDetailModel;
            string key = data.id + (detail ? "_d" : "_s");
            if (idle.TryGetValue(key, out var list) && list.Count > 0)
            {
                var v = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                idleOrder.Remove(v);
                if (v != null) { v.ResetState(); return v; }
            }

            var prefab = data.GetModel(detail);
            if (prefab == null)
            {
                Debug.LogWarning("[GamelAR] Model 3D belum ada untuk " + data.id);
                return null;
            }
            var go = new GameObject("Card_" + data.id);
            go.transform.SetParent(transform, false);
            var view = go.AddComponent<CardModelView>();
            view.PoolKey = key;
            view.Init(data, prefab, tagFont);
            view.ResetState();
            return view;
        }

        public void Release(CardModelView v)
        {
            if (v == null) return;
            v.Hide();
            v.gameObject.SetActive(false);
            v.transform.SetParent(transform, false);
            if (!idle.TryGetValue(v.PoolKey, out var list)) { list = new List<CardModelView>(); idle[v.PoolKey] = list; }
            list.Add(v);
            idleOrder.Add(v);
            while (idleOrder.Count > maxIdle)
            {
                var old = idleOrder[0];
                idleOrder.RemoveAt(0);
                if (old == null) continue;
                if (idle.TryGetValue(old.PoolKey, out var l)) l.Remove(old);
                Destroy(old.gameObject);
            }
        }
    }
}
