using UnityEngine;

namespace GamelAR
{
    /// <summary>Pengaturan global aplikasi. Berjalan otomatis sebelum scene pertama dimuat.</summary>
    public static class AppManager
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;     // layar tetap menyala saat memindai
            Screen.orientation = ScreenOrientation.Portrait;
            AudioListener.volume = GameSettings.MasterVolume;
            Application.quitting += GameSettings.Save;
        }
    }
}
