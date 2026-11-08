using UnityEngine;
using Vuforia;

namespace GamelAR
{
    /// <summary>
    /// Meneruskan pose kartu dari Vuforia ke ARCardManager.
    /// Hanya status TRACKED (kartu benar-benar terlihat kamera) yang dipakai. Pose EXTENDED_TRACKED
    /// berasal dari pelacakan perangkat; di HP/tablet tanpa ARCore (mis. Huawei) pose ini sering melompat,
    /// sehingga model tampak "berteleportasi". Lonjakan posisi yang tiba-tiba juga diabaikan
    /// sampai beberapa frame berturut-turut memastikannya.
    /// </summary>
    [RequireComponent(typeof(ObserverBehaviour))]
    public class VuforiaCardTarget : MonoBehaviour
    {
        const float JumpMeters = 0.12f;   // lompatan lebih jauh dari ini dalam satu frame dianggap pose salah
        const int ConfirmFrames = 4;      // ...kecuali muncul konsisten selama beberapa frame

        ObserverBehaviour observer;
        bool tracked;
        bool hasLast;
        Vector3 lastPos;
        int suspicious;

        void Awake()
        {
            observer = GetComponent<ObserverBehaviour>();
        }

        void OnEnable()
        {
            if (observer == null) return;
            observer.OnTargetStatusChanged += OnStatusChanged;
            Apply(observer.TargetStatus);
        }

        void OnDisable()
        {
            if (observer != null) observer.OnTargetStatusChanged -= OnStatusChanged;
            tracked = false;
            hasLast = false;
        }

        void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
        {
            Apply(status);
        }

        void Apply(TargetStatus status)
        {
            bool now = status.Status == Status.TRACKED;
            if (now && !tracked) { hasLast = false; suspicious = 0; }   // baru terlihat lagi: terima pose pertama
            tracked = now;
        }

        void LateUpdate()
        {
            if (!tracked || observer == null) return;
            var mgr = ARCardManager.Instance;
            if (mgr == null) return;

            Vector3 p = transform.position;
            if (hasLast && (p - lastPos).sqrMagnitude > JumpMeters * JumpMeters)
            {
                if (++suspicious < ConfirmFrames) return;   // abaikan lonjakan sesaat
            }
            suspicious = 0;
            lastPos = p;
            hasLast = true;
            mgr.ReportTracked(observer.TargetName, transform);
        }
    }
}
