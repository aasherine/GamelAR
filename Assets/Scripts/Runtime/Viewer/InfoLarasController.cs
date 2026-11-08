using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamelAR
{
    /// <summary>
    /// Halaman Informasi (Koleksi): 3D viewer, tab Fungsi/Teknik/Sejarah, tombol Coba Laras (Pelog/Slendro),
    /// dan navigasi sebelumnya/berikutnya di antara 15 instrumen.
    /// </summary>
    public class InfoLarasController : MonoBehaviour
    {
        [SerializeField] InstrumentViewer viewer;
        [SerializeField] LarasPlayer player;

        [Header("Teks")]
        [SerializeField] TMP_Text nameText;
        [SerializeField] TMP_Text subText;
        [SerializeField] TMP_Text counterText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] ScrollRect bodyScroll;

        [Header("Tombol")]
        [SerializeField] Button prevButton;
        [SerializeField] Button nextButton;
        [SerializeField] Button resetViewButton;
        [SerializeField] Button sampleButton;
        [SerializeField] TMP_Text sampleLabel;
        [SerializeField] Image sampleIcon;
        [SerializeField] Sprite iconPlay;
        [SerializeField] Sprite iconPause;
        [SerializeField] Button[] tabButtons;      // Fungsi, Teknik, Sejarah
        [SerializeField] Image[] tabImages;
        [SerializeField] TMP_Text[] tabLabels;
        [SerializeField] Button[] pelogButtons;    // 8
        [SerializeField] Button[] slendroButtons;  // 6
        [SerializeField] CanvasGroup pelogGroup;
        [SerializeField] CanvasGroup slendroGroup;

        InstrumentDatabase db;
        int index;
        int tab;

        // Tata letak responsif (dihitung ulang bila tinggi layar/safe area berubah)
        RectTransform safe, larasHeader, larasCaption;
        float lastHeight = -1f;

        /// <summary>Penjelasan umum laras untuk tab LARAS.</summary>
        const string LarasInfo =
            "<b>Laras</b> adalah tangga nada gamelan. Gamelan Jawa memakai dua laras yang berbeda:\n\n" +
            "<b>Slendro</b> - 5 nada dalam satu gembyang (oktaf): 1 <i>barang</i>, 2 <i>gulu</i>, 3 <i>dhadha</i>, 5 <i>lima</i>, 6 <i>nem</i>. " +
            "Jarak antarnadanya hampir sama besar, sehingga sering terasa riang, gagah, atau tegas.\n\n" +
            "<b>Pelog</b> - 7 nada: 1 <i>bem</i>, 2 <i>gulu</i>, 3 <i>dhadha</i>, 4 <i>pelog</i>, 5 <i>lima</i>, 6 <i>nem</i>, 7 <i>barang</i>. " +
            "Jarak antarnadanya tidak sama (ada yang rapat, ada yang lebar), sehingga sering terasa agung, khidmat, atau sendu. " +
            "Satu lagu pelog biasanya hanya memakai lima dari tujuh nada itu.\n\n" +
            "Karena tangga nadanya berbeda, satu perangkat gamelan ageng sebenarnya berisi dua set instrumen: satu berlaras slendro dan satu berlaras pelog. " +
            "Nada gamelan juga tidak baku; setiap perangkat ditala sedikit berbeda dan memiliki rasa tersendiri yang disebut <i>embat</i>.\n\n" +
            "Tanda 1' berarti nada 1 yang satu gembyang (oktaf) lebih tinggi.";

        void Start()
        {
            db = InstrumentDatabase.Load();
            if (db == null || db.instruments.Count == 0)
            {
                nameText.text = "Data belum ada";
                return;
            }

            EnsureLarasTab();
            EnsureLarasCaption();
            Relayout();

            prevButton.onClick.AddListener(() => { AppAudio.PlayClick(); Step(-1); });
            nextButton.onClick.AddListener(() => { AppAudio.PlayClick(); Step(1); });
            resetViewButton.onClick.AddListener(() => { AppAudio.PlayClick(); viewer.ResetView(); });
            sampleButton.onClick.AddListener(() => { AppAudio.PlayClick(); player.ToggleSample(Current); UpdateSampleLabel(); });
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int t = i;
                tabButtons[i].onClick.AddListener(() => { AppAudio.PlayClick(); SetTab(t); });
            }
            for (int i = 0; i < pelogButtons.Length; i++)
            {
                int n = i;
                pelogButtons[i].onClick.AddListener(() => PressFirstRow(n));
            }
            for (int i = 0; i < slendroButtons.Length; i++)
            {
                int n = i;
                slendroButtons[i].onClick.AddListener(() => player.PlayNote(Current, false, n));
            }

            index = 0;
            if (!string.IsNullOrEmpty(SceneLoader.SelectedInstrumentId))
            {
                for (int i = 0; i < db.instruments.Count; i++)
                    if (db.instruments[i] != null && db.instruments[i].id == SceneLoader.SelectedInstrumentId) { index = i; break; }
                SceneLoader.SelectedInstrumentId = null;
            }
            GameSettings.Changed += RefreshTabs;
            Show(index);
        }

        void PressFirstRow(int n)
        {
            var d = Current;
            if (d == null) return;
            if (InstrumentProfiles.Get(d.id).IsDrum) player.PlayDrum(d, n);
            else player.PlayNote(d, true, n);
        }

        void RefreshTabs()
        {
            if (tabImages == null) return;
            for (int i = 0; i < tabImages.Length; i++)
            {
                if (tabImages[i] == null) continue;
                bool on = i == tab;
                tabImages[i].color = Theme.Map(on ? Palette.Brown : Palette.SurfaceAlt);
                tabLabels[i].color = Theme.Map(on ? Palette.Cream : Palette.BrownDark, true);
            }
        }

        InstrumentData Current => db != null && index >= 0 && index < db.instruments.Count ? db.instruments[index] : null;

        void Step(int dir)
        {
            int n = db.instruments.Count;
            Show(((index + dir) % n + n) % n);
        }

        void Show(int i)
        {
            index = i;
            var d = Current;
            if (d == null) return;
            player.StopSample();
            UpdateSampleLabel();

            if (subText != null) { subText.enableAutoSizing = true; subText.fontSizeMin = 20; subText.fontSizeMax = 30; subText.textWrappingMode = TextWrappingModes.NoWrap; subText.overflowMode = TextOverflowModes.Ellipsis; }
            nameText.text = d.displayName;
            subText.text = string.IsNullOrEmpty(d.kelompok) ? d.tagline : d.kelompok + "  -  " + d.tagline;
            counterText.text = (index + 1) + " / " + db.instruments.Count;
            viewer.SetInstrument(d);
            SetTab(0);

            var prof = InstrumentProfiles.Get(d.id);
            if (prof.IsDrum)
            {
                SetRow(pelogGroup, pelogButtons, prof.drum, "PUKULAN", true);
                SetRow(slendroGroup, slendroButtons, new string[0], "", false);
                if (slendroGroup != null) slendroGroup.gameObject.SetActive(false);
            }
            else
            {
                if (slendroGroup != null) slendroGroup.gameObject.SetActive(true);
                SetRow(pelogGroup, pelogButtons, prof.PelogLabels, "PELOG", d.laras != LarasType.Slendro);
                SetRow(slendroGroup, slendroButtons, prof.SlendroLabels, "SLENDRO", d.laras != LarasType.Pelog);
            }
        }

        static void SetRow(CanvasGroup g, Button[] btns, string[] labels, string title, bool ok)
        {
            if (g == null) return;
            g.alpha = ok ? 1f : 0.35f;
            g.interactable = ok;
            g.blocksRaycasts = ok;
            var t = g.transform.Find("Judul");
            if (t != null) { var tt = t.GetComponent<TMP_Text>(); if (tt != null) tt.text = title; }
            for (int i = 0; i < btns.Length; i++)
            {
                bool used = i < labels.Length;
                btns[i].gameObject.SetActive(used);
                if (!used) continue;
                var l = btns[i].GetComponentInChildren<TMP_Text>(true);
                if (l != null)
                {
                    l.enableAutoSizing = true; l.fontSizeMin = 18; l.fontSizeMax = 34;
                    l.text = labels[i];
                }
            }
        }

        void SetTab(int t)
        {
            tab = t;
            var d = Current;
            if (d == null) return;
            bodyText.text = t == 0 ? d.fungsi : t == 1 ? d.teknik : t == 2 ? d.sejarah : LarasText(d);
            if (string.IsNullOrWhiteSpace(bodyText.text)) bodyText.text = "Informasi belum tersedia.";
            if (bodyScroll != null) bodyScroll.verticalNormalizedPosition = 1f;
            RefreshTabs();
        }

        static string LarasText(InstrumentData d)
        {
            var prof = InstrumentProfiles.Get(d.id);
            string own;
            if (prof.IsDrum)
                own = "<b>" + d.displayName + "</b> tidak bernada tetap, sehingga tidak mengikuti laras. Kendhang dipakai bersama gamelan pelog maupun slendro; di bagian Coba Laras tersedia bunyi pukulan dasarnya.";
            else
                own = "Di aplikasi ini, <b>" + d.displayName + "</b> dapat dicoba dengan nada pelog <b>" + string.Join(" ", prof.PelogLabels) +
                      "</b> dan nada slendro <b>" + string.Join(" ", prof.SlendroLabels) + "</b>.";
            return own + "\n\n" + LarasInfo;
        }

        // ---------- Tab LARAS & keterangan (dibuat saat runtime dari objek yang sudah ada) ----------
        void EnsureLarasTab()
        {
            if (tabButtons == null || tabButtons.Length != 3 || tabButtons[2] == null) return;
            var src = tabButtons[2];
            var clone = Instantiate(src.gameObject, src.transform.parent);
            clone.name = "Tab_LARAS";
            var btn = clone.GetComponent<Button>();
            var lbl = clone.GetComponentInChildren<TMP_Text>(true);
            if (lbl != null) lbl.text = "LARAS";

            System.Array.Resize(ref tabButtons, 4); tabButtons[3] = btn;
            System.Array.Resize(ref tabImages, 4); tabImages[3] = clone.GetComponent<Image>();
            System.Array.Resize(ref tabLabels, 4); tabLabels[3] = lbl;

            for (int i = 0; i < 4; i++)
            {
                var rt = (RectTransform)tabButtons[i].transform;
                rt.sizeDelta = new Vector2(228f, rt.sizeDelta.y);
                rt.anchoredPosition = new Vector2(-366f + i * 244f, rt.anchoredPosition.y);
                var l = tabLabels[i];
                if (l != null) { l.enableAutoSizing = true; l.fontSizeMin = 20; l.fontSizeMax = 30; }
            }
        }

        void EnsureLarasCaption()
        {
            if (pelogGroup == null) return;
            safe = pelogGroup.transform.parent as RectTransform;
            larasHeader = safe != null ? safe.Find("H_Laras") as RectTransform : null;
            if (larasHeader == null) return;
            var go = Instantiate(larasHeader.gameObject, safe);
            go.name = "Keterangan_Laras";
            larasCaption = (RectTransform)go.transform;
            larasCaption.sizeDelta = new Vector2(960f, 36f);
            var t = go.GetComponent<TMP_Text>();
            t.text = "Pelog: 7 nada, jaraknya tidak sama   •   Slendro: 5 nada, jaraknya hampir sama";
            t.fontStyle = FontStyles.Normal;
            t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 26;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            var themed = go.GetComponent<ThemedGraphic>();
            if (themed != null) themed.Setup(Palette.BrownMid, true);
            else t.color = Palette.BrownMid;
        }

        void LateUpdate()
        {
            if (safe != null && Mathf.Abs(safe.rect.height - lastHeight) > 0.5f) Relayout();
        }

        static void Place(RectTransform rt, float top, float height)
        {
            if (rt == null) return;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -top);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        /// <summary>
        /// Susun ulang halaman agar pas di layar mana pun (mis. tablet 16:10 yang lebih "pendek" dari 1080x1920):
        /// blok Coba Laras + tombol contoh suara menempel di atas NavBar, sisa tinggi dibagi untuk panel 3D dan teks.
        /// </summary>
        void Relayout()
        {
            if (safe == null) safe = pelogGroup != null ? pelogGroup.transform.parent as RectTransform : null;
            if (safe == null) return;
            float H = safe.rect.height;
            if (H < 100f) return;
            lastHeight = H;

            var nav = safe.Find("NavBar") as RectTransform;
            float navH = nav != null ? nav.rect.height : 190f;

            // dari bawah ke atas
            float sampleTop = H - navH - 18f - 96f;
            float slendroTop = sampleTop - 12f - 88f;
            float pelogTop = slendroTop - 6f - 88f;
            float captionTop = pelogTop - 4f - 36f;
            float headerTop = captionTop - 46f;
            if (larasCaption == null) headerTop = pelogTop - 52f;

            // dari atas ke bawah
            const float top = 150f, nameH = 80f, subH = 42f, tabH = 72f;
            const float chunk = 10f + nameH + 2f + subH + 12f + tabH + 12f;
            float avail = headerTop - 12f - top - chunk;
            float viewerH = Mathf.Clamp(avail * 0.62f, 300f, 600f);
            float textH = avail - viewerH;
            if (textH < 200f) { textH = 200f; viewerH = Mathf.Max(260f, avail - textH); }

            var panelViewer = viewer != null ? viewer.transform.parent as RectTransform : null;
            Place(panelViewer, top, viewerH);
            if (panelViewer != null)
                foreach (var n in new[] { "Btn_Prev", "Btn_Next" })
                {
                    var b = panelViewer.Find(n) as RectTransform;
                    if (b == null) continue;
                    b.anchorMin = new Vector2(b.anchorMin.x, 0.5f);
                    b.anchorMax = new Vector2(b.anchorMax.x, 0.5f);
                    b.pivot = new Vector2(b.pivot.x, 0.5f);
                    b.anchoredPosition = new Vector2(b.anchoredPosition.x, 0f);
                }

            float y = top + viewerH + 10f;
            Place(nameText.rectTransform, y, nameH); y += nameH + 2f;
            Place(subText.rectTransform, y, subH); y += subH + 12f;
            foreach (var b in tabButtons) if (b != null) Place((RectTransform)b.transform, y, tabH);
            y += tabH + 12f;
            Place(bodyScroll != null ? bodyScroll.transform.parent as RectTransform : null, y, textH);

            Place(larasHeader, headerTop, 46f);
            Place(larasCaption, captionTop, 36f);
            Place((RectTransform)pelogGroup.transform, pelogTop, 88f);
            if (slendroGroup != null) Place((RectTransform)slendroGroup.transform, slendroTop, 88f);
            if (sampleButton != null) Place((RectTransform)sampleButton.transform, sampleTop, 96f);
        }

        void UpdateSampleLabel()
        {
            bool playing = player != null && player.SamplePlaying;
            if (sampleLabel != null) sampleLabel.text = playing ? "JEDA SUARA" : "DENGAR CONTOH SUARA";
            if (sampleIcon != null && iconPlay != null && iconPause != null) sampleIcon.sprite = playing ? iconPause : iconPlay;
        }

        void OnDisable() { GameSettings.Changed -= RefreshTabs; if (player != null) player.StopSample(); }
    }
}
