using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Alat uji di Unity Editor (tanpa HP): saat mode Simulasi aktif, panel tombol kecil muncul di layar Game.
    /// Klik nama instrumen untuk "menaruh" / "mengangkat" kartunya, lalu lihat model 3D, label, penghitung dan audio bereaksi.
    /// Tidak aktif di perangkat Android sungguhan.
    /// </summary>
    public class EditorCardSimulator : MonoBehaviour
    {
#if UNITY_EDITOR
        InstrumentData[] items;
        Transform[] anchors;
        bool[] on;
        bool panel = false;
        Vector2 scroll;

        void Start()
        {
            var db = InstrumentDatabase.Load();
            if (db == null) return;
            items = db.instruments.ToArray();
            anchors = new Transform[items.Length];
            on = new bool[items.Length];

            var mat = MakeMat(new Color(0.99f, 0.96f, 0.88f));
            for (int i = 0; i < items.Length; i++)
            {
                var root = new GameObject("SimKartu_" + items[i].id).transform;
                root.SetParent(transform, false);
                root.position = new Vector3(((i % 5) - 2) * 0.14f, -0.2f, 0.42f + (i / 5) * 0.2f);
                anchors[i] = root;
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(quad.GetComponent<Collider>());
                quad.transform.SetParent(root, false);
                quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                quad.transform.localScale = new Vector3(0.105f, 0.148f, 1f);
                quad.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        static Material MakeMat(Color c)
        {
            var sh = Shader.Find("Universal Render Pipeline/Unlit");
            if (sh == null) sh = Shader.Find("Sprites/Default");
            var m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            m.color = c;
            return m;
        }

        void Update()
        {
            if (ARSessionBootstrap.State != ARState.Simulation || items == null) return;
            var cam = Camera.main;
            if (cam != null)
            {
                var pos = new Vector3(0f, 0.38f, -0.1f);
                cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(new Vector3(0f, -0.2f, 0.6f) - pos));
            }
            var cm = ARCardManager.Instance;
            if (cm == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                if (anchors[i] != null) anchors[i].gameObject.SetActive(on[i]);
                if (on[i]) cm.ReportTracked(items[i].referenceImageName, anchors[i]);
            }
        }

        GUIStyle btnOff, btnOn, titleStyle;
        Texture2D texOff, texOn, texBar;

        static Texture2D Solid(Color c) { var t = new Texture2D(1, 1); t.SetPixel(0, 0, c); t.Apply(); return t; }

        void BuildStyles()
        {
            texOff = Solid(new Color(0.16f, 0.11f, 0.09f, 0.95f));
            texOn = Solid(new Color(1f, 0.70f, 0f, 1f));
            texBar = Solid(new Color(0.07f, 0.05f, 0.04f, 0.96f));
            btnOff = new GUIStyle(GUI.skin.button) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            btnOff.normal.background = texOff; btnOff.hover.background = texOff; btnOff.active.background = texOff;
            btnOff.normal.textColor = btnOff.hover.textColor = btnOff.active.textColor = Color.white;
            btnOn = new GUIStyle(btnOff);
            btnOn.normal.background = texOn; btnOn.hover.background = texOn; btnOn.active.background = texOn;
            btnOn.normal.textColor = btnOn.hover.textColor = btnOn.active.textColor = new Color(0.24f, 0.15f, 0.14f);
            titleStyle = new GUIStyle(btnOff);
        }

        void OnGUI()
        {
            if (ARSessionBootstrap.State != ARState.Simulation || items == null) return;
            if (btnOff == null) BuildStyles();
            float s = Screen.width / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            int count = 0; foreach (var o in on) if (o) count++;
            if (GUI.Button(new Rect(24, 200, 190, 60), panel ? "Tutup sim" : "Sim (" + count + ")", count > 0 && !panel ? btnOn : btnOff)) panel = !panel;
            if (!panel) return;
            const float cw = 340, ch = 64, gap = 10;
            float x0 = 24, y0 = 272;
            int rows = (items.Length + 2) / 3;
            GUI.DrawTexture(new Rect(x0 - 10, y0 - 10, 3 * cw + 2 * gap + 20, rows * (ch + gap) + 10), texBar);
            for (int i = 0; i < items.Length; i++)
            {
                var r = new Rect(x0 + (i % 3) * (cw + gap), y0 + (i / 3) * (ch + gap), cw, ch);
                string label = (on[i] ? "[ON]  " : "") + items[i].displayName;
                if (GUI.Button(r, label, on[i] ? btnOn : btnOff)) { on[i] = !on[i]; panel = false; }
            }
        }
#endif
    }
}
