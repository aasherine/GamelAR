using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Menghubungkan kontrol halaman Pengaturan dengan GameSettings (PlayerPrefs).</summary>
    public class SettingsController : MonoBehaviour
    {
        [SerializeField] Slider masterSlider;
        [SerializeField] TMP_Text masterValue;
        [SerializeField] Slider sfxSlider;
        [SerializeField] TMP_Text sfxValue;
        [SerializeField] ToggleSwitch clickToggle;
        [SerializeField] ToggleSwitch darkToggle;
        [SerializeField] ToggleSwitch hintToggle;
        [SerializeField] ToggleSwitch vibrateToggle;
        [SerializeField] Button languageButton;
        [SerializeField] TMP_Text languageLabel;
        [SerializeField] Button qualityButton;
        [SerializeField] TMP_Text qualityLabel;
        [SerializeField] Button feedbackButton;
        [SerializeField] Button rateButton;
        [SerializeField] Button resetButton;

        void Start()
        {
            masterSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
            sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
            UpdateTexts();

            masterSlider.onValueChanged.AddListener(v => { GameSettings.MasterVolume = v; UpdateTexts(); });
            sfxSlider.onValueChanged.AddListener(v => { GameSettings.SfxVolume = v; UpdateTexts(); });

            clickToggle.Set(GameSettings.ButtonSfxOn, false);
            clickToggle.Changed += on => GameSettings.ButtonSfxOn = on;
            darkToggle.Set(GameSettings.DarkTheme, false);
            darkToggle.Changed += on => GameSettings.DarkTheme = on;
            hintToggle.Set(GameSettings.ScanHintOn, false);
            hintToggle.Changed += on =>
            {
                GameSettings.ScanHintOn = on;
                SceneLoader.Toast(on ? "Petunjuk kamera ditampilkan" : "Petunjuk kamera disembunyikan");
            };
            vibrateToggle.Set(GameSettings.VibrateOn, false);
            vibrateToggle.Changed += on =>
            {
                GameSettings.VibrateOn = on;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (on) Handheld.Vibrate();
#endif
            };

            languageButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                SceneLoader.Toast("Saat ini tersedia Bahasa Indonesia. Bahasa lain menyusul.");
            });
            qualityButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                GameSettings.ModelQuality = GameSettings.ModelQuality + 1;
                UpdateTexts();
            });
            feedbackButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                if (string.IsNullOrEmpty(AppInfo.FeedbackWhatsApp)) { SceneLoader.Toast("Nomor WhatsApp masukan belum diatur"); return; }
                string text = UnityEngine.Networking.UnityWebRequest.EscapeURL("Halo, saya ingin memberi masukan untuk " + AppInfo.Name + " v" + AppInfo.Version + ": ").Replace("+", "%20");
                // wa.me membuka aplikasi WhatsApp bila terpasang, atau WhatsApp Web di peramban.
                Application.OpenURL("https://wa.me/" + AppInfo.FeedbackWhatsApp + "?text=" + text);
            });
            rateButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                string url = AppInfo.StoreUrl;
#if UNITY_ANDROID && !UNITY_EDITOR
                if (string.IsNullOrEmpty(url)) url = "market://details?id=" + AppInfo.PackageId;
#endif
                if (string.IsNullOrEmpty(url)) { SceneLoader.Toast("Tautan toko aplikasi belum diatur"); return; }
                Application.OpenURL(url);
            });
            resetButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                GameSettings.MasterVolume = 0.8f;
                GameSettings.SfxVolume = 0.8f;
                GameSettings.ScanHintOn = true;
                GameSettings.ButtonSfxOn = true;
                GameSettings.VibrateOn = true;
                GameSettings.DarkTheme = false;
                GameSettings.MergeCards = true;
                GameSettings.ModelQuality = 0;
                masterSlider.SetValueWithoutNotify(0.8f);
                sfxSlider.SetValueWithoutNotify(0.8f);
                hintToggle.Set(true, false);
                clickToggle.Set(true, false);
                vibrateToggle.Set(true, false);
                darkToggle.Set(false, false);
                UpdateTexts();
                SceneLoader.Toast("Pengaturan dikembalikan ke awal");
            });
        }

        void UpdateTexts()
        {
            masterValue.text = Mathf.RoundToInt(masterSlider.value * 100f) + "%";
            sfxValue.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
            qualityLabel.text = GameSettings.ModelQualityLabel;
            languageLabel.text = "Indonesia";
        }

        void OnDisable() => GameSettings.Save();
    }
}
