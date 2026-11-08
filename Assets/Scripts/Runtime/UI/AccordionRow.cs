using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Baris yang bisa dibuka/ditutup (dipakai di halaman Tentang).</summary>
    public class AccordionRow : MonoBehaviour
    {
        [SerializeField] Button header;
        [SerializeField] GameObject body;
        [SerializeField] RectTransform chevron;
        [SerializeField] RectTransform layoutRoot;
        [SerializeField] bool startOpen;

        bool open;

        void Awake()
        {
            if (header != null) header.onClick.AddListener(() => { AppAudio.PlayClick(); Set(!open); });
            Set(startOpen);
        }

        void Set(bool value)
        {
            open = value;
            if (body != null) body.SetActive(open);
            if (chevron != null) chevron.localRotation = Quaternion.Euler(0f, 0f, open ? 90f : -90f);
            if (layoutRoot != null) LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
        }
    }
}
