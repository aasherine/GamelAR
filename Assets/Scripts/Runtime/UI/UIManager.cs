using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamelAR
{
    /// <summary>
    /// Pengelola panel pop-up (tumpukan). Tombol Back Android menutup panel teratas lebih dulu;
    /// bila tidak ada panel terbuka, baru kembali ke halaman sebelumnya.
    /// </summary>
    public static class UIManager
    {
        static readonly List<GameObject> stack = new List<GameObject>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            stack.Clear();
            SceneManager.sceneLoaded -= OnLoaded;
            SceneManager.sceneLoaded += OnLoaded;
        }

        static void OnLoaded(Scene s, LoadSceneMode m) => stack.Clear();

        public static void Open(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(true);
            stack.Remove(panel);
            stack.Add(panel);
        }

        public static void Close(GameObject panel)
        {
            if (panel == null) return;
            panel.SetActive(false);
            stack.Remove(panel);
        }

        /// <summary>True bila ada panel yang ditutup.</summary>
        public static bool CloseTop()
        {
            stack.RemoveAll(p => p == null || !p.activeInHierarchy);
            if (stack.Count == 0) return false;
            Close(stack[stack.Count - 1]);
            return true;
        }
    }
}
