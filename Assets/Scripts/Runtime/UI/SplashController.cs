using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Layar pembuka: logo berdenyut + bilah loading, lalu otomatis masuk ke Beranda.</summary>
    public class SplashController : MonoBehaviour
    {
        [SerializeField] RectTransform logo;
        [SerializeField] RectTransform barFill;     // anchorMax.x diubah 0..1
        [SerializeField] CanvasGroup content;
        [SerializeField] float minSeconds = 2.4f;

        IEnumerator Start()
        {
            InstrumentDatabase.Load(); // muat data lebih dulu
            var op = SceneManager.LoadSceneAsync(SceneLoader.Beranda);
            op.allowSceneActivation = false;
            float t = 0f;
            if (content != null) content.alpha = 0f;
            while (t < minSeconds || op.progress < 0.9f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(Mathf.Min(t / minSeconds, op.progress / 0.9f));
                if (barFill != null) barFill.anchorMax = new Vector2(Mathf.Lerp(0.04f, 1f, p), 1f);
                if (logo != null) logo.localScale = Vector3.one * (1f + 0.035f * Mathf.Sin(t * 3.2f));
                if (content != null) content.alpha = Mathf.MoveTowards(content.alpha, 1f, Time.unscaledDeltaTime * 2.5f);
                yield return null;
            }
            if (barFill != null) barFill.anchorMax = Vector2.one;
            yield return new WaitForSecondsRealtime(0.15f);
            if (content != null)
                while (content.alpha > 0f) { content.alpha -= Time.unscaledDeltaTime * 4f; yield return null; }
            op.allowSceneActivation = true;
        }
    }
}
