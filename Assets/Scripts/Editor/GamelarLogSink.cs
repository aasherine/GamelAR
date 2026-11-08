using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamelAR.EditorTools
{
    /// <summary>Menyalin log penting ([GamelAR], error, exception) ke file gamelar-log.txt di folder proyek agar mudah diperiksa.</summary>
    [InitializeOnLoad]
    public static class GamelarLogSink
    {
        static readonly string Path_ = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../gamelar-log.txt"));

        static GamelarLogSink()
        {
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            bool mine = msg.StartsWith("[GamelAR]");
            if (!mine && type == LogType.Log) return;
            if (!mine && type == LogType.Warning) return;
            try
            {
                string line = System.DateTime.Now.ToString("HH:mm:ss") + " [" + type + "] " + msg;
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    var lines = (stack ?? "").Split('\n');
                    for (int i = 0; i < Mathf.Min(6, lines.Length); i++) line += "\n    " + lines[i];
                }
                File.AppendAllText(Path_, line + "\n");
            }
            catch { }
        }

        [MenuItem("GamelAR/Kosongkan gamelar-log.txt")]
        static void Clear() { try { File.WriteAllText(Path_, ""); } catch { } }
    }
}
