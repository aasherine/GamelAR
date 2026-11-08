using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
#if GAMELAR_ARF
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

namespace GamelAR.EditorTools
{
    /// <summary>Scene PemindaiAR: rig AR Foundation + overlay UI pemindai.</summary>
    public static class PemindaiBuilder
    {
        public const string ScenePath = "Assets/Scenes/PemindaiAR.unity";

        public static void Build()
        {
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.75f, 0.72f, 0.68f);
            RenderSettings.skybox = null;
            var lightGo = new GameObject("Matahari", typeof(Light));
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
            var light = lightGo.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;

            // ---- Rig AR ----
            Behaviour[] arComponents = new Behaviour[0];
            Behaviour trackingSource = null;
            Camera cam;
#if GAMELAR_ARF
            var sessionGo = new GameObject("AR Session");
            var session = sessionGo.AddComponent<ARSession>();
            var inputMgr = sessionGo.AddComponent<ARInputManager>();

            var originGo = new GameObject("XR Origin");
            var origin = originGo.AddComponent<XROrigin>();
            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(originGo.transform, false);
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(offset.transform, false);
            cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.17f, 0.11f, 0.09f);
            cam.nearClipPlane = 0.03f;
            cam.farClipPlane = 30f;
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            var camMgr = camGo.AddComponent<ARCameraManager>();
            var camBg = camGo.AddComponent<ARCameraBackground>();
            var pose = camGo.AddComponent<TrackedPoseDriver>();
            pose.positionInput = new InputActionProperty(new InputAction("Position", InputActionType.Value, "<HandheldARInputDevice>/devicePosition", null, null, "Vector3"));
            pose.rotationInput = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, "<HandheldARInputDevice>/deviceRotation", null, null, "Quaternion"));

            origin.Camera = cam;
            origin.CameraFloorOffsetObject = offset;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;

            var imgMgr = originGo.AddComponent<ARTrackedImageManager>();
            var lib = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(GamelarXRSetup.LibraryPath);
            var src = originGo.AddComponent<ARImageTrackingSource>();
            UIKit.SetField(src, "manager", imgMgr);
            UIKit.SetField(src, "library", lib);
            UIKit.SetField(src, "maxMovingImages", 5);
            if (lib != null)
            {
                var so = new SerializedObject(imgMgr);
                var p = so.FindProperty("m_SerializedLibrary");
                if (p != null) { p.objectReferenceValue = lib; so.ApplyModifiedPropertiesWithoutUndo(); }
            }

            // Semua komponen AR dimatikan; ARSessionBootstrap menyalakannya setelah izin kamera & loader siap.
            session.enabled = false; inputMgr.enabled = false; camMgr.enabled = false; camBg.enabled = false; imgMgr.enabled = false;
            arComponents = new Behaviour[] { inputMgr, camMgr, camBg, imgMgr, session };
            trackingSource = src;
#else
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.17f, 0.11f, 0.09f);
            camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#endif

            // ---- Logika ----
            var logic = new GameObject("ARLogic");
            var poolGo = new GameObject("ModelPool");
            poolGo.transform.SetParent(logic.transform, false);
            var pool = poolGo.AddComponent<ModelPool>();
            UIKit.SetField(pool, "tagFont", UIKit.GetFont(UIKit.FontKind.BodyBold));
            var cards = logic.AddComponent<ARCardManager>();
            UIKit.SetField(cards, "pool", pool);
            logic.AddComponent<AudioMixerManager>();
            logic.AddComponent<EditorCardSimulator>();
            var boot = logic.AddComponent<ARSessionBootstrap>();
            UIKit.SetArray(boot, "arComponents", arComponents);
            if (trackingSource != null) UIKit.SetField(boot, "trackingSource", trackingSource);
            logic.AddComponent<AndroidBackHandler>();

            // ---- UI ----
            var canvas = UIKit.CreateCanvas();
            UIKit.EnsureEventSystem();
            var root = UIKit.Rect("Root", canvas.transform);
            UIKit.Stretch(root);

            // Status (izin kamera / tidak didukung): di bawah overlay agar tombol kembali tetap terlihat
            var status = BuildStatus(canvas.transform, out var sTitle, out var sBody, out var sBtn, out var sBtnLabel);

            var safe = UIKit.Rect("SafeArea", root);
            UIKit.Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter>();

            // Bilah atas
            var scrim = UIKit.Panel(root, "ScrimAtas", new Color(0.12f, 0.08f, 0.06f, 0.62f), false);
            UIKit.StretchTopBar(scrim.rectTransform, 330);
            scrim.transform.SetAsFirstSibling();

            var backMain = UIKit.BackButtonWidget(safe, new Color(1f, 0.97f, 0.9f, 0.95f), Palette.BrownDark, -440f, 24f);
            backMain.gameObject.AddComponent<ThemeSkip>();
            var title = UIKit.Label(safe, "Pindai Kartu", 48, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Judul");
            UIKit.PlaceTop(title.rectTransform, 0, 36, 600, 90);

            var counterBg = UIKit.Panel(safe, "PilKartu", new Color(0.365f, 0.251f, 0.216f, 0.92f), true, 40f);
            UIKit.PlaceTop(counterBg.rectTransform, 0, 160, 560, 80);
            var counter = UIKit.Label(counterBg.transform, "Kartu terdeteksi: 0", 34, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(counter.rectTransform);

            var simBadge = UIKit.Panel(safe, "LencanaSimulasi", Palette.Gold, true, 30f);
            UIKit.PlaceTop(simBadge.rectTransform, 0, 262, 560, 54);
            var simTxt = UIKit.Label(simBadge.transform, "MODE SIMULASI (Editor)", 26, Palette.OnGold, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(simTxt.rectTransform);

            // Bingkai pindai
            var frame = UIKit.Rect("BingkaiPindai", safe);
            UIKit.PlaceCenter(frame, 0, 20, 720, 1000);
            Corner(frame, new Vector2(0, 1), 1, -1);
            Corner(frame, new Vector2(1, 1), -1, -1);
            Corner(frame, new Vector2(0, 0), 1, 1);
            Corner(frame, new Vector2(1, 0), -1, 1);

            var hint = UIKit.Panel(safe, "PetunjukPil", new Color(0.12f, 0.08f, 0.06f, 0.7f), true, 40f);
            UIKit.PlaceCenter(hint.rectTransform, 0, -530, 780, 84);
            var hintTxt = UIKit.Label(hint.transform, "Arahkan kamera ke kartu", 38, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyMedium);
            UIKit.Stretch(hintTxt.rectTransform);

            // Panel bawah
            var bottom = UIKit.Panel(safe, "PanelBawah", new Color(Palette.Surface.r, Palette.Surface.g, Palette.Surface.b, 0.96f), true, 52f);
            UIKit.PlaceBottom(bottom.rectTransform, 0, 40, 980, 320);
            bottom.raycastTarget = true;
            var names = UIKit.Label(bottom.transform, "Belum ada kartu", 36, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyBold, "Nama");
            UIKit.PlaceLeftTop(names.rectTransform, 48, 26, 660, 120);
            var info = UIKit.Chip(bottom.transform, "Btn_Info", "INFO", Palette.Brown, Palette.Cream, 30, UIKit.FontKind.BodyBold, 40f);
            UIKit.PlaceRightTop((RectTransform)info.transform, 40, 44, 210, 84);
            var line = UIKit.Panel(bottom.transform, "Garis", Palette.SurfaceLine, false);
            UIKit.PlaceTop(line.rectTransform, 0, 160, 884, 3);
            var gl = UIKit.Label(bottom.transform, "Gabung Kartu", 36, Palette.BrownDark, TextAlignmentOptions.MidlineLeft, UIKit.FontKind.BodyMedium, "Label");
            UIKit.PlaceLeftTop(gl.rectTransform, 48, 186, 600, 50);
            var gs = UIKit.Label(bottom.transform, "Campur suara semua kartu yang terdeteksi", 26, Palette.Muted, TextAlignmentOptions.TopLeft, UIKit.FontKind.Body, "Sub");
            UIKit.PlaceLeftTop(gs.rectTransform, 48, 238, 640, 70);
            var sw = UIKit.MakeSwitch(bottom.transform, "Saklar_Gabung");
            UIKit.PlaceRightTop((RectTransform)sw.transform, 48, 196, 124, 68);

            UIKit.BuildToast(canvas.transform);
            UIKit.ApplyTheme(canvas);

            // Layar status paling atas, tombol kembali duplikat
            status.transform.SetAsLastSibling();

            var ui = root.gameObject.AddComponent<ScannerUI>();
            UIKit.SetField(ui, "scanFrame", frame.gameObject);
            UIKit.SetField(ui, "hintPill", hint.gameObject);
            UIKit.SetField(ui, "counterText", counter);
            UIKit.SetField(ui, "bottomPanel", bottom.gameObject);
            UIKit.SetField(ui, "namesText", names);
            UIKit.SetField(ui, "infoButton", info);
            UIKit.SetField(ui, "mergeToggle", sw);
            UIKit.SetField(ui, "statusRoot", status);
            UIKit.SetField(ui, "statusTitle", sTitle);
            UIKit.SetField(ui, "statusBody", sBody);
            UIKit.SetField(ui, "statusButton", sBtn);
            UIKit.SetField(ui, "statusButtonLabel", sBtnLabel);
            UIKit.SetField(ui, "simBadge", simBadge.gameObject);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log("[GamelAR] Scene PemindaiAR dibuat: " + ScenePath);
        }

        // Sudut bingkai berbentuk "L"
        static void Corner(RectTransform frame, Vector2 anchor, int dirX, int dirY)
        {
            const float len = 130f, th = 12f;
            var h = UIKit.Panel(frame, "SudutH", Palette.Gold, true, 6f);
            h.rectTransform.anchorMin = h.rectTransform.anchorMax = anchor;
            h.rectTransform.pivot = new Vector2(dirX > 0 ? 0f : 1f, dirY > 0 ? 0f : 1f);
            h.rectTransform.sizeDelta = new Vector2(len, th);
            h.rectTransform.anchoredPosition = Vector2.zero;
            var v = UIKit.Panel(frame, "SudutV", Palette.Gold, true, 6f);
            v.rectTransform.anchorMin = v.rectTransform.anchorMax = anchor;
            v.rectTransform.pivot = new Vector2(dirX > 0 ? 0f : 1f, dirY > 0 ? 0f : 1f);
            v.rectTransform.sizeDelta = new Vector2(th, len);
            v.rectTransform.anchoredPosition = Vector2.zero;
        }

        static GameObject BuildStatus(Transform canvas, out TMP_Text title, out TMP_Text body, out Button button, out TMP_Text buttonLabel)
        {
            var bg = UIKit.Panel(canvas, "LayarStatus", Palette.Bg, false);
            bg.raycastTarget = true;
            UIKit.Stretch(bg.rectTransform);
            var safe = UIKit.Rect("Aman", bg.transform);
            UIKit.Stretch(safe);
            safe.gameObject.AddComponent<SafeAreaFitter>();

            UIKit.KawungPattern(safe, "Batik", 0, 0, 6, 2, 190, new Color(1f, 0.70f, 0f, 0.11f));
            UIKit.BackButtonWidget(safe, Palette.SurfaceAlt, Palette.BrownDark, -440f, 24f);

            var logo = UIKit.CirclePanel(safe, "Logo", Palette.Brown);
            UIKit.PlaceCenter(logo.rectTransform, 0, 330, 260, 260);
            var ic = UIKit.AppLogo(logo.transform, 244f);
            UIKit.PlaceCenter(ic.rectTransform, 0, 0, 244, 244);

            title = UIKit.Label(safe, "Menyiapkan kamera...", 54, Palette.BrownDark, TextAlignmentOptions.Center, UIKit.FontKind.Heading, "Judul");
            UIKit.PlaceCenter(title.rectTransform, 0, 90, 900, 150);
            body = UIKit.Label(safe, "Mohon tunggu sebentar.", 34, Palette.BrownMid, TextAlignmentOptions.Top, UIKit.FontKind.Body, "Isi");
            body.lineSpacing = 10f;
            UIKit.PlaceCenter(body.rectTransform, 0, -120, 820, 340);

            button = UIKit.MakeButton(safe, "Btn_Aksi", Palette.Brown, 48f);
            UIKit.PlaceCenter((RectTransform)button.transform, 0, -420, 760, 120);
            buttonLabel = UIKit.Label(button.transform, "COBA LAGI", 38, Palette.Cream, TextAlignmentOptions.Center, UIKit.FontKind.BodyBold);
            UIKit.Stretch(buttonLabel.rectTransform);
            button.gameObject.SetActive(false);
            return bg.gameObject;
        }
    }
}
