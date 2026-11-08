using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GamelAR.EditorTools
{
    /// <summary>Pembantu untuk membangun UI uGUI + TextMeshPro lewat kode (palet krem / cokelat / emas).</summary>
    public static class UIKit
    {
        public static Color Hex(string hex) => Palette.Hex(hex);

        // Alias palet
        public static readonly Color Bg = Palette.Bg;
        public static readonly Color Surface = Palette.Surface;
        public static readonly Color SurfaceAlt = Palette.SurfaceAlt;
        public static readonly Color SurfaceLine = Palette.SurfaceLine;
        public static readonly Color Brown = Palette.Brown;
        public static readonly Color BrownDark = Palette.BrownDark;
        public static readonly Color BrownMid = Palette.BrownMid;
        public static readonly Color Cream = Palette.Cream;
        public static readonly Color Gold = Palette.Gold;
        public static readonly Color Muted = Palette.Muted;

        public enum FontKind { Heading, Body, BodyMedium, BodyBold }

        public const string FontDir = "Assets/Fonts/";
        public const string TmpFontDir = "Assets/Fonts/TMP/";
        const string IconDir = "Assets/UI/Icons/";
        const string SpriteDir = "Assets/UI/Sprites/";

        // ---------- Font TMP ----------
        static string FontFile(FontKind k) => k == FontKind.Heading ? "Heading-DejaVuSerifBold"
                                            : k == FontKind.BodyBold ? "Body-Poppins-Bold"
                                            : k == FontKind.BodyMedium ? "Body-Poppins-Medium"
                                            : "Body-Poppins-Regular";

        public static TMP_FontAsset GetFont(FontKind kind)
        {
            string path = TmpFontDir + FontFile(kind) + " SDF.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa == null)
            {
                CreateFontAssets();
                fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }
            return fa;
        }

        public static void CreateFontAssets()
        {
            Directory.CreateDirectory(TmpFontDir);
            foreach (FontKind k in System.Enum.GetValues(typeof(FontKind)))
            {
                string ttf = FontDir + FontFile(k) + ".ttf";
                string assetPath = TmpFontDir + FontFile(k) + " SDF.asset";
                if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null) continue;
                var font = AssetDatabase.LoadAssetAtPath<Font>(ttf);
                if (font == null) { Debug.LogWarning("[GamelAR] Font tidak ditemukan: " + ttf); continue; }
                var fa = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                                                       1024, 1024, AtlasPopulationMode.Dynamic, true);
                if (fa == null) { Debug.LogWarning("[GamelAR] Gagal membuat font TMP: " + ttf); continue; }
                fa.name = FontFile(k) + " SDF";
                AssetDatabase.CreateAsset(fa, assetPath);
                if (fa.atlasTexture != null) { fa.atlasTexture.name = fa.name + " Atlas"; AssetDatabase.AddObjectToAsset(fa.atlasTexture, fa); }
                if (fa.material != null) { fa.material.name = fa.name + " Material"; AssetDatabase.AddObjectToAsset(fa.material, fa); }
                EditorUtility.SetDirty(fa);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[GamelAR] Font TextMeshPro dibuat di " + TmpFontDir);
        }

        // ---------- Sprite ----------
        static Sprite roundedSprite, circleSprite, ringSprite;
        public static Sprite Rounded => roundedSprite != null ? roundedSprite : roundedSprite = MakeSprite(SpriteDir + "rounded.png", 128, 40, 0, true);
        public static Sprite Circle => circleSprite != null ? circleSprite : circleSprite = MakeSprite(SpriteDir + "circle.png", 128, 64, 0, false);
        public static Sprite Ring => ringSprite != null ? ringSprite : ringSprite = MakeSprite(SpriteDir + "ring.png", 128, 63, 5, false);

        static Sprite MakeSprite(string path, int size, float radius, float ringThickness, bool sliced)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            float half = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                    float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                    float outside = new Vector2(Mathf.Max(dx, 0f), Mathf.Max(dy, 0f)).magnitude;
                    float inside = Mathf.Min(Mathf.Max(dx, dy), 0f);
                    float dist = outside + inside - radius;
                    float a = Mathf.Clamp01(0.5f - dist);
                    if (ringThickness > 0f) a *= Mathf.Clamp01(dist + ringThickness + 0.5f);
                    px[y * size + x] = new Color(1f, 1f, 1f, a);
                }
            }
            tex.SetPixels32(px);
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.spritePixelsPerUnit = 100;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spriteBorder = sliced ? new Vector4(radius, radius, radius, radius) : Vector4.zero;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            if (!File.Exists(path)) return null;
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null) { AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport); imp = AssetImporter.GetAtPath(path) as TextureImporter; }
            if (imp == null) return null;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.alphaIsTransparency = true;
            imp.mipmapEnabled = false;
            imp.spritePixelsPerUnit = 100;
            imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static Sprite Icon(string name) => LoadSprite(IconDir + name + ".png");

        // ---------- Scene & Canvas ----------
        public static Canvas CreateCanvas(string name = "Canvas")
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            s.matchWidthOrHeight = 0.5f;
            return c;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var m = go.GetComponent<InputSystemUIInputModule>();
            var asset = UnityEngine.InputSystem.InputSystem.actions;
            if (asset != null && asset.FindAction("UI/Point") != null)
            {
                m.actionsAsset = asset;
                m.point = Ref(asset, "UI/Point");
                m.leftClick = Ref(asset, "UI/Click");
                m.middleClick = Ref(asset, "UI/MiddleClick");
                m.rightClick = Ref(asset, "UI/RightClick");
                m.scrollWheel = Ref(asset, "UI/ScrollWheel");
                m.move = Ref(asset, "UI/Navigate");
                m.submit = Ref(asset, "UI/Submit");
                m.cancel = Ref(asset, "UI/Cancel");
                m.trackedDevicePosition = Ref(asset, "UI/TrackedDevicePosition");
                m.trackedDeviceOrientation = Ref(asset, "UI/TrackedDeviceOrientation");
            }
            else
            {
                try { m.AssignDefaultActions(); }
                catch (System.Exception e) { Debug.LogWarning("[GamelAR] EventSystem: " + e.Message); }
            }
        }

        static UnityEngine.InputSystem.InputActionReference Ref(UnityEngine.InputSystem.InputActionAsset asset, string path)
        {
            var a = asset.FindAction(path);
            return a != null ? UnityEngine.InputSystem.InputActionReference.Create(a) : null;
        }

        public static void EnsureRendererSettings() { }

        // ---------- Rect helpers ----------
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>x = geser dari tengah, y = jarak dari tepi atas ke tepi atas objek.</summary>
        public static RectTransform PlaceTop(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform PlaceBottom(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform PlaceCenter(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        /// <summary>Jangkar kiri-atas: x dari tepi kiri, y dari tepi atas.</summary>
        public static RectTransform PlaceLeftTop(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        /// <summary>Jangkar kanan-atas: x dari tepi kanan (positif ke dalam), y dari tepi atas.</summary>
        public static RectTransform PlaceRightTop(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform StretchTopBar(RectTransform rt, float height)
        {
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0, height);
            return rt;
        }

        // ---------- Widgets ----------
        public static Image Panel(Transform parent, string name, Color color, bool rounded = true, float radius = 40f)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            if (rounded)
            {
                img.sprite = Rounded;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 40f / Mathf.Max(radius, 1f);
            }
            return img;
        }

        public static Image CirclePanel(Transform parent, string name, Color color)
        {
            var img = Panel(parent, name, color, false);
            img.sprite = Circle;
            img.type = Image.Type.Simple;
            return img;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, FontKind font = FontKind.Body, string name = "Label")
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            var fa = GetFont(font);
            if (fa != null) t.font = fa;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            t.raycastTarget = false;
            t.richText = true;
            return t;
        }

        public static Image IconImage(Transform parent, string iconName, Color tint, float size, string name = "Icon")
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Icon(iconName);
            img.color = tint;
            img.preserveAspect = true;
            img.raycastTarget = false;
            rt.sizeDelta = new Vector2(size, size);
            return img;
        }

        public const string LogoPath = "Assets/UI/Logo/GamelAR_logo_round.png";

        /// <summary>Logo resmi GamelAR (ikon aplikasi, dipotong bulat). Tidak ikut diwarnai tema.</summary>
        public static Image AppLogo(Transform parent, float size, string name = "Icon")
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = LoadSprite(LogoPath);
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            rt.sizeDelta = new Vector2(size, size);
            rt.gameObject.AddComponent<ThemeSkip>();
            return img;
        }

        public static Button MakeButton(Transform parent, string name, Color bg, float radius = 40f, bool rounded = true)
        {
            var img = Panel(parent, name, bg, rounded, radius);
            img.raycastTarget = true;
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.97f, 0.97f, 0.97f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            cb.selectedColor = Color.white;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            cb.fadeDuration = 0.08f;
            b.colors = cb;
            return b;
        }

        public static Button MakeCircleButton(Transform parent, string name, Color bg, float size)
        {
            var b = MakeButton(parent, name, bg, 64f, false);
            var img = b.GetComponent<Image>();
            img.sprite = Circle;
            img.type = Image.Type.Simple;
            ((RectTransform)b.transform).sizeDelta = new Vector2(size, size);
            return b;
        }

        /// <summary>Tombol kapsul bertuliskan teks (chip).</summary>
        public static Button Chip(Transform parent, string name, string text, Color bg, Color fg, float fontSize, FontKind font = FontKind.BodyBold, float radius = 36f)
        {
            var b = MakeButton(parent, name, bg, radius);
            var l = Label(b.transform, text, fontSize, fg, TextAlignmentOptions.Center, font);
            Stretch(l.rectTransform);
            return b;
        }

        /// <summary>Tombol tanpa latar (hanya area sentuh transparan).</summary>
        public static Button MakeInvisibleButton(Transform parent, string name)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0);
            img.raycastTarget = true;
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            b.transition = Selectable.Transition.None;
            return b;
        }

        public static Button BackButtonWidget(Transform parent, Color circleColor, Color iconColor, float x = -440f, float y = 40f)
        {
            var b = MakeCircleButton(parent, "BackButton", circleColor, 112f);
            PlaceTop((RectTransform)b.transform, x, y, 112, 112);
            b.gameObject.AddComponent<BackButton>();
            var ic = IconImage(b.transform, "ic_back", iconColor, 56f);
            PlaceCenter((RectTransform)ic.transform, -3f, 0f, 56, 56);
            return b;
        }

        /// <summary>Bar judul halaman: tombol kembali + judul serif di tengah.</summary>
        public static void TopBar(Transform parent, string title, bool back = true)
        {
            if (back) BackButtonWidget(parent, SurfaceAlt, BrownDark, -440f, 24f);
            var t = Label(parent, title, 54, BrownDark, TextAlignmentOptions.Center, FontKind.Heading, "Judul");
            PlaceTop(t.rectTransform, 0, 30, 700, 100);
        }

        public static Slider MakeSlider(Transform parent, string name, float width)
        {
            var root = Rect(name, parent);
            root.sizeDelta = new Vector2(width, 64);
            var track = Panel(root, "Track", SurfaceLine, true, 12f);
            PlaceCenter(track.rectTransform, 0, 0, width, 24);
            var fillArea = Rect("Fill Area", root);
            PlaceCenter(fillArea, 0, 0, width - 56, 24);
            var fill = Panel(fillArea, "Fill", Gold, true, 12f);
            fill.rectTransform.anchorMin = new Vector2(0, 0); fill.rectTransform.anchorMax = new Vector2(0, 1);
            // Isi emas melebar 28 px ke kiri agar menutup penuh ujung trek abu-abu
            fill.rectTransform.offsetMin = new Vector2(-28, 0);
            fill.rectTransform.offsetMax = new Vector2(0, 0);
            // Slider menarik anchor tombol sepanjang sumbu Y (0..1), jadi tinggi tombol = tinggi area + sizeDelta.y.
            // Area dibuat setinggi tombol (56) dan sizeDelta.y = 0 agar tombol benar-benar bulat.
            var handleArea = Rect("Handle Slide Area", root);
            PlaceCenter(handleArea, 0, 0, width - 56, 56);
            var handle = CirclePanel(handleArea, "Handle", Brown);
            handle.raycastTarget = true;
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            handle.rectTransform.sizeDelta = new Vector2(56, 0);
            var ring = CirclePanel(handle.transform, "Inner", Cream);
            PlaceCenter(ring.rectTransform, 0, 0, 22, 22);

            var s = root.gameObject.AddComponent<Slider>();
            s.fillRect = fill.rectTransform;
            s.handleRect = handle.rectTransform;
            s.targetGraphic = handle;
            s.direction = Slider.Direction.LeftToRight;
            s.minValue = 0f; s.maxValue = 1f; s.value = 0.8f;
            return s;
        }

        public static ToggleSwitch MakeSwitch(Transform parent, string name)
        {
            var b = MakeButton(parent, name, SwitchOffColor, 40f);
            var rt = (RectTransform)b.transform;
            rt.sizeDelta = new Vector2(124, 68);
            b.transition = Selectable.Transition.None;
            var knob = CirclePanel(b.transform, "Knob", Color.white);
            PlaceCenter(knob.rectTransform, -28, 0, 52, 52);
            var sw = b.gameObject.AddComponent<ToggleSwitch>();
            SetField(sw, "button", b);
            SetField(sw, "track", b.GetComponent<Image>());
            SetField(sw, "knob", knob.rectTransform);
            SetField(sw, "travel", 28f);
            SetField(sw, "onColor", Brown);
            SetField(sw, "offColor", SwitchOffColor);
            b.gameObject.AddComponent<ThemeSkip>();   // warna saklar diatur ToggleSwitch sendiri
            return sw;
        }

        static Color SwitchOffColor => Palette.SwitchOff;

        /// <summary>Area gulir vertikal. Mengembalikan ScrollRect; isi ditambahkan ke 'content' (VerticalLayoutGroup).</summary>
        public static ScrollRect MakeScroll(Transform parent, string name, out RectTransform content, float spacing = 0f)
        {
            var root = Rect(name, parent);
            var bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0);
            bg.raycastTarget = true;
            var sr = root.gameObject.AddComponent<ScrollRect>();

            var viewport = Rect("Viewport", root);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            content = Rect("Content", viewport);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.spacing = spacing;
            var fit = content.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.viewport = viewport;
            sr.content = content;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Elastic;
            sr.scrollSensitivity = 30f;
            sr.decelerationRate = 0.12f;
            return sr;
        }

        /// <summary>Pola kawung sederhana (empat cincin saling tumpuk) sebagai ornamen batik minimalis.</summary>
        public static void KawungPattern(Transform parent, string name, float x, float y, int cols, int rows, float cell, Color color)
        {
            var root = Rect(name, parent);
            PlaceTop(root, x, y, cols * cell, rows * cell);
            root.gameObject.AddComponent<CanvasGroup>();
            root.gameObject.AddComponent<ThemeFade>().Setup(0.2f);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float cx = (c + 0.5f) * cell - cols * cell / 2f;
                    float cy = -((r + 0.5f) * cell);
                    float d = cell * 0.5f;
                    float s = cell * 0.74f;
                    Vector2[] off = { new Vector2(0, d * 0.5f), new Vector2(0, -d * 0.5f), new Vector2(-d * 0.5f, 0), new Vector2(d * 0.5f, 0) };
                    foreach (var o in off)
                    {
                        var ring = Panel(root, "k", color, false);
                        ring.sprite = Ring; ring.type = Image.Type.Simple;
                        ring.rectTransform.anchorMin = ring.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                        ring.rectTransform.sizeDelta = new Vector2(s, s);
                        ring.rectTransform.anchoredPosition = new Vector2(cx + o.x, cy + o.y);
                    }
                }
            }
        }

        // ---------- Serialisasi field privat ----------
        public static void SetField(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning("[GamelAR] Field tidak ditemukan: " + target.GetType().Name + "." + field); return; }
            if (value is Object o) p.objectReferenceValue = o;
            else if (value is string s) p.stringValue = s;
            else if (value is float f) p.floatValue = f;
            else if (value is int i) p.intValue = i;
            else if (value is bool bo) p.boolValue = bo;
            else if (value is Color c) p.colorValue = c;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogWarning("[GamelAR] Array tidak ditemukan: " + target.GetType().Name + "." + field); return; }
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Komponen bersama ----------
        public static RectTransform BuildSafeRoot(Canvas canvas, Color background)
        {
            var bg = Panel(canvas.transform, "Background", background, false);
            Stretch(bg.rectTransform);
            var safe = Rect("SafeArea", canvas.transform);
            Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter>();
            return safe;
        }

        public static void BuildNavBar(Transform parent, string activeScene)
        {
            var nav = Panel(parent, "NavBar", Surface, false);
            nav.rectTransform.anchorMin = new Vector2(0, 0); nav.rectTransform.anchorMax = new Vector2(1, 0);
            nav.rectTransform.pivot = new Vector2(0.5f, 0); nav.rectTransform.anchoredPosition = Vector2.zero;
            nav.rectTransform.sizeDelta = new Vector2(0, 190);
            nav.raycastTarget = true;

            var line = Panel(nav.transform, "Line", SurfaceLine, false);
            line.rectTransform.anchorMin = new Vector2(0, 1); line.rectTransform.anchorMax = new Vector2(1, 1);
            line.rectTransform.pivot = new Vector2(0.5f, 1); line.rectTransform.anchoredPosition = Vector2.zero;
            line.rectTransform.sizeDelta = new Vector2(0, 3);

            string[] labels = { "Beranda", "Pindai", "Informasi", "Pengaturan" };
            string[] icons = { "ic_home", "ic_ar", "ic_book", "ic_gear" };
            string[] scenes = { SceneLoader.Beranda, SceneLoader.Pemindai, SceneLoader.InfoLaras, SceneLoader.Pengaturan };
            float w = 1f / 4f;
            for (int i = 0; i < 4; i++)
            {
                bool active = scenes[i] == activeScene;
                var b = MakeInvisibleButton(nav.transform, "Nav_" + labels[i]);
                var rt = (RectTransform)b.transform;
                rt.anchorMin = new Vector2(i * w, 0); rt.anchorMax = new Vector2((i + 1) * w, 1);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                var link = b.gameObject.AddComponent<SceneLinkButton>();
                SetField(link, "targetScene", scenes[i]);
                Color c = active ? BrownDark : Hex("B7A99C");
                if (active)
                {
                    var pill = Panel(b.transform, "Aktif", Palette.GoldSoft, true, 36f);
                    PlaceTop(pill.rectTransform, 0, 20, 130, 76);
                }
                var ic = IconImage(b.transform, icons[i], active ? Palette.OnGold : c, 64f);
                PlaceTop(ic.rectTransform, 0, 26, 64, 64);
                var lb = Label(b.transform, labels[i], 28, c, TextAlignmentOptions.Center, active ? FontKind.BodyBold : FontKind.Body);
                PlaceTop(lb.rectTransform, 0, 112, 260, 44);
            }
        }

        public static ToastView BuildToast(Transform parent)
        {
            var root = Panel(parent, "Toast", new Color(0.18f, 0.12f, 0.09f, 0.94f), true, 36f);
            PlaceBottom(root.rectTransform, 0, 230, 860, 110);
            root.gameObject.AddComponent<CanvasGroup>().alpha = 0f;
            var txt = Label(root.transform, "", 32, Cream);
            Stretch(txt.rectTransform, 24, 8, 24, 8);
            var view = root.gameObject.AddComponent<ToastView>();
            SetField(view, "label", txt);
            return view;
        }

        /// <summary>Toast + penangan tombol Back Android; panggil di akhir setiap scene UI.</summary>
        public static void FinishScene(Canvas canvas)
        {
            BuildToast(canvas.transform);
            var go = new GameObject("App");
            go.AddComponent<AndroidBackHandler>();
            ApplyTheme(canvas);
        }

        /// <summary>Pasang ThemedGraphic pada semua Image/TMP agar mengikuti tema terang/gelap. Beri ThemeSkip untuk mengecualikan.</summary>
        public static void ApplyTheme(Canvas canvas)
        {
            foreach (var g in canvas.GetComponentsInChildren<Graphic>(true))
            {
                if (g.GetComponentInParent<ThemeSkip>(true) != null) continue;
                if (g.GetComponent<ThemedGraphic>() != null) continue;
                g.gameObject.AddComponent<ThemedGraphic>().Setup(g.color, g is TMP_Text);
            }
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<ThemedCamera>() == null)
                cam.gameObject.AddComponent<ThemedCamera>().Setup(cam.backgroundColor);
        }

        public static void NewUIScene(Color camBg, out Canvas canvas)
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = camBg;
            cam.orthographic = true;
            cam.cullingMask = 0;
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            canvas = CreateCanvas();
            EnsureEventSystem();
        }
    }
}
