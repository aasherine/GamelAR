using UnityEngine;

namespace GamelAR
{
    /// <summary>Menyesuaikan RectTransform dengan area aman layar (poni/notch dan bilah navigasi).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        Rect last;
        Vector2Int lastSize;

        void OnEnable() => Apply();
        void Update()
        {
            if (Screen.safeArea != last || lastSize.x != Screen.width || lastSize.y != Screen.height) Apply();
        }

        void Apply()
        {
            var rt = (RectTransform)transform;
            var sa = Screen.safeArea;
            last = sa;
            lastSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;
            Vector2 min = sa.position, max = sa.position + sa.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
