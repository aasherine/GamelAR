using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>
    /// UI overlay scene PemindaiAR: bingkai pindai, petunjuk, penghitung kartu, nama instrumen,
    /// tombol Gabung Kartu, serta layar status (izin kamera / AR tidak didukung).
    /// </summary>
    public class ScannerUI : MonoBehaviour
    {
        [Header("Bingkai & petunjuk")]
        [SerializeField] GameObject scanFrame;
        [SerializeField] GameObject hintPill;
        [SerializeField] TMP_Text counterText;

        [Header("Panel bawah")]
        [SerializeField] GameObject bottomPanel;
        [SerializeField] TMP_Text namesText;
        [SerializeField] Button infoButton;
        [SerializeField] ToggleSwitch mergeToggle;

        [Header("Layar status")]
        [SerializeField] GameObject statusRoot;
        [SerializeField] TMP_Text statusTitle;
        [SerializeField] TMP_Text statusBody;
        [SerializeField] Button statusButton;
        [SerializeField] TMP_Text statusButtonLabel;
        [SerializeField] GameObject simBadge;

        ARSessionBootstrap bootstrap;
        System.Action statusAction;

        void Awake()
        {
            bootstrap = FindFirstObjectByType<ARSessionBootstrap>();
            if (statusButton != null) statusButton.onClick.AddListener(() => { AppAudio.PlayClick(); statusAction?.Invoke(); });
            if (infoButton != null) infoButton.onClick.AddListener(() =>
            {
                AppAudio.PlayClick();
                var cm = ARCardManager.Instance;
                if (cm != null && cm.Latest != null) SceneLoader.OpenInstrument(cm.Latest.id);
            });
            if (mergeToggle != null)
            {
                mergeToggle.Set(GameSettings.MergeCards, false);
                mergeToggle.Changed += on =>
                {
                    GameSettings.MergeCards = on;
                    if (ARCardManager.Instance != null) ARCardManager.Instance.SetMergeMode(on);
                    SceneLoader.Toast(on ? "Gabung Kartu aktif: suara semua kartu dicampur" : "Gabung Kartu mati: hanya kartu terbaru yang bersuara");
                };
            }
        }

        void OnEnable()
        {
            ARSessionBootstrap.StateChanged += OnState;
            if (ARCardManager.Instance != null) ARCardManager.Instance.Changed += Refresh;
            OnState(ARSessionBootstrap.State);
            Refresh();
        }

        void Start()
        {
            // ARCardManager mungkin baru siap setelah Awake
            if (ARCardManager.Instance != null)
            {
                ARCardManager.Instance.Changed -= Refresh;
                ARCardManager.Instance.Changed += Refresh;
                ARCardManager.Instance.SetMergeMode(GameSettings.MergeCards);
            }
            Refresh();
        }

        void OnDisable()
        {
            ARSessionBootstrap.StateChanged -= OnState;
            if (ARCardManager.Instance != null) ARCardManager.Instance.Changed -= Refresh;
        }

        void OnState(ARState s)
        {
            bool ready = s == ARState.Ready || s == ARState.Simulation;
            if (statusRoot != null) statusRoot.SetActive(!ready);
            if (simBadge != null) simBadge.SetActive(s == ARState.Simulation);

            switch (s)
            {
                case ARState.Starting:
                    SetStatus("Menyiapkan kamera...", "Mohon tunggu sebentar.", null, null);
                    break;
                case ARState.NeedPermission:
                    if (ARSessionBootstrap.PermissionPermanentlyDenied)
                        SetStatus("Izin kamera diperlukan",
                                  "GamelAR memakai kamera hanya untuk mengenali kartu. Aktifkan izin Kamera di Pengaturan aplikasi, lalu kembali ke sini.",
                                  "BUKA PENGATURAN", ARSessionBootstrap.OpenAppSettings);
                    else
                        SetStatus("Izin kamera diperlukan",
                                  "GamelAR memakai kamera hanya untuk mengenali kartu. Gambar tidak disimpan maupun dikirim ke mana pun.",
                                  "IZINKAN KAMERA", () => { if (bootstrap != null) bootstrap.Begin(); });
                    break;
                case ARState.Unsupported:
                    SetStatus("AR belum bisa dipakai",
                              "Mesin AR (Vuforia) gagal dimulai. Pastikan kamera tidak sedang dipakai aplikasi lain, lalu coba lagi.",
                              "COBA LAGI", () => { if (bootstrap != null) bootstrap.Begin(); });
                    break;
            }
            Refresh();
        }

        void SetStatus(string title, string body, string buttonLabel, System.Action action)
        {
            if (statusTitle != null) statusTitle.text = title;
            if (statusBody != null) statusBody.text = body;
            statusAction = action;
            if (statusButton != null) statusButton.gameObject.SetActive(action != null);
            if (statusButtonLabel != null && buttonLabel != null) statusButtonLabel.text = buttonLabel;
        }

        void Refresh()
        {
            var cm = ARCardManager.Instance;
            int n = cm != null ? cm.DetectedCount : 0;
            bool ready = ARSessionBootstrap.State == ARState.Ready || ARSessionBootstrap.State == ARState.Simulation;

            if (counterText != null) counterText.text = "Kartu terdeteksi: " + n;
            if (scanFrame != null) scanFrame.SetActive(ready && n == 0 && GameSettings.ScanHintOn);
            if (hintPill != null) hintPill.SetActive(ready && n == 0 && GameSettings.ScanHintOn);
            if (bottomPanel != null) bottomPanel.SetActive(ready);
            if (infoButton != null) infoButton.gameObject.SetActive(n > 0);

            if (namesText != null)
            {
                if (n == 0) namesText.text = "Belum ada kartu";
                else
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < cm.Detected.Count; i++)
                    {
                        if (i > 0) sb.Append("  +  ");
                        sb.Append(cm.Detected[i].displayName);
                    }
                    namesText.text = sb.ToString();
                }
            }
        }
    }
}
