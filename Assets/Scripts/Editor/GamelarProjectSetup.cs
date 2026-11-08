using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace GamelAR.EditorTools
{
    /// <summary>Folder, Player Settings Android, dan Build Settings.</summary>
    public static class GamelarProjectSetup
    {
        public const string ArDefine = "GAMELAR_ARF";

        static readonly string[] Folders =
        {
            "Assets/Scenes", "Assets/Scripts", "Assets/Prefabs", "Assets/UI", "Assets/Models",
            "Assets/Materials", "Assets/Audio", "Assets/ScriptableObjects", "Assets/Resources",
            "Assets/Cards", "Assets/Fonts", "Assets/Fonts/TMP"
        };

        public static void CreateFolders()
        {
            foreach (var f in Folders) Directory.CreateDirectory(f);
            AssetDatabase.Refresh();
        }

        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "AASA";
            PlayerSettings.productName = "GamelAR";
            PlayerSettings.bundleVersion = "1.0.0";

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, "com.aasa.gamelar");
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // syarat ARCore
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            EditorUserBuildSettings.buildAppBundle = false; // APK
            Debug.Log("[GamelAR] Player Settings Android diterapkan (Portrait, IL2CPP, ARM64, com.aasa.gamelar, API 24+).");
        }

        public static bool HasDefine(NamedBuildTarget t, string define)
        {
            var defs = PlayerSettings.GetScriptingDefineSymbols(t).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            return System.Array.IndexOf(defs, define) >= 0;
        }

        public static bool EnsureDefine(string define)
        {
            bool changed = false;
            foreach (var t in new[] { NamedBuildTarget.Android, NamedBuildTarget.Standalone })
            {
                var defs = PlayerSettings.GetScriptingDefineSymbols(t).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (System.Array.IndexOf(defs, define) >= 0) continue;
                var list = new List<string>(defs) { define };
                PlayerSettings.SetScriptingDefineSymbols(t, string.Join(";", list.ToArray()));
                changed = true;
            }
            return changed;
        }

        public static void ApplyBuildSettingsScenes()
        {
            string[] order =
            {
                "Splash", "Beranda", "PemindaiAR", "InfoLaras", "Pengaturan", "Tentang"
            };
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var n in order)
            {
                string p = $"Assets/Scenes/{n}.unity";
                if (File.Exists(p)) scenes.Add(new EditorBuildSettingsScene(p, true));
            }
            if (scenes.Count > 0) EditorBuildSettings.scenes = scenes.ToArray();
            var splash = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Splash.unity");
            if (splash != null) UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = splash; // Play selalu mulai dari Splash
            Debug.Log($"[GamelAR] Build Settings: {scenes.Count} scene terdaftar.");
        }

        public static void SwitchToAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) return;
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                Debug.LogWarning("[GamelAR] Modul Android Build Support belum terpasang. Pasang lewat Unity Hub > Installs > Add modules.");
                return;
            }
            Debug.Log("[GamelAR] Beralih build target ke Android (butuh beberapa menit)...");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
    }
}
