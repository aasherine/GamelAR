using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Menyetel frame rate sekali saat aplikasi mulai (tanpa perlu objek di scene).
    /// Di Android, Unity memakai 30 FPS bila targetFrameRate tidak diatur, sehingga
    /// putaran model 3D terlihat patah-patah. Dengan 60 FPS gerakannya lebih halus.
    /// </summary>
    public static class FrameRateSetup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }
    }
}
