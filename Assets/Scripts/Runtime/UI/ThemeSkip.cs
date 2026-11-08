using UnityEngine;

namespace GamelAR
{
    /// <summary>Penanda: elemen ini (dan turunannya) tidak diwarnai otomatis oleh tema.</summary>
    /// <remarks>Harus berada di file bernama sama dengan kelasnya. Kalau tidak, Unity menanam MonoScript
    /// palsu ke dalam scene dan scene itu rusak ("is corrupted ... Position out of bounds") di build pemain.</remarks>
    public class ThemeSkip : MonoBehaviour { }
}
