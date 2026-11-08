using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Membuka/menutup sebuah panel pop-up lewat tombol (terhubung ke UIManager).</summary>
    public class PanelToggle : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Button openButton;
        [SerializeField] Button[] closeButtons;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (openButton != null) openButton.onClick.AddListener(() => { AppAudio.PlayClick(); UIManager.Open(panel); });
            foreach (var b in closeButtons)
                if (b != null) b.onClick.AddListener(() => { AppAudio.PlayClick(); UIManager.Close(panel); });
        }
    }
}
