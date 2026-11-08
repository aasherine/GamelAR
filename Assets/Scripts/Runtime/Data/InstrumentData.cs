using UnityEngine;

namespace GamelAR
{
    public enum LarasType { Pelog, Slendro, Keduanya }

    /// <summary>
    /// Satu aset = satu instrumen gamelan. Semua data (model, audio, teks) dikelola di sini.
    /// Aset dibuat otomatis oleh menu GamelAR > Bangun Semua, tetapi bisa diubah bebas di Inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "GamelAR/Instrument Data", fileName = "Instrument_")]
    public class InstrumentData : ScriptableObject
    {
        [Header("Identitas")]
        [Tooltip("Sama dengan nama file FBX dan nama kartu, misalnya BonangBarung")]
        public string id;
        public string displayName;
        public string tagline;
        public string kelompok;
        public LarasType laras = LarasType.Keduanya;

        [Header("Model 3D")]
        [Tooltip("Model untuk halaman Informasi dan kualitas Rendah")]
        public GameObject modelStandar;
        [Tooltip("Model detail (high-poly) untuk tampilan AR dan kualitas Tinggi")]
        public GameObject modelDetail;
        public Sprite thumbnail;

        [Header("Audio (TODO: isi dengan file asli)")]
        [Tooltip("Sampel suara yang diputar looping saat kartu terdeteksi. Kosong = nada placeholder otomatis.")]
        public AudioClip sampleLoop;
        [Range(0f, 1f)] public float loopVolume = 1f;
        [Tooltip("Opsional: nada laras khusus instrumen ini. Urutan Pelog: 1,2,3,4,5,6,7,1'")]
        public AudioClip[] larasPelog;
        [Tooltip("Opsional. Urutan Slendro: 1,2,3,5,6,1'")]
        public AudioClip[] larasSlendro;

        [Header("Info (Bahasa Indonesia)")]
        [TextArea(2, 6)] public string fungsi;
        [TextArea(2, 6)] public string teknik;
        [TextArea(2, 6)] public string sejarah;

        [Header("Pelacakan AR")]
        [Tooltip("Nama gambar di XRReferenceImageLibrary, misalnya Kartu_BonangBarung")]
        public string referenceImageName;
        [Tooltip("Lebar model (meter) saat tampil di atas kartu")]
        public float arSizeMeter = 0.12f;

        public GameObject GetModel(bool detail)
        {
            if (detail && modelDetail != null) return modelDetail;
            return modelStandar != null ? modelStandar : modelDetail;
        }
    }
}
