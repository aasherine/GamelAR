using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Scene Tentang: pengembang, kredit & sumber, lisensi sumber terbuka, kebijakan privasi.</summary>
    public static class TentangBuilder
    {
        public const string ScenePath = "Assets/Scenes/Tentang.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            UIKit.NewUIScene(Palette.Bg, out var canvas);
            var safe = UIKit.BuildSafeRoot(canvas, Palette.Bg);
            UIKit.KawungPattern(safe, "Batik", 0, 100, 6, 2, 190, new Color(1f, 0.70f, 0f, 0.09f));
            UIKit.TopBar(safe, "Tentang");

            var logo = UIKit.CirclePanel(safe, "Logo", Palette.Brown);
            UIKit.PlaceTop(logo.rectTransform, 0, 170, 190, 190);
            var ic = UIKit.AppLogo(logo.transform, 176f);
            UIKit.PlaceCenter(ic.rectTransform, 0, 0, 176, 176);
            var name = UIKit.Label(safe, "GAMELAR", 64, Palette.Brown, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Nama");
            UIKit.PlaceTop(name.rectTransform, 0, 380, 800, 90);
            var ver = UIKit.Label(safe, "Mengenal Gamelan Lewat AR   -   v" + AppInfo.Version, 30, Palette.BrownMid, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Versi");
            UIKit.PlaceTop(ver.rectTransform, 0, 475, 960, 50);

            var scroll = UIKit.MakeScroll(safe, "Gulir", out var content, 22f);
            UIKit.Stretch((RectTransform)scroll.transform, 60, 206, 60, 560);   // berhenti di atas NavBar di semua rasio layar

            Row(content, "Pengembang",
                "GamelAR dikembangkan oleh <b>" + AppInfo.Developer + "</b> untuk memperkenalkan 15 instrumen gamelan Jawa lewat Augmented Reality: arahkan kamera ke kartu, lalu lihat model 3D dan dengarkan suaranya.\n\n" +
                "Versi " + AppInfo.Version + "\nPaket: com.aasa.gamelar", true);
            Row(content, "Kredit & Sumber", AppTexts.Kredit, false);
            Row(content, "Lisensi Sumber Terbuka", AppTexts.Lisensi, false);
            Row(content, "Kebijakan Privasi", AppTexts.Privasi, false);

            UIKit.BuildNavBar(safe, "");
            UIKit.FinishScene(canvas);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene Tentang dibuat: " + ScenePath);
        }

        static void Row(RectTransform content, string title, string text, bool open)
        {
            var row = UIKit.Panel(content, "Baris_" + title, Palette.SurfaceLine, true, 44f);
            var vlg = row.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(3, 3, 3, 3);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.spacing = 0;

            // wadah dalam (latar putih)
            var inner = UIKit.Panel(row.transform, "Isi", Palette.Surface, true, 42f);
            var ivlg = inner.gameObject.AddComponent<VerticalLayoutGroup>();
            ivlg.childControlWidth = true; ivlg.childControlHeight = true;
            ivlg.childForceExpandWidth = true; ivlg.childForceExpandHeight = false;

            var header = UIKit.MakeInvisibleButton(inner.transform, "Header");
            var le = header.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 128; le.minHeight = 128;
            var t = UIKit.Label(header.transform, title, 36, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyBold);
            UIKit.Stretch(t.rectTransform, 48, 0, 120, 0);
            var chev = UIKit.IconImage(header.transform, "ic_chevron", Palette.BrownMid, 44f, "Panah");
            chev.rectTransform.anchorMin = chev.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            chev.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            chev.rectTransform.anchoredPosition = new Vector2(-70, 0);

            var body = UIKit.Label(inner.transform, text, 30, Palette.BrownDark, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Isi");
            body.lineSpacing = 10f;
            body.margin = new Vector4(48, 0, 48, 44);

            var acc = row.gameObject.AddComponent<AccordionRow>();
            UIKit.SetField(acc, "header", header);
            UIKit.SetField(acc, "body", body.gameObject);
            UIKit.SetField(acc, "chevron", chev.rectTransform);
            UIKit.SetField(acc, "layoutRoot", content);
            UIKit.SetField(acc, "startOpen", open);
        }
    }
}
