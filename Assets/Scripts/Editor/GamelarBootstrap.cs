using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace GamelAR.EditorTools
{
    /// <summary>
    /// Menjalankan setup proyek secara bertahap dan otomatis (aman terhadap domain reload).
    /// Tahap: 0 folder+Player Settings -> 1 pasang package AR -> 2 tunggu package -> 3 define GAMELAR_ARF
    ///        -> 4 ARCore loader -> 5 bangun data+scene -> 6 switch ke Android -> 7 impor TMP -> 8 font TMP -> 9 bangun semua -> 10 selesai.
    /// </summary>
    [InitializeOnLoad]
    public static class GamelarBootstrap
    {
        const int Done = 10;
        static AddAndRemoveRequest request;
        static double nextCheck;
        static int waitTicks;

        static string Key(string k) => "GamelAR." + Application.dataPath + "." + k;
        static int Stage { get => EditorPrefs.GetInt(Key("stage"), 0); set => EditorPrefs.SetInt(Key("stage"), value); }
        static int Attempts { get => EditorPrefs.GetInt(Key("attempts"), 0); set => EditorPrefs.SetInt(Key("attempts"), value); }

        static GamelarBootstrap()
        {
            // Migrasi: versi 2 menambah tahap TMP + scene baru; jalankan ulang dari tahap 5.
            if (EditorPrefs.GetInt(Key("ver"), 0) < 2)
            {
                EditorPrefs.SetInt(Key("ver"), 2);
                if (Stage > 5) Stage = 5;
            }
            if (Stage < Done) EditorApplication.update += Poll;
        }

        static bool PackagesInstalled =>
            UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.xr.arfoundation") != null &&
            UnityEditor.PackageManager.PackageInfo.FindForPackageName("com.unity.xr.arcore") != null;

        static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 1.0;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || EditorUtility.scriptCompilationFailed) return;

            try { Step(); }
            catch (Exception e)
            {
                Debug.LogError("[GamelAR] Setup gagal pada tahap " + Stage + ": " + e);
                Stage = Stage + 1; // lanjut agar tidak macet; jalankan ulang lewat menu bila perlu
            }
            if (Stage >= Done) EditorApplication.update -= Poll;
        }

        static void Step()
        {
            switch (Stage)
            {
                case 0:
                    Debug.Log("[GamelAR] Tahap 0: membuat folder dan Player Settings...");
                    GamelarProjectSetup.CreateFolders();
                    GamelarProjectSetup.ApplyPlayerSettings();
                    Stage = 1;
                    break;

                case 1:
                    if (PackagesInstalled) { Stage = 3; break; }
                    Debug.Log("[GamelAR] Tahap 1: memasang AR Foundation + ARCore (tunggu sebentar)...");
                    request = Client.AddAndRemove(new[] { "com.unity.xr.arfoundation", "com.unity.xr.arcore" }, null);
                    Attempts = Attempts + 1;
                    Stage = 2;
                    break;

                case 2:
                    if (PackagesInstalled) { Stage = 3; break; }
                    if (request != null && request.IsCompleted)
                    {
                        if (request.Status == StatusCode.Failure)
                        {
                            Debug.LogError("[GamelAR] Gagal memasang package AR: " + request.Error.message +
                                           "\nPeriksa koneksi internet, lalu pakai menu GamelAR > Setup Ulang.");
                            Stage = Done;
                            break;
                        }
                        request = null;
                    }
                    else if (request == null && Attempts < 4) Stage = 1; // request hilang karena domain reload
                    else if (request == null) { Debug.LogError("[GamelAR] Package AR belum terpasang. Buka Window > Package Manager atau jalankan GamelAR > Setup Ulang."); Stage = Done; }
                    break;

                case 3:
                    if (GamelarProjectSetup.EnsureDefine(GamelarProjectSetup.ArDefine))
                    {
                        Debug.Log("[GamelAR] Tahap 3: mengaktifkan kode AR (define " + GamelarProjectSetup.ArDefine + ")...");
                        Stage = 4; // domain reload akan terjadi
                    }
                    else Stage = 4;
                    break;

                case 4:
#if GAMELAR_ARF
                    Debug.Log("[GamelAR] Tahap 4: mengaktifkan ARCore loader...");
                    GamelarXRSetup.EnableARCoreLoader();
                    Stage = 5;
#else
                    // define belum aktif; tunggu kompilasi ulang
#endif
                    break;

                case 5:
                    Debug.Log("[GamelAR] Tahap 5: mengimpor TextMeshPro Essentials...");
                    GamelarTmpSetup.ImportEssentials();
                    Stage = 6;
                    break;

                case 6:
                    if (!GamelarTmpSetup.EssentialsPresent)
                    {
                        waitTicks++;
                        if (waitTicks < 90) break; // tunggu impor (maks ~90 detik)
                        Debug.LogWarning("[GamelAR] TMP Essentials belum muncul; lanjut tanpa menunggu.");
                    }
                    waitTicks = 0;
                    GamelarTmpSetup.CreateFonts();
                    Stage = 7;
                    break;

                case 7:
                    Debug.Log("[GamelAR] Tahap 7: membangun data instrumen dan semua scene...");
                    GamelarMenu.BuildAllInternal();
                    Stage = 8;
                    break;

                case 8:
                    GamelarProjectSetup.SwitchToAndroid();
                    Stage = Done;
                    Debug.Log("[GamelAR] Setup selesai. Lihat menu GamelAR di bagian atas Unity.");
                    break;
            }
        }

        public static void Restart()
        {
            Stage = 0;
            Attempts = 0;
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }
    }

    public static class GamelarMenu
    {
        [MenuItem("GamelAR/Setup Ulang (Package AR + Player Settings)")]
        static void Restart() => GamelarBootstrap.Restart();

        [MenuItem("GamelAR/Bangun Semua (Data + Scene)")]
        static void BuildAll() => BuildAllInternal();

        [MenuItem("GamelAR/Bangun Scene Beranda")]
        static void BuildBeranda() { BerandaBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Bangun Scene Splash")]
        static void BuildSplash() { SplashBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Bangun Scene Pemindai AR")]
        static void BuildPemindai() { PemindaiBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Bangun Scene Informasi")]
        static void BuildInfo() { InfoLarasBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Bangun Scene Pengaturan")]
        static void BuildPengaturan() { PengaturanBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Bangun Scene Tentang")]
        static void BuildTentang() { TentangBuilder.Build(); GamelarProjectSetup.ApplyBuildSettingsScenes(); }

        [MenuItem("GamelAR/Impor TMP + Buat Font")]
        static void Tmp() { GamelarTmpSetup.ImportEssentials(); GamelarTmpSetup.CreateFonts(); }

        [MenuItem("GamelAR/Pasang Renderer Feature Kamera AR")]
        static void ArFeature() => GamelarTmpSetup.EnsureARBackgroundFeature();

        [MenuItem("GamelAR/Buat Ulang Thumbnail Instrumen")]
        static void Thumbs()
        {
            var db = GamelarDataBuilder.BuildAll();
            GamelarThumbnails.RenderAll(db, true);
            GamelarDataBuilder.BuildAll();
        }

        [MenuItem("GamelAR/Buat Image Library Kartu AR")]
        static void ImageLib()
        {
#if GAMELAR_ARF
            GamelarXRSetup.BuildImageLibrary();
#else
            Debug.LogWarning("[GamelAR] Package AR belum terpasang. Jalankan GamelAR > Setup Ulang terlebih dahulu.");
#endif
        }

        public static void BuildAllInternal()
        {
            AssetDatabase.Refresh();
            GamelarTmpSetup.CreateFonts();               // 0. font TMP (jika belum ada)
            var db = GamelarDataBuilder.BuildAll();      // 1. data (model terpasang)
            GamelarThumbnails.RenderAll(db);             // 2. thumbnail dari model
            GamelarDataBuilder.BuildAll();               // 3. pasang thumbnail ke data
            GamelarTmpSetup.EnsureLarasData();           // 4. data nada laras (placeholder)
#if GAMELAR_ARF
            GamelarXRSetup.BuildImageLibrary();          // 5. library kartu
            GamelarTmpSetup.EnsureARBackgroundFeature(); // 6. kamera AR di URP
#endif
            SplashBuilder.Build();                       // 7. scene-scene
            BerandaBuilder.Build();
            PemindaiBuilder.Build();
            InfoLarasBuilder.Build();
            PengaturanBuilder.Build();
            TentangBuilder.Build();
            GamelarProjectSetup.ApplyBuildSettingsScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[GamelAR] Bangun Semua selesai.");
        }
    }
}
