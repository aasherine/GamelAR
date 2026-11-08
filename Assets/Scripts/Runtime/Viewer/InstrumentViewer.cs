using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>
    /// Penampil 3D di halaman Informasi: model dirender oleh kamera khusus ke RenderTexture
    /// yang ditampilkan di RawImage. Geser untuk memutar, cubit (2 jari) atau scroll mouse untuk zoom.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class InstrumentViewer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler
    {
        [SerializeField] Camera viewCamera;
        [SerializeField] Transform stage;
        [SerializeField] float rotateSpeed = 0.4f;
        [SerializeField] float autoSpinSpeed = 16f;
        [SerializeField] Vector2 zoomRange = new Vector2(0.6f, 3f);

        RawImage raw;
        RenderTexture rt;
        Transform pivot;
        GameObject current;
        float yaw, pitch, zoom = 1f, baseDist = 3f, idle;
        int downCount;
        bool hovering;
        float lastPinch;

        const float DefaultYaw = 28f, DefaultPitch = 12f;

        void Awake()
        {
            raw = GetComponent<RawImage>();
            pivot = new GameObject("ViewerPivot").transform;
            pivot.SetParent(stage != null ? stage : transform, false);
            pivot.localPosition = Vector3.zero;
        }

        Vector2 rtForSize;

        void Start() => EnsureTarget();

        /// <summary>Buat (ulang) RenderTexture bila ukuran panel berubah, agar model tidak gepeng/terpotong.</summary>
        void EnsureTarget()
        {
            var r = raw.rectTransform.rect;
            if (r.width < 1f || r.height < 1f) return;
            if (rt != null && Mathf.Abs(r.width - rtForSize.x) < 1f && Mathf.Abs(r.height - rtForSize.y) < 1f) return;
            rtForSize = r.size;

            int w = 1024;
            int h = Mathf.Clamp(Mathf.RoundToInt(w * (r.height / r.width)), 256, 1536);
            if (rt != null) { viewCamera.targetTexture = null; rt.Release(); Destroy(rt); }
            // antiAliasing harus 1: URP proyek ini memakai MSAA mati. Target 2 sampel memicu error
            // "Attachment 0 was created with 1 samples but 2 samples were requested" (kotak teks merah).
            rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            rt.Create();
            viewCamera.targetTexture = rt;
            viewCamera.aspect = (float)w / h;
            raw.texture = rt;
            raw.color = Color.white;
        }

        void OnDestroy()
        {
            if (viewCamera != null) viewCamera.targetTexture = null;
            if (rt != null) { rt.Release(); Destroy(rt); }
        }

        public void SetInstrument(InstrumentData d)
        {
            if (current != null) Destroy(current);
            current = null;
            if (d == null) return;
            var prefab = d.GetModel(GameSettings.UseDetailModel);
            if (prefab == null) return;

            current = Instantiate(prefab, pivot);
            current.transform.localPosition = Vector3.zero;
            current.transform.localRotation = prefab.transform.localRotation;   // pertahankan koreksi sumbu FBX Blender
            current.transform.localScale = Vector3.one;
            foreach (var c in current.GetComponentsInChildren<Collider>()) Destroy(c);

            var rends = current.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                pivot.localRotation = Quaternion.identity;
                var b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float k = maxDim > 0.0001f ? 1f / maxDim : 1f;
                current.transform.localScale = Vector3.one * k;
                current.transform.localPosition = -(b.center - pivot.position) * k;
            }
            float fov = viewCamera.fieldOfView * Mathf.Deg2Rad;
            baseDist = 0.5f / Mathf.Tan(fov * 0.5f) * 1.2f + 0.5f;
            ResetView();
        }

        public void ResetView()
        {
            yaw = DefaultYaw; pitch = DefaultPitch; zoom = 1f; idle = 0f;
            ApplyCamera();
        }

        void Update()
        {
            EnsureTarget();
            idle += Time.unscaledDeltaTime;
            HandlePinch();
            if (idle > 2.5f && downCount == 0) yaw += autoSpinSpeed * Time.unscaledDeltaTime;
            ApplyCamera();
        }

        void HandlePinch()
        {
            var ts = Touchscreen.current;
            if (ts != null && downCount > 0)
            {
                Vector2 a = Vector2.zero, b = Vector2.zero;
                int n = 0;
                for (int i = 0; i < ts.touches.Count && n < 2; i++)
                {
                    var t = ts.touches[i];
                    if (!t.press.isPressed) continue;
                    if (n == 0) a = t.position.ReadValue(); else b = t.position.ReadValue();
                    n++;
                }
                if (n >= 2)
                {
                    float d = Vector2.Distance(a, b);
                    if (lastPinch > 1f) zoom = Mathf.Clamp(zoom * (d / lastPinch), zoomRange.x, zoomRange.y);
                    lastPinch = d;
                    idle = 0f;
                    return;
                }
            }
            lastPinch = 0f;

            var mouse = Mouse.current;
            if (mouse != null && hovering)
            {
                float s = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(s) > 0.01f)
                {
                    zoom = Mathf.Clamp(zoom * (1f + Mathf.Sign(s) * 0.08f), zoomRange.x, zoomRange.y);
                    idle = 0f;
                }
            }
        }

        void ApplyCamera()
        {
            if (viewCamera == null || stage == null) return;
            // Putar dulu terhadap sumbu tegak model (yaw), baru miringkan terhadap sumbu kamera (pitch).
            // Quaternion.Euler(pitch, yaw, 0) memiringkan model lalu memutarnya pada sumbu dunia,
            // sehingga model yang dimiringkan tampak bergoyang/oleng saat berputar otomatis.
            pivot.localRotation = Quaternion.AngleAxis(pitch, Vector3.right) * Quaternion.AngleAxis(yaw, Vector3.up);
            viewCamera.transform.position = stage.position + new Vector3(0f, 0f, -baseDist / zoom);
            viewCamera.transform.rotation = Quaternion.identity;
        }

        public void OnPointerDown(PointerEventData e) { downCount++; idle = 0f; }
        public void OnPointerUp(PointerEventData e) { downCount = Mathf.Max(0, downCount - 1); idle = 0f; }
        public void OnPointerEnter(PointerEventData e) => hovering = true;
        public void OnPointerExit(PointerEventData e) => hovering = false;

        public void OnDrag(PointerEventData e)
        {
            if (lastPinch > 0f) return; // sedang mencubit
            yaw -= e.delta.x * rotateSpeed;
            pitch = Mathf.Clamp(pitch + e.delta.y * rotateSpeed, -60f, 60f);
            idle = 0f;
        }
    }
}
