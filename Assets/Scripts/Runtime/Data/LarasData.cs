using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Audio nada laras global untuk fitur "Coba Laras". TODO: seret file audio asli ke array ini.
    /// Jika kosong, aplikasi memakai nada sintetis sebagai placeholder.
    /// </summary>
    [CreateAssetMenu(menuName = "GamelAR/Laras Data", fileName = "LarasData")]
    public class LarasData : ScriptableObject
    {
        [Tooltip("Urutan: 1, 2, 3, 4, 5, 6, 7, 1' (8 nada)")]
        public AudioClip[] pelog = new AudioClip[8];
        [Tooltip("Urutan: 1, 2, 3, 5, 6, 1' (6 nada)")]
        public AudioClip[] slendro = new AudioClip[6];

        static LarasData cached;
        public static LarasData Load()
        {
            if (cached == null) cached = Resources.Load<LarasData>("LarasData");
            return cached;
        }
    }
}
