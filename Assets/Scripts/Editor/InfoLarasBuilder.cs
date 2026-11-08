using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Scene InfoLaras (Koleksi): penampil 3D + tab info + Coba Laras.</summary>
    public static class InfoLarasBuilder
    {
        public const string ScenePath = "Assets/Scenes/InfoLaras.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            UIKit.NewUIScene(Palette.Bg, out var canvas);
            var safe = UIKit.BuildSafeRoot(canvas, Palette.Bg);

            // --- Panggung 3D (kamera + lampu) ---
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.68f, 0.63f, 0.58f);
            RenderSettings.skybox = null;
            var stage = new GameObject("Panggung3D").transform;
            stage.position = new Vector3(0, -500, 0);
            var camGo = new GameObject("ViewerCamera", typeof(Camera));
            camGo.transform.SetParent(stage, false);
            camGo.transform.localPosition = new Vector3(0, 0, -3);
            var vcam = camGo.GetComponent<Camera>();
            vcam.clearFlags = CameraClearFlags.SolidColor;
            vcam.backgroundColor = Palette.SurfaceAlt;
            vcam.fieldOfView = 28f;
            vcam.nearClipPlane = 0.05f;
            vcam.farClipPlane = 60f;
            vcam.depth = -10;
            vcam.allowHDR = false;
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            MakeLight("LampuUtama", stage, new Vector3(40, -30, 0), 1.25f, new Color(1f, 0.96f, 0.9f));
            MakeLight("LampuIsi", stage, new Vector3(-20, 140, 0), 0.55f, new Color(0.85f, 0.9f, 1f));

            // --- UI ---
            UIKit.TopBar(safe, "Informasi Instrumen");

            var panel = UIKit.Panel(safe, "PanelViewer", Palette.SurfaceAlt, true, 48f);
            UIKit.PlaceTop(panel.rectTransform, 0, 150, 960, 600);
            panel.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var rawGo = UIKit.Rect("Viewer3D", panel.transform);
            UIKit.Stretch(rawGo);
            var raw = rawGo.gameObject.AddComponent<RawImage>();
            raw.color = new Color(1, 1, 1, 0);
            raw.raycastTarget = true;
            var viewer = rawGo.gameObject.AddComponent<InstrumentViewer>();
            UIKit.SetField(viewer, "viewCamera", vcam);
            UIKit.SetField(viewer, "stage", stage);

            var prev = UIKit.MakeCircleButton(panel.transform, "Btn_Prev", new Color(0.365f, 0.251f, 0.216f, 0.88f), 96f);
            UIKit.PlaceLeftTop((RectTransform)prev.transform, 20, 252, 96, 96);
            var pi = UIKit.IconImage(prev.transform, "ic_prev", Palette.Cream, 44f);
            UIKit.PlaceCenter(pi.rectTransform, 0, 0, 44, 44);
            var next = UIKit.MakeCircleButton(panel.transform, "Btn_Next", new Color(0.365f, 0.251f, 0.216f, 0.88f), 96f);
            UIKit.PlaceRightTop((RectTransform)next.transform, 20, 252, 96, 96);
            var ni = UIKit.IconImage(next.transform, "ic_next", Palette.Cream, 44f);
            UIKit.PlaceCenter(ni.rectTransform, 0, 0, 44, 44);

            var counterBg = UIKit.Panel(panel.transform, "CounterBg", new Color(1, 1, 1, 0.85f), true, 28f);
            UIKit.PlaceLeftTop(counterBg.rectTransform, 24, 24, 150, 56);
            counterBg.gameObject.AddComponent<ThemeSkip>();
            var counter = UIKit.Label(counterBg.transform, "1 / 15", 28, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(counter.rectTransform);

            var resetBtn = UIKit.Chip(panel.transform, "Btn_ResetView", "RESET", new Color(1, 1, 1, 0.85f), Palette.BrownDark, 26, UIKit.FontKind.BodyBold, 28f);
            UIKit.PlaceRightTop((RectTransform)resetBtn.transform, 24, 24, 140, 56);
            resetBtn.gameObject.AddComponent<ThemeSkip>();

            panel.gameObject.AddComponent<ThemeSkip>();   // panel viewer & isinya tetap terang
            var hint = UIKit.Label(panel.transform, "Geser untuk memutar  -  Cubit untuk zoom", 26, Palette.BrownMid, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Petunjuk");
            UIKit.PlaceBottom(hint.rectTransform, 0, 16, 800, 40);

            var nameT = UIKit.Label(safe, "Nama Instrumen", 56, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Nama");
            UIKit.PlaceTop(nameT.rectTransform, 0, 765, 960, 84);
            var sub = UIKit.Label(safe, "Kelompok - Keterangan", 30, Palette.BrownMid, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceTop(sub.rectTransform, 0, 850, 920, 44);

            // Tab
            string[] tabNames = { "FUNGSI", "TEKNIK", "SEJARAH" };
            var tabBtns = new Button[3];
            var tabImgs = new Image[3];
            var tabLbls = new TMP_Text[3];
            for (int i = 0; i < 3; i++)
            {
                var chip = UIKit.Chip(safe, "Tab_" + tabNames[i], tabNames[i], Palette.SurfaceAlt, Palette.BrownDark, 30, UIKit.FontKind.BodyBold, 38f);
                UIKit.PlaceTop((RectTransform)chip.transform, -320 + i * 320, 912, 300, 76);
                tabBtns[i] = chip;
                tabImgs[i] = chip.GetComponent<Image>();
                tabLbls[i] = chip.GetComponentInChildren<TMP_Text>();
                chip.gameObject.AddComponent<ThemeSkip>();   // warna tab diatur InfoLarasController
            }

            // Isi teks (gulir)
            var textPanel = UIKit.Panel(safe, "PanelTeks", Palette.Surface, true, 40f);
            UIKit.PlaceTop(textPanel.rectTransform, 0, 1004, 960, 320);
            var scroll = UIKit.MakeScroll(textPanel.transform, "Gulir", out var content);
            UIKit.Stretch((RectTransform)scroll.transform, 32, 22, 32, 22);
            var body = UIKit.Label(content, "", 33, Palette.BrownDark, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Isi");
            body.lineSpacing = 10f;

            // Coba Laras
            var hLaras = UIKit.Label(safe, "COBA LARAS", 34, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold, "H_Laras");
            UIKit.PlaceTop(hLaras.rectTransform, 0, 1342, 600, 50);
            string[] pelogN = { "1", "2", "3", "4", "5", "6", "7", "1'" };
            string[] slenN = { "1", "2", "3", "5", "6", "1'" };
            var pelogBtns = LarasRow(safe, "Pelog", "PELOG", pelogN, 1398, Palette.Brown, out var pelogGroup);
            var slenBtns = LarasRow(safe, "Slendro", "SLENDRO", slenN, 1498, Palette.Gold, out var slenGroup);

            var sample = UIKit.MakeButton(safe, "Btn_Contoh", Palette.Brown, 44f);
            UIKit.PlaceTop((RectTransform)sample.transform, 0, 1606, 960, 96);
            var sIcon = UIKit.IconImage(sample.transform, "ic_play", Palette.Gold, 48f);
            UIKit.PlaceCenter(sIcon.rectTransform, -300, 0, 48, 48);
            var sLabel = UIKit.Label(sample.transform, "DENGAR CONTOH SUARA", 34, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(sLabel.rectTransform, 80, 0, 0, 0);

            UIKit.BuildNavBar(safe, SceneLoader.InfoLaras);
            UIKit.FinishScene(canvas);

            // Pengendali
            var ctrl = new GameObject("InfoLaras");
            var player = ctrl.AddComponent<LarasPlayer>();
            var c = ctrl.AddComponent<InfoLarasController>();
            UIKit.SetField(c, "viewer", viewer);
            UIKit.SetField(c, "player", player);
            UIKit.SetField(c, "nameText", nameT);
            UIKit.SetField(c, "subText", sub);
            UIKit.SetField(c, "counterText", counter);
            UIKit.SetField(c, "bodyText", body);
            UIKit.SetField(c, "bodyScroll", scroll);
            UIKit.SetField(c, "prevButton", prev);
            UIKit.SetField(c, "nextButton", next);
            UIKit.SetField(c, "resetViewButton", resetBtn);
            UIKit.SetField(c, "sampleButton", sample);
            UIKit.SetField(c, "sampleLabel", sLabel);
            UIKit.SetField(c, "sampleIcon", sIcon);
            UIKit.SetField(c, "iconPlay", UIKit.Icon("ic_play"));
            UIKit.SetField(c, "iconPause", UIKit.Icon("ic_pause"));
            UIKit.SetArray(c, "tabButtons", tabBtns);
            UIKit.SetArray(c, "tabImages", tabImgs);
            UIKit.SetArray(c, "tabLabels", tabLbls);
            UIKit.SetArray(c, "pelogButtons", pelogBtns);
            UIKit.SetArray(c, "slendroButtons", slenBtns);
            UIKit.SetField(c, "pelogGroup", pelogGroup);
            UIKit.SetField(c, "slendroGroup", slenGroup);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene InfoLaras dibuat: " + ScenePath);
        }

        static void MakeLight(string name, Transform parent, Vector3 euler, float intensity, Color color)
        {
            var go = new GameObject(name, typeof(Light));
            go.transform.SetParent(parent, false);
            go.transform.localRotation = Quaternion.Euler(euler);
            var l = go.GetComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = intensity;
            l.color = color;
            l.shadows = LightShadows.None;
        }

        static Button[] LarasRow(Transform parent, string id, string title, string[] notes, float y, Color color, out CanvasGroup group)
        {
            var row = UIKit.Rect("Laras_" + id, parent);
            UIKit.PlaceTop(row, 0, y, 960, 90);
            group = row.gameObject.AddComponent<CanvasGroup>();
            var lbl = UIKit.Label(row, title, 28, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyBold, "Judul");
            UIKit.PlaceLeftTop(lbl.rectTransform, 6, 0, 190, 90);
            var list = new Button[notes.Length];
            for (int i = 0; i < notes.Length; i++)
            {
                var b = UIKit.MakeCircleButton(row, "Nada_" + notes[i], color, 84f);
                UIKit.PlaceCenter((RectTransform)b.transform, -255 + i * 100 + 0f, 0, 84, 84);
                var l = UIKit.Label(b.transform, notes[i], 34, color == Palette.Gold ? Palette.OnGold : Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
                UIKit.Stretch(l.rectTransform);
                list[i] = b;
            }
            return list;
        }
    }
}
