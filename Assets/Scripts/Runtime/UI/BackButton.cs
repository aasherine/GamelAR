using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Tombol "Kembali" universal.</summary>
    [RequireComponent(typeof(Button))]
    public class BackButton : MonoBehaviour
    {
        void Awake() => GetComponent<Button>().onClick.AddListener(() =>
        {
            AppAudio.PlayClick();
            SceneLoader.Back();
        });
    }
}
