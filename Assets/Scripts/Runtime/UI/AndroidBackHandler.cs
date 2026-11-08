using UnityEngine;
using UnityEngine.InputSystem;

namespace GamelAR
{
    /// <summary>Tombol Back bawaan Android (dipetakan ke Escape oleh Input System).</summary>
    public class AndroidBackHandler : MonoBehaviour
    {
        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                if (!UIManager.CloseTop()) SceneLoader.Back();
            }
        }
    }
}
