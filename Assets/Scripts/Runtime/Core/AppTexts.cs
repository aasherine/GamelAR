namespace GamelAR
{
    /// <summary>
    /// Teks panjang halaman Tentang. Satu sumber untuk TentangBuilder; isi yang sama juga tertulis di scene Tentang.
    /// Bila diubah di sini, ubah juga teks di scene (atau bangun ulang scene Tentang saja).
    /// </summary>
    public static class AppTexts
    {
        public const string Kredit =
            "Model 3D instrumen, desain kartu, dan antarmuka: dibuat oleh tim pengembang GamelAR dengan bantuan <b>Claude (Anthropic)</b>.\n\n" +
            "Materi ringkasan instrumen (fungsi, teknik, sejarah, serta penjelasan laras pelog dan slendro): disusun dengan bantuan <b>Claude (Anthropic)</b> berdasarkan pengetahuan umum karawitan Jawa.\n\n" +
            "Suara instrumen dan nada laras: dibangkitkan secara digital (sintesis) oleh aplikasi dengan kode yang dibuat bersama <b>Claude (Anthropic)</b>, bukan rekaman gamelan asli. Nada yang terdengar adalah perkiraan.\n\n" +
            "Font: Poppins (SIL Open Font License 1.1) dan DejaVu Serif (lisensi bebas DejaVu/Bitstream Vera).\n\n" +
            "Teknologi: Unity, Vuforia Engine (PTC), TextMeshPro.";

        public const string Lisensi =
            "Aplikasi ini memakai komponen berikut:\n\n" +
            "- Unity Engine dan paket Unity (Universal RP, Input System, TextMeshPro): lisensi paket Unity.\n" +
            "- Vuforia Engine: hak cipta PTC Inc., dipakai sesuai Vuforia Engine Developer Agreement.\n" +
            "- Poppins: SIL Open Font License 1.1.\n" +
            "- DejaVu Serif: DejaVu Fonts License (berbasis Bitstream Vera), bebas dipakai dan disebarkan.";

        public const string Privasi =
            "Terakhir diperbarui: November 2026.\n\n" +
            "GamelAR <b>tidak memerlukan akun atau login</b> dan tidak mengumpulkan data pribadi seperti nama, email, nomor telepon, kontak, atau lokasi.\n\n" +
            "<b>Kamera.</b> Kamera hanya aktif saat halaman Pindai dibuka, untuk mengenali kartu GamelAR langsung di perangkat. Gambar dan video kamera tidak disimpan dan tidak dikirim ke server mana pun.\n\n" +
            "<b>Vuforia Engine.</b> Pengenalan kartu memakai Vuforia Engine dari PTC. Pustaka ini dapat mengirim data teknis anonim (misalnya model perangkat, versi sistem operasi, dan versi aplikasi) ke PTC untuk validasi lisensi dan statistik penggunaan, sesuai kebijakan privasi PTC. Gambar kamera tidak termasuk di dalamnya.\n\n" +
            "<b>Pengaturan.</b> Pilihan seperti volume, tema, dan kualitas model disimpan lokal di perangkat dan ikut terhapus bila data aplikasi dihapus atau aplikasi dicopot.\n\n" +
            "<b>Izin aplikasi.</b> Kamera (untuk memindai kartu) dan getar (umpan balik saat kartu terdeteksi). Izin kamera dapat dicabut kapan saja lewat Pengaturan Android.\n\n" +
            "<b>Iklan dan pembelian.</b> GamelAR tidak menampilkan iklan dan tidak memiliki pembelian di dalam aplikasi, sehingga aman dipakai pelajar.\n\n" +
            "<b>Kontak.</b> Pertanyaan tentang privasi dapat disampaikan kepada pengembang (" + AppInfo.Developer + ") melalui WhatsApp " + AppInfo.FeedbackWhatsAppDisplay + ", atau lewat menu Pengaturan > Kirim masukan.";
    }
}
