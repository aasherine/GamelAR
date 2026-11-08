using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Tombol yang membuka halaman Informasi pada instrumen tertentu.</summary>
    [RequireComponent(typeof(Button))]
    public class InstrumentLinkButton : MonoBehaviour
    {
        [SerializeField] string instrumentId;

        void Awake() => GetComponent<Button>().onClick.AddListener(() =>
        {
            AppAudio.PlayClick();
            SceneLoader.OpenInstrument(instrumentId);
        });
    }
}
