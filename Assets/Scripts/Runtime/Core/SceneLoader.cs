using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamelAR
{
    /// <summary>Pemindah scene terpusat + penyimpan "instrumen terpilih" antar scene.</summary>
    public static class SceneLoader
    {
        public const string Splash = "Splash";
        public const string Beranda = "Beranda";
        public const string Pemindai = "PemindaiAR";
        public const string InfoLaras = "InfoLaras";
        public const string Pengaturan = "Pengaturan";
        public const string Tentang = "Tentang";

        public static string Previous { get; private set; }
        public static string SelectedInstrumentId;
        public static event Action<string> ToastRequested;

        public static void Toast(string message) => ToastRequested?.Invoke(message);

        public static void Load(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Toast("Halaman ini belum tersedia");
                return;
            }
            string current = SceneManager.GetActiveScene().name;
            if (current == scene) return;
            Previous = current;
            SceneManager.LoadScene(scene);
        }

        public static void Back()
        {
            string current = SceneManager.GetActiveScene().name;
            if (current == Beranda)
            {
                Application.Quit();
                return;
            }
            string target = string.IsNullOrEmpty(Previous) || Previous == current ? Beranda : Previous;
            Previous = null;
            if (!Application.CanStreamedLevelBeLoaded(target)) target = Beranda;
            SceneManager.LoadScene(target);
        }

        public static void OpenInstrument(string instrumentId)
        {
            SelectedInstrumentId = instrumentId;
            Load(InfoLaras);
        }
    }
}
