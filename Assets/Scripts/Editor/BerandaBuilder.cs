using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Membangun scene Beranda sesuai mockup (palet krem / cokelat / emas).</summary>
    public static class BerandaBuilder
    {
        public const string ScenePath = "Assets/Scenes/Beranda.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            UIKit.NewUIScene(Palette.Bg, out var canvas);
            var db = InstrumentDatabase.Load();
            var safe = UIKit.BuildSafeRoot(canvas, Palette.Bg);

            UIKit.KawungPattern(safe, "Batik", 0, -60, 6, 2, 190, new Color(1f, 0.70f, 0f, 0.11f));

            // Header
            var logo = UIKit.Label(safe, "GAMELAR", 112, Palette.Brown, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Logo");
            UIKit.PlaceTop(logo.rectTransform, 0, 90, 1000, 160);
            var sub = UIKit.Label(safe, "Mengenal Gamelan Lewat AR", 44, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Subjudul");
            UIKit.PlaceTop(sub.rectTransform, 0, 245, 1000, 64);

            // Tombol besar Mulai Pemindaian
            var scan = UIKit.MakeButton(safe, "Btn_MulaiPemindaian", Palette.Brown, 48f);
            UIKit.PlaceTop((RectTransform)scan.transform, 0, 380, 960, 250);
            scan.gameObject.AddComponent<SceneLinkButton>();
            UIKit.SetField(scan.GetComponent<SceneLinkButton>(), "targetScene", SceneLoader.Pemindai);
            var t1 = UIKit.Label(scan.transform, "MULAI PEMINDAIAN (AR)", 44, Palette.Cream, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyBold, "Judul");
            UIKit.PlaceLeftTop(t1.rectTransform, 60, 52, 680, 70);
            var t2 = UIKit.Label(scan.transform, "Arahkan kamera ke kartu", 36, new Color(0.99f, 0.96f, 0.90f, 0.82f), TextAlignmentOptions.MidlineLeft, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceLeftTop(t2.rectTransform, 60, 130, 680, 60);
            var gold = UIKit.CirclePanel(scan.transform, "LingkarEmas", Palette.Gold);
            gold.rectTransform.anchorMin = gold.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            gold.rectTransform.pivot = new Vector2(1f, 0.5f);
            gold.rectTransform.anchoredPosition = new Vector2(-50, 0);
            gold.rectTransform.sizeDelta = new Vector2(150, 150);
            var arIcon = UIKit.AppLogo(gold.transform, 136f);   // logo resmi dengan bingkai emas tipis
            UIKit.PlaceCenter(arIcon.rectTransform, 0, 0, 136, 136);

            // Pintasan cepat
            var h1 = UIKit.Label(safe, "PINTASAN CEPAT", 38, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold, "H_Pintasan");
            UIKit.PlaceTop(h1.rectTransform, 0, 672, 900, 60);

            Shortcut(safe, -245, 750, "PANDUAN", "FAQ & Cara Pakai", "ic_question", null, out var panduanBtn);
            Shortcut(safe, 245, 750, "TENTANG", "Info Aplikasi", "ic_info", SceneLoader.Tentang, out _);
            Shortcut(safe, -245, 1000, "KOLEKSI", "Semua Instrumen", "ic_book", SceneLoader.InfoLaras, out _);
            Shortcut(safe, 245, 1000, "PENGATURAN", "Suara, Kamera, 3D", "ic_gear", SceneLoader.Pengaturan, out _);

            // Info menarik
            var h2 = UIKit.Label(safe, "INFO MENARIK ALAT GAMELAN", 38, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold, "H_Info");
            UIKit.PlaceTop(h2.rectTransform, 0, 1275, 1000, 60);
            string[] ids = { "GongAgeng", "Demung", "GenderBarung" };
            float[] xs = { -330, 0, 330 };
            for (int i = 0; i < 3; i++) InfoTile(safe, db, ids[i], xs[i], 1350);

            var all = UIKit.MakeInvisibleButton(safe, "Btn_LihatSemua");
            UIKit.PlaceTop((RectTransform)all.transform, 0, 1600, 400, 60);
            all.gameObject.AddComponent<SceneLinkButton>();
            UIKit.SetField(all.GetComponent<SceneLinkButton>(), "targetScene", SceneLoader.InfoLaras);
            var allTxt = UIKit.Label(all.transform, "lihat semua", 32, Palette.BrownMid);
            UIKit.Stretch(allTxt.rectTransform);

            UIKit.BuildNavBar(safe, SceneLoader.Beranda);

            BuildGuidePanel(canvas.transform, panduanBtn);
            UIKit.FinishScene(canvas);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene Beranda dibuat: " + ScenePath);
        }

        static void Shortcut(Transform parent, float x, float y, string title, string subtitle, string icon, string scene, out Button button)
        {
            var b = UIKit.MakeButton(parent, "Btn_" + title, Palette.SurfaceLine, 40f);
            UIKit.PlaceTop((RectTransform)b.transform, x, y, 470, 230);
            var inner = UIKit.Panel(b.transform, "Isi", Palette.Surface, true, 38f);
            UIKit.Stretch(inner.rectTransform, 3, 3, 3, 3);
            if (scene != null)
            {
                b.gameObject.AddComponent<SceneLinkButton>();
                UIKit.SetField(b.GetComponent<SceneLinkButton>(), "targetScene", scene);
            }
            var disc = UIKit.CirclePanel(b.transform, "Cakram", Palette.GoldSoft);
            UIKit.PlaceTop(disc.rectTransform, 0, 20, 100, 100);
            var ic = UIKit.IconImage(disc.transform, icon, Palette.OnGold, 56f);
            UIKit.PlaceCenter(ic.rectTransform, 0, 0, 56, 56);
            var t = UIKit.Label(b.transform, title, 34, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.PlaceTop(t.rectTransform, 0, 128, 450, 46);
            var s = UIKit.Label(b.transform, subtitle, 26, Palette.Muted);
            UIKit.PlaceTop(s.rectTransform, 0, 174, 450, 40);
            button = b;
        }

        static void InfoTile(Transform parent, InstrumentDatabase db, string id, float x, float y)
        {
            var data = db != null ? db.GetById(id) : null;
            var b = UIKit.MakeButton(parent, "Info_" + id, Palette.Brown, 40f);
            UIKit.PlaceTop((RectTransform)b.transform, x, y, 310, 230);
            var link = b.gameObject.AddComponent<InstrumentLinkButton>();
            UIKit.SetField(link, "instrumentId", id);

            // Bingkai krem (tetap krem pada tema gelap) berisi model Set transparan
            var frame = UIKit.Panel(b.transform, "Bingkai", Palette.SurfaceAlt, true, 28f);
            UIKit.PlaceTop(frame.rectTransform, 0, 14, 282, 150);
            frame.gameObject.AddComponent<ThemeSkip>();
            if (data != null && data.thumbnail != null)
            {
                var img = UIKit.Panel(frame.transform, "Thumb", Color.white, false);
                img.sprite = data.thumbnail;
                img.preserveAspect = true;
                UIKit.Stretch(img.rectTransform, 8, 6, 8, 6);
            }
            string name = data != null ? data.displayName : id;
            var t = UIKit.Label(b.transform, name, 30, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyMedium);
            UIKit.PlaceTop(t.rectTransform, 0, 172, 300, 46);
        }

        static void BuildGuidePanel(Transform canvas, Button openButton)
        {
            var overlay = UIKit.Panel(canvas, "GuideOverlay", new Color(0.1f, 0.06f, 0.04f, 0.65f), false);
            overlay.raycastTarget = true;
            UIKit.Stretch(overlay.rectTransform);

            var closeBg = overlay.gameObject.AddComponent<Button>();
            closeBg.targetGraphic = overlay;
            closeBg.transition = Selectable.Transition.None;

            var card = UIKit.Panel(overlay.transform, "Kartu", Palette.Surface, true, 52f);
            card.raycastTarget = true;
            UIKit.PlaceCenter(card.rectTransform, 0, 40, 940, 1280);

            var title = UIKit.Label(card.transform, "PANDUAN PENGGUNAAN", 48, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.Heading);
            UIKit.PlaceTop(title.rectTransform, 0, 50, 860, 90);

            string steps =
                "1.  Buka aplikasi GamelAR.\n" +
                "2.  Tekan tombol <b>Mulai Pemindaian</b> dan izinkan akses kamera.\n" +
                "3.  Arahkan kamera ke sisi depan kartu dan pastikan kartu masuk ke dalam bingkai layar.\n" +
                "4.  Pastikan ruangan cukup terang dan kartu diletakkan di permukaan datar.\n\n" +
                "<b>Tip seru:</b> letakkan 2 kartu atau lebih bersamaan untuk mendengarkan perpaduan instrumennya. Matikan \"Gabung Kartu\" bila ingin mendengar kartu terbaru saja.\n\n" +
                "<b>Kartu tidak terdeteksi?</b> Dekatkan atau jauhkan kamera sedikit, hindari pantulan cahaya pada kartu, dan jangan menutupi gambar dengan jari.";
            var body = UIKit.Label(card.transform, steps, 34, Palette.BrownDark, TextAlignmentOptions.TopLeft);
            UIKit.PlaceTop(body.rectTransform, 0, 170, 820, 860);
            body.lineSpacing = 12f;

            var close = UIKit.MakeButton(card.transform, "Btn_Tutup", Palette.Brown, 40f);
            UIKit.PlaceBottom((RectTransform)close.transform, 0, 50, 520, 120);
            var cl = UIKit.Label(close.transform, "Mengerti", 40, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(cl.rectTransform);

            var ctrl = new GameObject("GuideController");
            ctrl.transform.SetParent(canvas, false);
            var toggle = ctrl.AddComponent<PanelToggle>();
            UIKit.SetField(toggle, "panel", overlay.gameObject);
            UIKit.SetField(toggle, "openButton", openButton);
            UIKit.SetArray(toggle, "closeButtons", new Object[] { close, closeBg });
            overlay.gameObject.SetActive(false);
        }
    }
}
