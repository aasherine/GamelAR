using UnityEngine;

namespace GamelAR
{
    /// <summary>Menurunkan opasitas hiasan (mis. motif batik) pada tema gelap agar tidak mengganggu teks.</summary>
    /// <remarks>Harus berada di file bernama sama dengan kelasnya (lihat ThemeSkip.cs).</remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class ThemeFade : MonoBehaviour
    {
        [SerializeField] float darkAlpha = 0.25f;
        public void Setup(float a) => darkAlpha = a;
        void OnEnable() { GameSettings.Changed += Apply; Apply(); }
        void OnDisable() => GameSettings.Changed -= Apply;
        void Apply() => GetComponent<CanvasGroup>().alpha = Theme.Dark ? darkAlpha : 1f;
    }
}
