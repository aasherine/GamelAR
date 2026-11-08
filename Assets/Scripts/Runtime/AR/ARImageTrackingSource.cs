using UnityEngine;
#if GAMELAR_ARF
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace GamelAR
{
    /// <summary>
    /// Membaca kartu yang dilacak ARTrackedImageManager (AR Foundation + ARCore) dan meneruskannya ke ARCardManager.
    /// Mendukung beberapa gambar bergerak sekaligus (maksimal 5).
    /// </summary>
    public class ARImageTrackingSource : MonoBehaviour
    {
#if GAMELAR_ARF
        [SerializeField] ARTrackedImageManager manager;
        [SerializeField] XRReferenceImageLibrary library;
#endif
        [SerializeField] int maxMovingImages = 5;

        bool running;

        /// <summary>Dipanggil ARSessionBootstrap setelah izin kamera dan loader AR siap.</summary>
        public void Begin()
        {
#if GAMELAR_ARF
            if (manager == null) manager = GetComponent<ARTrackedImageManager>();
            if (manager == null) { Debug.LogError("[GamelAR] ARTrackedImageManager tidak ditemukan."); return; }
            if (library != null) manager.referenceLibrary = library;
            manager.requestedMaxNumberOfMovingImages = maxMovingImages;
            manager.enabled = true;
            running = true;
            Debug.Log("[GamelAR] Pelacakan gambar aktif (" + (library != null ? library.count : 0) + " kartu terdaftar, maks " + maxMovingImages + " bersamaan).");
#endif
        }

        void Update()
        {
#if GAMELAR_ARF
            if (!running || manager == null || ARCardManager.Instance == null) return;
            foreach (var img in manager.trackables)
            {
                if (img.trackingState != TrackingState.Tracking) continue;
                ARCardManager.Instance.ReportTracked(img.referenceImage.name, img.transform);
            }
#endif
        }
    }
}
