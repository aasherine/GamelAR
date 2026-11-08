using System;
using UnityEngine;

namespace GamelAR
{
    /// <summary>Pengaturan pengguna yang disimpan lokal lewat PlayerPrefs.</summary>
    public static class GameSettings
    {
        public static event Action Changed;

        const string KVolume = "gamelar.volume";
        const string KSfxVolume = "gamelar.sfxvolume";
        const string KSfx = "gamelar.sfx";
        const string KHint = "gamelar.hint";
        const string KVibrate = "gamelar.vibrate";
        const string KQuality = "gamelar.quality";
        const string KLang = "gamelar.lang";

        public static float MasterVolume
        {
            get => PlayerPrefs.GetFloat(KVolume, 0.8f);
            set { value = Mathf.Clamp01(value); PlayerPrefs.SetFloat(KVolume, value); AudioListener.volume = value; Changed?.Invoke(); }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(KSfxVolume, 0.8f);
            set { PlayerPrefs.SetFloat(KSfxVolume, Mathf.Clamp01(value)); Changed?.Invoke(); }
        }

        public static bool ButtonSfxOn { get => PlayerPrefs.GetInt(KSfx, 1) == 1; set => SetBool(KSfx, value); }
        public static bool ScanHintOn { get => PlayerPrefs.GetInt(KHint, 1) == 1; set => SetBool(KHint, value); }
        public static bool MergeCards { get => PlayerPrefs.GetInt("gamelar.merge", 1) == 1; set => SetBool("gamelar.merge", value); }
        public static bool DarkTheme { get => PlayerPrefs.GetInt("gamelar.dark", 0) == 1; set => SetBool("gamelar.dark", value); }
        public static bool VibrateOn { get => PlayerPrefs.GetInt(KVibrate, 1) == 1; set => SetBool(KVibrate, value); }

        /// <summary>"id" = Indonesia (satu-satunya bahasa saat ini).</summary>
        public static string Language
        {
            get => PlayerPrefs.GetString(KLang, "id");
            set { PlayerPrefs.SetString(KLang, value); Changed?.Invoke(); }
        }

        /// <summary>0 = Otomatis, 1 = Rendah (model standar), 2 = Tinggi (model detail)</summary>
        public static int ModelQuality
        {
            get => PlayerPrefs.GetInt(KQuality, 0);
            set { PlayerPrefs.SetInt(KQuality, ((value % 3) + 3) % 3); Changed?.Invoke(); }
        }

        public static string ModelQualityLabel => ModelQuality == 1 ? "Rendah" : ModelQuality == 2 ? "Tinggi" : "Otomatis";

        /// <summary>True bila model detail (high-poly) dipakai. Mode Otomatis memilih berdasarkan RAM perangkat.</summary>
        public static bool UseDetailModel
        {
            get
            {
                switch (ModelQuality)
                {
                    case 1: return false;
                    case 2: return true;
                    default: return SystemInfo.systemMemorySize >= 3500;
                }
            }
        }

        public static void Save() => PlayerPrefs.Save();

        static void SetBool(string key, bool v)
        {
            PlayerPrefs.SetInt(key, v ? 1 : 0);
            Changed?.Invoke();
        }
    }
}
