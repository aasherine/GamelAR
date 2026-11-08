using TMPro;
using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Satu model 3D yang melayang di atas kartu: dinormalkan ukurannya, berputar pelan, naik-turun,
    /// muncul/menghilang dengan animasi skala, dan membawa label nama yang selalu menghadap kamera.
    /// Objek ini dipakai ulang oleh ModelPool (object pooling).
    /// </summary>
    public class CardModelView : MonoBehaviour
    {
        public InstrumentData Data { get; private set; }
        public string PoolKey { get; set; }
        public bool FullyHidden => !shown && scale01 <= 0.001f;

        Transform spinPivot;
        Transform nameTag;
        float height;
        float scale01;
        bool shown;
        float phase;
        float spinSpeed = 28f;
        float yaw;

        public void Init(InstrumentData data, GameObject prefab, TMP_FontAsset font)
        {
            Data = data;
            phase = Random.value * 6.28f;
            if (spinPivot != null) return;

            spinPivot = new GameObject("Spin").transform;
            spinPivot.SetParent(transform, false);
            var model = Instantiate(prefab, spinPivot);
            model.name = "Model_" + data.id;
            model.transform.localPosition = Vector3.zero;
            // FBX dari Blender menyimpan koreksi sumbu (sekitar -90 derajat di X) pada objek akarnya.
            // Rotasi itu harus dipertahankan; kalau di-reset ke nol, model tergeletak miring dan
            // putarannya tampak vertikal, bukan memutar di atas kartu.
            model.transform.localRotation = prefab.transform.localRotation;
            model.transform.localScale = Vector3.one;
            foreach (var c in model.GetComponentsInChildren<Collider>()) Destroy(c);

            // Hitung ukuran, skalakan agar sisi terpanjang = arSizeMeter, pusatkan, dasar di y=0
            var rends = model.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float k = maxDim > 0.0001f ? Mathf.Max(0.01f, data.arSizeMeter) / maxDim : 1f;
                model.transform.localScale = Vector3.one * k;
                // bounds dalam koordinat dunia -> jadikan relatif terhadap titik putar (pool tidak selalu di titik nol)
                Vector3 o = spinPivot.position;
                model.transform.localPosition = new Vector3(-(b.center.x - o.x) * k, -(b.min.y - o.y) * k, -(b.center.z - o.z) * k);
                height = b.size.y * k;
            }
            else height = data.arSizeMeter;

            // Label nama 3D sengaja tidak dibuat lagi: tulisan itu menimpa model dan mengganggu.
            // Nama instrumen yang terdeteksi sudah tampil di panel bawah layar Pindai (ScannerUI).
        }

        public void Show()
        {
            gameObject.SetActive(true);
            shown = true;
        }

        public void Hide() => shown = false;

        public void ResetState()
        {
            scale01 = 0f;
            shown = false;
            yaw = 0f;
            transform.localScale = Vector3.zero;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float target = shown ? 1f : 0f;
            scale01 = Mathf.MoveTowards(scale01, target, dt * (shown ? 3.2f : 4.5f));
            // Pop-in dengan sedikit overshoot
            float eased = shown ? EaseOutBack(scale01) : scale01 * scale01;
            transform.localScale = Vector3.one * Mathf.Max(0f, eased);

            yaw += spinSpeed * dt;
            if (spinPivot != null)
            {
                float bob = Mathf.Sin(Time.time * 1.8f + phase) * 0.006f;
                spinPivot.localPosition = new Vector3(0f, bob, 0f);
                spinPivot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }

            if (nameTag != null)
            {
                var cam = Camera.main;
                if (cam != null) nameTag.rotation = Quaternion.LookRotation(nameTag.position - cam.transform.position, Vector3.up);
            }
        }

        static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            x = Mathf.Clamp01(x) - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
