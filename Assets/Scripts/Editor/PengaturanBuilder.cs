using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Scene Pengaturan: volume utama, volume efek, petunjuk kamera, kualitas model 3D.</summary>
    public static class PengaturanBuilder
    {
        public const string ScenePath = "Assets/Scenes/Pengaturan.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            UIKit.NewUIScene(Palette.Bg, out var canvas);
            var safe = UIKit.BuildSafeRoot(canvas, Palette.Bg);
            UIKit.TopBar(safe, "Pengaturan");

            // Seluruh isi dalam area gulir agar muat di layar kecil
            var scroll = UIKit.MakeScroll(safe, "Gulir", out var content, 30f);
            UIKit.Stretch((RectTransform)scroll.transform, 60, 200, 60, 150);

            // --- Suara ---
            var card1 = Card(content, "Kartu_Suara", 560, "SUARA");
            SliderRow(card1, 96, "Volume utama", out var master, out var masterVal);
            SliderRow(card1, 266, "Volume efek & bunyi tombol", out var sfx, out var sfxVal);
            var click = SwitchRow(card1, 440, "Bunyi tombol", "Bunyi \"tok\" saat menekan tombol");

            // --- Tampilan & bahasa ---
            var card2 = Card(content, "Kartu_Tampilan", 430, "TAMPILAN & BAHASA");
            var dark = SwitchRow(card2, 90, "Tema gelap", "Warna gelap yang nyaman di mata");
            var lang = ChipRow(card2, 250, "Bahasa", "Bahasa tampilan aplikasi", "Indonesia", "Btn_Bahasa", out var langLabel);

            // --- Kamera & AR ---
            var card3 = Card(content, "Kartu_Kamera", 590, "KAMERA & AR");
            var hint = SwitchRow(card3, 90, "Petunjuk kamera", "Bingkai & teks \"Arahkan kamera ke kartu\"");
            var vib = SwitchRow(card3, 250, "Getar saat kartu terdeteksi", "Getaran singkat tiap kartu baru terbaca");
            var quality = ChipRow(card3, 410, "Kualitas model 3D", "Ketuk: Otomatis, Rendah, Tinggi", "Otomatis", "Btn_Kualitas", out var qValue);

            // --- Lainnya ---
            var card4 = Card(content, "Kartu_Lainnya", 360, "LAINNYA");
            var feedback = ActionRow(card4, 90, "Kirim masukan", "Saran & laporan masalah lewat WhatsApp", "Btn_Masukan");
            var line = UIKit.Panel(card4, "Garis", Palette.SurfaceLine, false);
            UIKit.PlaceTop(line.rectTransform, 0, 218, 860, 3);
            var rate = ActionRow(card4, 226, "Beri nilai aplikasi", "Bantu kami dengan ulasan di toko aplikasi", "Btn_Nilai");

            // --- Reset & versi ---
            var reset = UIKit.Chip(content, "Btn_Reset", "ATUR ULANG PENGATURAN", Palette.SurfaceAlt, Palette.BrownDark, 32, UIKit.FontKind.BodyBold, 44f);
            reset.gameObject.AddComponent<LayoutElement>().preferredHeight = 100;
            var ver = UIKit.Label(content, AppInfo.Name + "  v" + AppInfo.Version, 28, Palette.Muted, TextAlignmentOptions.Center, UIKit.FontKind.Body, "Versi");
            ver.gameObject.AddComponent<LayoutElement>().preferredHeight = 60;

            UIKit.BuildNavBar(safe, SceneLoader.Pengaturan);
            UIKit.FinishScene(canvas);

            var ctrl = new GameObject("Pengaturan");
            var sc = ctrl.AddComponent<SettingsController>();
            UIKit.SetField(sc, "masterSlider", master);
            UIKit.SetField(sc, "masterValue", masterVal);
            UIKit.SetField(sc, "sfxSlider", sfx);
            UIKit.SetField(sc, "sfxValue", sfxVal);
            UIKit.SetField(sc, "clickToggle", click);
            UIKit.SetField(sc, "darkToggle", dark);
            UIKit.SetField(sc, "hintToggle", hint);
            UIKit.SetField(sc, "vibrateToggle", vib);
            UIKit.SetField(sc, "languageButton", lang);
            UIKit.SetField(sc, "languageLabel", langLabel);
            UIKit.SetField(sc, "qualityButton", quality);
            UIKit.SetField(sc, "qualityLabel", qValue);
            UIKit.SetField(sc, "feedbackButton", feedback);
            UIKit.SetField(sc, "rateButton", rate);
            UIKit.SetField(sc, "resetButton", reset);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene Pengaturan dibuat: " + ScenePath);
        }

        static RectTransform Card(Transform parent, string name, float h, string title)
        {
            var card = UIKit.Panel(parent, name, Palette.SurfaceLine, true, 48f);
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = h;
            var inner = UIKit.Panel(card.transform, "Isi", Palette.Surface, true, 46f);
            UIKit.Stretch(inner.rectTransform, 3, 3, 3, 3);
            var t = UIKit.Label(inner.transform, title, 28, Palette.BrownMid, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyBold, "Judul");
            UIKit.PlaceLeftTop(t.rectTransform, 48, 30, 700, 40);
            return inner.rectTransform;
        }

        static void SliderRow(Transform card, float y, string label, out Slider slider, out TMP_Text value)
        {
            var l = UIKit.Label(card, label, 34, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyMedium, "Label");
            UIKit.PlaceLeftTop(l.rectTransform, 48, y, 640, 50);
            var v = UIKit.Label(card, "80%", 32, Palette.BrownMid, TextAlignmentOptions.MidlineRight, UIKit.FontKind.BodyBold, "Nilai");
            UIKit.PlaceRightTop(v.rectTransform, 48, y, 200, 50);
            slider = UIKit.MakeSlider(card, "Slider", 840);
            UIKit.PlaceLeftTop((RectTransform)slider.transform, 60, y + 70, 840, 64);
            value = v;
        }

        static ToggleSwitch SwitchRow(Transform card, float y, string label, string sub)
        {
            var l = UIKit.Label(card, label, 34, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyMedium, "Label");
            UIKit.PlaceLeftTop(l.rectTransform, 48, y, 640, 50);
            var s = UIKit.Label(card, sub, 26, Palette.Muted, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceLeftTop(s.rectTransform, 48, y + 52, 640, 80);
            var sw = UIKit.MakeSwitch(card, "Saklar");
            UIKit.PlaceRightTop((RectTransform)sw.transform, 48, y + 14, 124, 68);
            return sw;
        }

        static Button ChipRow(Transform card, float y, string label, string sub, string value, string name, out TMP_Text valueLabel)
        {
            var l = UIKit.Label(card, label, 34, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyMedium, "Label");
            UIKit.PlaceLeftTop(l.rectTransform, 48, y, 560, 50);
            var s = UIKit.Label(card, sub, 26, Palette.Muted, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceLeftTop(s.rectTransform, 48, y + 52, 560, 80);
            var chip = UIKit.Chip(card, name, value, Palette.Brown, Palette.Cream, 32, UIKit.FontKind.BodyBold, 40f);
            UIKit.PlaceRightTop((RectTransform)chip.transform, 48, y + 10, 250, 84);
            valueLabel = chip.GetComponentInChildren<TMP_Text>();
            return chip;
        }

        static Button ActionRow(Transform card, float y, string label, string sub, string name)
        {
            var b = UIKit.MakeInvisibleButton(card, name);
            UIKit.PlaceTop((RectTransform)b.transform, 0, y, 920, 120);
            var l = UIKit.Label(b.transform, label, 34, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyMedium, "Label");
            UIKit.PlaceLeftTop(l.rectTransform, 28, 8, 700, 50);
            var s = UIKit.Label(b.transform, sub, 26, Palette.Muted, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceLeftTop(s.rectTransform, 28, 60, 740, 44);
            var ic = UIKit.Label(b.transform, ">", 52, Palette.BrownMid, TextAlignmentOptions.MidlineRight, UIKit.FontKind.BodyBold, "Panah");
            ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            ic.rectTransform.pivot = new Vector2(1f, 0.5f);
            ic.rectTransform.sizeDelta = new Vector2(60, 60);
            ic.rectTransform.anchoredPosition = new Vector2(-28, 0);
            return b;
        }
    }
}
