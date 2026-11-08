#if GAMELAR_ARF
using System.IO;
using UnityEditor;
using UnityEditor.XR.ARSubsystems;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace GamelAR.EditorTools
{
    /// <summary>Mengaktifkan ARCore di XR Plug-in Management dan membuat Reference Image Library dari kartu.</summary>
    public static class GamelarXRSetup
    {
        public const string LibraryPath = "Assets/ScriptableObjects/GamelarImageLibrary.asset";
        const string SettingsKey = "com.unity.xr.management.loader_settings";
        const string SettingsAssetPath = "Assets/XR/XRGeneralSettingsPerBuildTarget.asset";

        // Ukuran kartu A6 dalam meter
        public static readonly Vector2 CardSizeMeters = new Vector2(0.105f, 0.148f);

        public static void EnableARCoreLoader()
        {
            Directory.CreateDirectory("Assets/XR");
            XRGeneralSettingsPerBuildTarget perTarget = null;
            EditorBuildSettings.TryGetConfigObject(SettingsKey, out perTarget);
            if (perTarget == null)
            {
                perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(SettingsAssetPath);
                if (perTarget == null)
                {
                    perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                    AssetDatabase.CreateAsset(perTarget, SettingsAssetPath);
                }
                EditorBuildSettings.AddConfigObject(SettingsKey, perTarget, true);
            }

            var group = BuildTargetGroup.Android;
            if (!perTarget.HasSettingsForBuildTarget(group)) perTarget.CreateDefaultSettingsForBuildTarget(group);
            if (!perTarget.HasManagerSettingsForBuildTarget(group)) perTarget.CreateDefaultManagerSettingsForBuildTarget(group);

            var general = perTarget.SettingsForBuildTarget(group);
            var manager = perTarget.ManagerSettingsForBuildTarget(group);
            if (general != null)
            {
                // Dijalankan manual di scene PemindaiAR agar kamera tidak menyala di halaman lain.
                general.InitManagerOnStart = false;
            }

            bool ok = XRPackageMetadataStore.AssignLoader(manager, "UnityEngine.XR.ARCore.ARCoreLoader", group);
            EditorUtility.SetDirty(perTarget);
            AssetDatabase.SaveAssets();
            Debug.Log(ok ? "[GamelAR] ARCore loader aktif untuk Android."
                         : "[GamelAR] Gagal mengaktifkan ARCore loader. Cek Project Settings > XR Plug-in Management > Android > ARCore.");
        }

        public static void BuildImageLibrary()
        {
            var cards = new System.Collections.Generic.List<Texture2D>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D Kartu_", new[] { "Assets/Cards" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (!Path.GetFileNameWithoutExtension(p).StartsWith("Kartu_")) continue;
                PrepareCardTexture(p);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (tex != null) cards.Add(tex);
            }
            cards.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            AssetDatabase.DeleteAsset(LibraryPath);
            var lib = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            AssetDatabase.CreateAsset(lib, LibraryPath);

            foreach (var tex in cards)
            {
                lib.Add();
                int i = lib.count - 1;
                lib.SetName(i, tex.name);
                lib.SetTexture(i, tex, true);
                lib.SetSpecifySize(i, true);
                lib.SetSize(i, CardSizeMeters);
            }
            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GamelAR] Image Library dibuat dengan {cards.Count} kartu: {LibraryPath}");
        }

        static void PrepareCardTexture(string path)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) return;
            bool dirty = false;
            if (imp.textureType != TextureImporterType.Default) { imp.textureType = TextureImporterType.Default; dirty = true; }
            if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
            if (!imp.isReadable) { imp.isReadable = true; dirty = true; }
            if (imp.mipmapEnabled) { imp.mipmapEnabled = false; dirty = true; }
            if (imp.npotScale != TextureImporterNPOTScale.None) { imp.npotScale = TextureImporterNPOTScale.None; dirty = true; }
            if (dirty) imp.SaveAndReimport();
        }
    }
}
#endif
