using System;
using System.Collections;
using UnityEngine;
#if GAMELAR_VUFORIA
using Vuforia;
#endif
#if GAMELAR_ARF
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace GamelAR
{
    public enum ARState { Starting, NeedPermission, Unsupported, Ready, Simulation }

    /// <summary>
    /// Menyiapkan AR di scene PemindaiAR secara berurutan dan aman:
    /// 1) izin kamera
    /// 2) inisialisasi mesin AR:
    ///    - GAMELAR_VUFORIA : Vuforia Engine (Delayed Initialization harus aktif di Vuforia Configuration)
    ///    - GAMELAR_ARF     : AR Foundation + ARCore (jalur lama, dipertahankan)
    /// Di Unity Editor (tanpa kamera AR) otomatis masuk mode Simulasi.
    /// </summary>
    public class ARSessionBootstrap : MonoBehaviour
    {
        public static ARState State { get; private set; } = ARState.Starting;
        public static bool PermissionPermanentlyDenied { get; private set; }
        public static event Action<ARState> StateChanged;

        [Tooltip("Urutan penting: ARSession terakhir. (Jalur AR Foundation saja; tidak dipakai oleh Vuforia.)")]
        [SerializeField] Behaviour[] arComponents;
        [SerializeField] ARImageTrackingSource trackingSource;

        bool loaderStarted;
        bool running;

        void Start() => Begin();

        public void Begin()
        {
            if (running) return;
            StartCoroutine(Run());
        }

        void SetState(ARState s)
        {
            State = s;
            Debug.Log("[GamelAR] Status AR: " + s);
            StateChanged?.Invoke(s);
        }

        IEnumerator Run()
        {
            running = true;
            SetState(ARState.Starting);
            yield return null;

#if UNITY_EDITOR || !UNITY_ANDROID
            SetState(ARState.Simulation);
            running = false;
            yield break;
#else
            // 1) Izin kamera
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                bool answered = false, granted = false, dontAsk = false;
                var cb = new PermissionCallbacks();
                cb.PermissionGranted += _ => { answered = true; granted = true; };
                cb.PermissionDenied += _ => { answered = true; };
                cb.PermissionDeniedAndDontAskAgain += _ => { answered = true; dontAsk = true; };
                Permission.RequestUserPermission(Permission.Camera, cb);
                float t = 0f;
                while (!answered && t < 60f) { t += Time.unscaledDeltaTime; yield return null; }
                if (!granted && !Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    PermissionPermanentlyDenied = dontAsk;
                    SetState(ARState.NeedPermission);
                    running = false;
                    yield break;
                }
            }
            PermissionPermanentlyDenied = false;

#if GAMELAR_VUFORIA
            // 2) Vuforia Engine. Butuh "Delayed Initialization" aktif di Vuforia Configuration,
            //    supaya inisialisasi baru dimulai setelah izin kamera didapat.
            var vuforia = VuforiaApplication.Instance;
            if (vuforia == null) { SetState(ARState.Unsupported); running = false; yield break; }

            bool vfStarted = vuforia.IsRunning;
            bool vfFailed = false;
            VuforiaInitError vfError = VuforiaInitError.NONE;
            Action<VuforiaInitError> onInit = e =>
            {
                vfError = e;
                if (e != VuforiaInitError.NONE) vfFailed = true;
            };
            Action onStarted = () => { vfStarted = true; };
            vuforia.OnVuforiaInitialized += onInit;
            vuforia.OnVuforiaStarted += onStarted;

            if (!vuforia.IsInitialized && !vuforia.IsRunning) vuforia.Initialize();

            float vfWait = 0f;
            while (!vfStarted && !vfFailed && vfWait < 20f)
            {
                vfWait += Time.unscaledDeltaTime;
                yield return null;
            }
            vuforia.OnVuforiaInitialized -= onInit;
            vuforia.OnVuforiaStarted -= onStarted;

            if (!vfStarted)
            {
                // Jangan tampilkan License Key di log; hanya kode error.
                Debug.LogError("[GamelAR] Vuforia gagal mulai. Error: " + vfError + (vfFailed ? "" : " (timeout)"));
                SetState(ARState.Unsupported);
                running = false;
                yield break;
            }

            // Komponen AR Foundation tidak dipakai di jalur Vuforia.
            CreateVuforiaTargets();
            SetState(ARState.Ready);
#elif GAMELAR_ARF
            // 2) XR loader (ARCore)
            var mgr = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
            if (mgr == null) { SetState(ARState.Unsupported); running = false; yield break; }
            if (mgr.activeLoader == null)
            {
                yield return mgr.InitializeLoader();
                loaderStarted = mgr.activeLoader != null;
            }
            else loaderStarted = true;
            if (mgr.activeLoader == null) { SetState(ARState.Unsupported); running = false; yield break; }

            // 3) Ketersediaan ARCore
            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
                yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall)
                yield return ARSession.Install();
            if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.NeedsInstall)
            {
                SetState(ARState.Unsupported);
                running = false;
                yield break;
            }

            // 4) Aktifkan komponen AR
            foreach (var b in arComponents) if (b != null) b.enabled = true;
            if (trackingSource != null) trackingSource.Begin();
            SetState(ARState.Ready);
#else
            SetState(ARState.Unsupported);
#endif
            running = false;
#endif
        }

#if GAMELAR_VUFORIA
        const string VuforiaDatabase = "Vuforia/GamelAR_Kartu.xml";   // relatif terhadap StreamingAssets

        /// <summary>
        /// Scene hanya berisi satu ImageTarget (Bonang Barung). Kartu lain dibuat di sini dari database
        /// Vuforia yang sama, satu per instrumen, sehingga ke-15 kartu bisa dikenali.
        /// </summary>
        void CreateVuforiaTargets()
        {
            var vb = VuforiaBehaviour.Instance;
            var db = InstrumentDatabase.Load();
            if (vb == null || vb.ObserverFactory == null || db == null) return;

            var existing = new System.Collections.Generic.HashSet<string>();
            foreach (var ob in FindObjectsByType<ImageTargetBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!string.IsNullOrEmpty(ob.TargetName)) existing.Add(ob.TargetName);

            int made = 0;
            foreach (var d in db.instruments)
            {
                if (d == null || string.IsNullOrEmpty(d.referenceImageName) || existing.Contains(d.referenceImageName)) continue;
                try
                {
                    var target = vb.ObserverFactory.CreateImageTarget(VuforiaDatabase, d.referenceImageName);
                    if (target == null) { Debug.LogWarning("[GamelAR] Target Vuforia tidak ditemukan: " + d.referenceImageName); continue; }
                    target.gameObject.name = "ImageTarget_" + d.id;
                    FixTargetWidth(target);
                    target.gameObject.AddComponent<VuforiaCardTarget>();
                    existing.Add(d.referenceImageName);
                    made++;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[GamelAR] Gagal membuat target " + d.referenceImageName + ": " + e.Message);
                }
            }
            Debug.Log("[GamelAR] Target kartu Vuforia aktif: " + existing.Count + " (baru dibuat " + made + ")");
        }

        /// <summary>Lebar kartu cetak dalam meter (A6 = 10,5 x 14,8 cm), sama dengan ImageTarget Bonang di scene.</summary>
        const float CardWidthMeters = 0.105f;

        /// <summary>
        /// Pengaman: bila database masih menulis ukuran dalam sentimeter (10.5), target dianggap 100x lebih besar
        /// sehingga model 3D tampil terlalu jauh/kecil dan tidak terlihat. Samakan ke meter.
        /// </summary>
        static void FixTargetWidth(ImageTargetBehaviour target)
        {
            try
            {
                float w = target.GetSize().x;
                if (Mathf.Abs(w - CardWidthMeters) < 0.001f) return;
                target.enabled = false;           // ukuran hanya bisa diubah saat target nonaktif
                target.SetWidth(CardWidthMeters);
                target.enabled = true;
                Debug.Log("[GamelAR] Lebar target " + target.TargetName + " disesuaikan: " + w + " -> " + CardWidthMeters);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[GamelAR] Gagal menyesuaikan ukuran target " + target.TargetName + ": " + e.Message);
            }
        }
#endif

        public static void OpenAppSettings()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", Application.identifier, null))
                using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uri))
                {
                    activity.Call("startActivity", intent);
                }
            }
            catch (Exception e) { Debug.LogWarning("[GamelAR] Gagal membuka pengaturan: " + e.Message); }
#endif
        }

        void OnApplicationFocus(bool focus)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // Pengguna kembali dari layar pengaturan: coba lagi otomatis jika izin sudah diberikan.
            if (focus && State == ARState.NeedPermission && Permission.HasUserAuthorizedPermission(Permission.Camera)) Begin();
#endif
        }

        void OnDestroy()
        {
#if GAMELAR_ARF && !GAMELAR_VUFORIA
            if (arComponents != null)
                for (int i = arComponents.Length - 1; i >= 0; i--) if (arComponents[i] != null) arComponents[i].enabled = false;
            if (loaderStarted && XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                var mgr = XRGeneralSettings.Instance.Manager;
                if (mgr.isInitializationComplete)
                {
                    mgr.StopSubsystems();
                    mgr.DeinitializeLoader();
                }
            }
#endif
            State = ARState.Starting;
        }
    }
}
