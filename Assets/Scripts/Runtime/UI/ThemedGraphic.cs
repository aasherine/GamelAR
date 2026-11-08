using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Menyesuaikan warna Image/TMP dengan tema (terang/gelap). Dipasang otomatis oleh pembangun scene.</summary>
    [RequireComponent(typeof(Graphic))]
    public class ThemedGraphic : MonoBehaviour
    {
        [SerializeField] Color baseColor = Color.white;
        [SerializeField] bool isText;
        Graphic g;

        public void Setup(Color c, bool text)
        {
            baseColor = c; isText = text;
            if (Application.isPlaying && isActiveAndEnabled) { g = GetComponent<Graphic>(); Apply(); }   // dipakai juga saat runtime
        }

        void OnEnable()
        {
            g = GetComponent<Graphic>();
            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDisable() => GameSettings.Changed -= Apply;

        void Apply()
        {
            if (g == null) return;
            g.color = Theme.Map(baseColor, isText);
        }
    }
}
