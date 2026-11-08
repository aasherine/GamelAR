using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamelAR.EditorTools
{
    /// <summary>Impor TMP Essential Resources, buat font TextMeshPro, aset LarasData, dan renderer feature kamera AR (URP).</summary>
    public static class GamelarTmpSetup
    {
        public const string SettingsAsset = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        public static bool EssentialsPresent => File.Exists(SettingsAsset);

        public static void ImportEssentials()
        {
            if (EssentialsPresent) return;
            var info = UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.ugui");
            if (info == null) { Debug.LogError("[GamelAR] Paket com.unity.ugui tidak ditemukan."); return; }
            string pkg = Path.Combine(info.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
            if (!File.Exists(pkg)) { Debug.LogError("[GamelAR] TMP Essential Resources tidak ditemukan di " + pkg); return; }
            Debug.Log("[GamelAR] Mengimpor TMP Essential Resources...");
            AssetDatabase.ImportPackage(pkg, false);
        }

        public static void CreateFonts()
        {
            UIKit.CreateFontAssets();
        }

        public static void EnsureLarasData()
        {
            const string path = "Assets/Resources/LarasData.asset";
            Directory.CreateDirectory("Assets/Resources");
            if (AssetDatabase.LoadAssetAtPath<LarasData>(path) != null) return;
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LarasData>(), path);
            AssetDatabase.SaveAssets();
        }

        /// <summary>Menambahkan ARBackgroundRendererFeature ke semua URP Renderer agar gambar kamera AR tampil.</summary>
        public static void EnsureARBackgroundFeature()
        {
            Type featureType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                featureType = asm.GetType("UnityEngine.XR.ARFoundation.ARBackgroundRendererFeature");
                if (featureType != null) break;
            }
            if (featureType == null)
            {
                Debug.LogWarning("[GamelAR] ARBackgroundRendererFeature belum tersedia (package AR belum terpasang).");
                return;
            }

            int added = 0;
            var guids = AssetDatabase.FindAssets("t:ScriptableRendererData");
            Debug.Log("[GamelAR] Renderer URP ditemukan: " + guids.Length + ", tipe feature: " + featureType.FullName);
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var rd = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.ScriptableRendererData>(path);
                if (rd == null) continue;
                bool has = false;
                foreach (var f in rd.rendererFeatures) if (f != null && f.GetType() == featureType) has = true;
                if (has) { Debug.Log("[GamelAR] Sudah ada di " + path); continue; }

                var feature = (ScriptableObject)ScriptableObject.CreateInstance(featureType);
                feature.name = "ARBackgroundRendererFeature";
                AssetDatabase.AddObjectToAsset(feature, rd);

                var so = new SerializedObject(rd);
                var list = so.FindProperty("m_RendererFeatures");
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = feature;
                var map = so.FindProperty("m_RendererFeatureMap");
                if (map != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string g, out long local))
                {
                    map.arraySize++;
                    map.GetArrayElementAtIndex(map.arraySize - 1).longValue = local;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(rd);
                added++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[GamelAR] ARBackgroundRendererFeature ditambahkan ke " + added + " renderer.");
        }
    }
}
