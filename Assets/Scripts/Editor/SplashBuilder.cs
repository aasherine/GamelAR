using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Scene Splash: logo, bilah loading, lalu otomatis ke Beranda.</summary>
    public static class SplashBuilder
    {
        public const string ScenePath = "Assets/Scenes/Splash.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            UIKit.NewUIScene(Palette.Bg, out var canvas);
            var safe = UIKit.BuildSafeRoot(canvas, Palette.Bg);

            UIKit.KawungPattern(safe, "Batik_Atas", 0, -40, 6, 2, 190, new Color(1f, 0.70f, 0f, 0.13f));
            UIKit.KawungPattern(safe, "Batik_Bawah", 0, 1480, 6, 2, 190, new Color(1f, 0.70f, 0f, 0.13f));

            var group = UIKit.Rect("Konten", safe);
            UIKit.Stretch(group);
            var cg = group.gameObject.AddComponent<CanvasGroup>();

            var glow = UIKit.CirclePanel(group, "Glow", new Color(1f, 0.70f, 0f, 0.16f));
            UIKit.PlaceCenter(glow.rectTransform, 0, 200, 560, 560);
            var logo = UIKit.CirclePanel(group, "Logo", Palette.Brown);
            UIKit.PlaceCenter(logo.rectTransform, 0, 200, 380, 380);
            var ringOuter = UIKit.Panel(logo.transform, "Cincin", Palette.Gold, false);
            ringOuter.sprite = UIKit.Ring; ringOuter.type = Image.Type.Simple;
            UIKit.PlaceCenter(ringOuter.rectTransform, 0, 0, 340, 340);
            var ic = UIKit.AppLogo(logo.transform, 310f);   // logo resmi di dalam cincin emas
            UIKit.PlaceCenter(ic.rectTransform, 0, 0, 310, 310);

            var title = UIKit.Label(group, "GAMELAR", 120, Palette.Brown, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Judul");
            UIKit.PlaceCenter(title.rectTransform, 0, -140, 1000, 160);
            var sub = UIKit.Label(group, "Mengenal Gamelan Lewat AR", 44, Palette.BrownMid, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Subjudul");
            UIKit.PlaceCenter(sub.rectTransform, 0, -250, 1000, 70);

            var track = UIKit.Panel(group, "BarTrack", Palette.SurfaceLine, true, 12f);
            UIKit.PlaceCenter(track.rectTransform, 0, -520, 560, 24);
            var fill = UIKit.Panel(track.transform, "BarFill", Palette.Gold, true, 12f);
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(0.04f, 1f);
            fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
            var loading = UIKit.Label(group, "Memuat...", 34, Palette.Muted, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Memuat");
            UIKit.PlaceCenter(loading.rectTransform, 0, -580, 600, 50);

            var ver = UIKit.Label(safe, "v" + AppInfo.Version + "   -   " + AppInfo.Developer, 28, Palette.Muted, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Versi");
            UIKit.PlaceBottom(ver.rectTransform, 0, 50, 800, 50);

            var ctrl = new GameObject("Splash");
            var sc = ctrl.AddComponent<SplashController>();
            UIKit.SetField(sc, "logo", logo.rectTransform);
            UIKit.SetField(sc, "barFill", fill.rectTransform);
            UIKit.SetField(sc, "content", cg);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene Splash dibuat: " + ScenePath);
        }
    }
}
