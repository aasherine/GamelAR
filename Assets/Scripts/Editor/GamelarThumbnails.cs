using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamelAR.EditorTools
{
    /// <summary>Membuat gambar mini (thumbnail) tiap instrumen dari model 3D-nya.</summary>
    public static class GamelarThumbnails
    {
        const string Dir = "Assets/UI/Thumbs";

        public static void RenderAll(InstrumentDatabase db, bool force = false)
        {
            Directory.CreateDirectory(Dir);
            foreach (var d in db.instruments)
            {
                if (d == null) continue;
                string path = $"{Dir}/{d.id}_set.png";
                if (!force && File.Exists(path)) { AssignSprite(d, path); continue; }
                var model = d.modelDetail != null ? d.modelDetail : d.modelStandar;   // model set lengkap
                if (model == null) continue;
                try
                {
                    var tex = RenderTransparent(model, 512, 384);
                    if (tex == null) continue;
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    Object.DestroyImmediate(tex);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                    AssignSprite(d, path);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[GamelAR] Gagal membuat thumbnail {d.id}: {e.Message}");
                }
            }
            AssetDatabase.SaveAssets();
        }

        static void AssignSprite(InstrumentData d, string path)
        {
            var s = UIKit.LoadSprite(path);
            if (s == null) return;
            d.thumbnail = s;
            EditorUtility.SetDirty(d);
        }

        /// <summary>Render dua kali (latar hitam & putih) lalu hitung alpha, agar latar benar-benar transparan.</summary>
        static Texture2D RenderTransparent(GameObject prefab, int w, int h)
        {
            var tb = Render(prefab, w, h, Color.black);
            var tw = Render(prefab, w, h, Color.white);
            if (tb == null || tw == null) return null;
            var cb = tb.GetPixels32(); var cw = tw.GetPixels32();
            var res = new Color32[cb.Length];
            for (int i = 0; i < cb.Length; i++)
            {
                float diff = ((cw[i].r - cb[i].r) + (cw[i].g - cb[i].g) + (cw[i].b - cb[i].b)) / (3f * 255f);
                float a = Mathf.Clamp01(1f - diff);
                if (a < 0.02f) { res[i] = new Color32(0, 0, 0, 0); continue; }
                res[i] = new Color32(
                    (byte)Mathf.Clamp(cb[i].r / a, 0, 255),
                    (byte)Mathf.Clamp(cb[i].g / a, 0, 255),
                    (byte)Mathf.Clamp(cb[i].b / a, 0, 255),
                    (byte)Mathf.RoundToInt(a * 255f));
            }
            Object.DestroyImmediate(tb); Object.DestroyImmediate(tw);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.SetPixels32(res);
            tex.Apply();
            return tex;
        }

        static Texture2D Render(GameObject prefab, int w, int h, Color bg)
        {
            var pru = new PreviewRenderUtility();
            try
            {
                var go = Object.Instantiate(prefab);
                pru.AddSingleGO(go);

                var renderers = go.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) return null;
                var b = renderers[0].bounds;
                foreach (var r in renderers) b.Encapsulate(r.bounds);

                var cam = pru.camera;
                cam.fieldOfView = 28f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = bg;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 1000f;
                float radius = Mathf.Max(b.extents.magnitude, 0.01f);
                float dist = radius / Mathf.Sin(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 0.85f;
                cam.transform.position = b.center + Quaternion.Euler(18f, -32f, 0f) * (Vector3.back * dist);
                cam.transform.LookAt(b.center);

                pru.lights[0].intensity = 1.3f;
                pru.lights[0].transform.rotation = Quaternion.Euler(40f, 35f, 0f);
                pru.lights[1].intensity = 0.8f;
                pru.ambientColor = new Color(0.55f, 0.52f, 0.5f, 1f);

                pru.BeginStaticPreview(new Rect(0, 0, w, h));
                cam.Render();
                return pru.EndStaticPreview();
            }
            finally
            {
                pru.Cleanup();
            }
        }
    }
}
