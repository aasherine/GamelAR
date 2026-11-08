using UnityEngine;

namespace GamelAR
{
    /// <summary>Dibuat otomatis saat aplikasi mulai. Menerapkan volume utama dan memutar bunyi klik tombol.</summary>
    public class AppAudio : MonoBehaviour
    {
        public static AppAudio Instance { get; private set; }
        AudioSource sfx;
        AudioClip clickClip;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            if (Instance != null) return;
            var go = new GameObject("[AppAudio]");
            DontDestroyOnLoad(go);
            go.AddComponent<AppAudio>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            sfx.spatialBlend = 0f;
            clickClip = MakeClick();
            AudioListener.volume = GameSettings.MasterVolume;
        }

        public static void PlayClick()
        {
            if (Instance == null || !GameSettings.ButtonSfxOn) return;
            Instance.sfx.PlayOneShot(Instance.clickClip, 0.7f * GameSettings.SfxVolume);
        }

        // Bunyi "tok" pendek dibuat lewat kode agar tidak butuh file audio.
        static AudioClip MakeClick()
        {
            const int rate = 44100;
            int n = (int)(rate * 0.09f);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)rate;
                float env = Mathf.Exp(-t * 55f);
                data[i] = (Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 1320f * t) * 0.3f) * env * 0.5f;
            }
            var clip = AudioClip.Create("click", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
