using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Pesan singkat di bawah layar, mis. "Halaman ini belum tersedia".</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ToastView : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] float showSeconds = 1.8f;
        CanvasGroup group;
        Coroutine routine;

        void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        void OnEnable() => SceneLoader.ToastRequested += Show;
        void OnDisable() => SceneLoader.ToastRequested -= Show;

        public void Show(string message)
        {
            if (label != null) label.text = message;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(showSeconds);
            for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / 0.3f;
                yield return null;
            }
            group.alpha = 0f;
        }
    }
}
