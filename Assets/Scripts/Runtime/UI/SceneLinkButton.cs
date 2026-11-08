using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Pasang pada objek Button: saat diklik membuka scene tujuan.</summary>
    [RequireComponent(typeof(Button))]
    public class SceneLinkButton : MonoBehaviour
    {
        [SerializeField] string targetScene = SceneLoader.Beranda;

        void Awake() => GetComponent<Button>().onClick.AddListener(OnClick);

        void OnClick()
        {
            AppAudio.PlayClick();
            SceneLoader.Load(targetScene);
        }
    }
}
