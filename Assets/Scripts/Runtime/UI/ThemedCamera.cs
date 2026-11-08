using UnityEngine;

namespace GamelAR
{
    /// <summary>Warna latar kamera mengikuti tema.</summary>
    /// <remarks>Harus berada di file bernama sama dengan kelasnya (lihat ThemeSkip.cs).</remarks>
    [RequireComponent(typeof(Camera))]
    public class ThemedCamera : MonoBehaviour
    {
        [SerializeField] Color baseColor = Color.white;
        public void Setup(Color c) => baseColor = c;
        void OnEnable() { GameSettings.Changed += Apply; Apply(); }
        void OnDisable() => GameSettings.Changed -= Apply;
        void Apply() => GetComponent<Camera>().backgroundColor = Theme.Map(baseColor);
    }
}
