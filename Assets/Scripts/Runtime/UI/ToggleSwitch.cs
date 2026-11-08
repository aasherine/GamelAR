using System;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>Saklar on/off bergaya iOS dengan animasi halus.</summary>
    public class ToggleSwitch : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] Image track;
        [SerializeField] RectTransform knob;
        [SerializeField] float travel = 34f;
        [SerializeField] Color onColor = new Color(0.365f, 0.251f, 0.216f);
        [SerializeField] Color offColor = new Color(0.851f, 0.812f, 0.753f);

        public event Action<bool> Changed;
        public bool IsOn { get; private set; }
        float anim;

        void Awake()
        {
            if (button != null) button.onClick.AddListener(() => { AppAudio.PlayClick(); Set(!IsOn, true); });
            Apply(true);
        }

        public void Set(bool on, bool notify)
        {
            IsOn = on;
            if (notify) Changed?.Invoke(on);
        }

        void Update() => Apply(false);

        void Apply(bool instant)
        {
            float target = IsOn ? 1f : 0f;
            anim = instant ? target : Mathf.MoveTowards(anim, target, Time.unscaledDeltaTime * 8f);
            if (knob != null) knob.anchoredPosition = new Vector2(Mathf.Lerp(-travel, travel, anim), 0f);
            if (track != null) track.color = Color.Lerp(Theme.Map(Palette.SwitchOff), Theme.Map(Palette.Brown), anim);
        }
    }
}
