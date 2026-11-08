# GamelAR — Mengenal Gamelan Lewat AR

Aplikasi Android **marker-based augmented reality** untuk mengenalkan 15 instrumen gamelan Jawa.
Kartu suvenir (image target) dipindai dengan kamera ponsel/tablet. Model 3D instrumen muncul di atas
kartu dan suaranya diputar. Bila beberapa kartu dipindai sekaligus (maksimal 5), suaranya digabung.

Tugas UTS **Augmented Reality untuk Pendidikan**, Universitas Darussalam Gontor.

| | |
|---|---|
| Kelompok | Kelompok 3 / Kelas C |
| Anggota | Aasa Malaayiine Rachma (NIM 452024618070) |
| Dosen pengampu | Faisal Reza Pradhana, M.Kom |

## Fitur

- **Pindai kartu**: 15 kartu image target (ID-01 s.d. ID-15, ukuran A6 10,5 × 14,8 cm). Setiap kartu memunculkan model 3D dan suara instrumennya.
- **Gabung kartu**: hingga 5 kartu dilacak bersamaan, suaranya dicampur.
- **Marker hilang**: model hilang dan bingkai petunjuk pindai muncul lagi.
- **Izin kamera**: diminta saat membuka halaman Pindai, dengan pesan bila ditolak.
- **Informasi instrumen**: penampil 3D (putar & zoom), tab Fungsi, Teknik, Sejarah, dan Laras.
- **Coba laras**: tombol nada Pelog dan Slendro per instrumen.
- **Pengaturan**: volume, bunyi tombol, tema gelap, petunjuk kamera, getar, kualitas model 3D.
- **Tentang**: pengembang, kredit & sumber, lisensi, kebijakan privasi.

## Teknologi

- Unity **6000.0.62f1** (Universal Render Pipeline, IL2CPP, Android)
- **Vuforia Engine 11.4.4** (image target)
- TextMeshPro, Input System

## Cara memasang (pengguna)

1. Unduh berkas `UTS_AR_Kelompok3_KelasC.apk`.
2. Salin ke ponsel/tablet Android (Android 10 / API 29 ke atas), buka, lalu pilih **Instal**.
   Bila diminta, izinkan pemasangan dari sumber tidak dikenal.
3. Buka **GamelAR**, tekan **Mulai Pemindaian**, izinkan kamera, lalu arahkan kamera ke kartu.
4. Cetak kartu marker dari `UTS_AR_Kelompok3_KelasC.pdf` pada ukuran **A6 (10,5 × 14,8 cm)**,
   sebaiknya kertas Art Carton 260 gsm doff. Pakai ruangan yang cukup terang dan letakkan kartu di permukaan datar.

## Cara menjalankan dari kode sumber (pengembang)

1. Pasang **Unity Hub** dan **Unity 6000.0.62f1** dengan modul **Android Build Support** (SDK, NDK, OpenJDK).
2. Clone repositori ini:
   ```bash
   git clone <url-repositori-ini>
   ```
3. **Pasang paket Vuforia** (tidak ikut di repo karena ukurannya 138 MB):
   unduh `com.ptc.vuforia.engine-11.4.4.tgz` dari [developer.vuforia.com](https://developer.vuforia.com/downloads/sdk)
   lalu taruh di folder `Packages/`. Nama berkas harus sama persis dengan yang tertulis di `Packages/manifest.json`.
4. Buka folder proyek lewat Unity Hub. Impor pertama butuh beberapa menit.
5. Isi **License Key Vuforia** milik Anda di `Assets/Resources/VuforiaConfiguration.asset`
   (Window > Vuforia Configuration) bila key bawaan tidak berlaku.
6. **File > Build Profiles > Android > Build**. Urutan scene: Splash, Beranda, PemindaiAR, InfoLaras, Pengaturan, Tentang.

> Jangan menjalankan menu *GamelAR > Bangun Semua*: menu ini membangun ulang semua scene
> dan akan menghapus setelan Vuforia yang dibuat manual di scene PemindaiAR.

## Struktur folder penting

| Folder | Isi |
|---|---|
| `Assets/Scenes` | 6 scene aplikasi |
| `Assets/Scripts/Runtime` | Kode aplikasi (AR, audio, UI, data) |
| `Assets/Scripts/Editor` | Pembangun scene & data (alat Editor) |
| `Assets/Models` | 15 model instrumen `.fbx` (versi standar & detail) |
| `Assets/Cards` | Gambar kartu marker |
| `Assets/ScriptableObjects/Instruments` | Data teks tiap instrumen |
| `Assets/StreamingAssets/Vuforia` | Database image target `GamelAR_Kartu` |

## Kredit

- Model 3D, desain kartu, antarmuka, materi ringkasan, dan kode pembangkit suara dibuat dengan bantuan **Claude (Anthropic)**.
- Suara instrumen adalah hasil sintesis digital, bukan rekaman gamelan asli.
- Font: Poppins (SIL OFL 1.1), DejaVu Serif (DejaVu Fonts License).
- Vuforia Engine © PTC Inc.
