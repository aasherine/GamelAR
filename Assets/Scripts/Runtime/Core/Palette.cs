using UnityEngine;

namespace GamelAR
{
    /// <summary>Palet warna aplikasi (krem, cokelat kayu, emas). Dipakai oleh script runtime dan pembangun UI.</summary>
    public static class Palette
    {
        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static readonly Color Bg = Hex("FDFBF7");          // krem utama
        public static readonly Color Surface = Hex("FFFFFF");     // panel
        public static readonly Color SurfaceAlt = Hex("F4ECDD");  // chip / area sekunder
        public static readonly Color SurfaceLine = Hex("EADFCB");
        public static readonly Color Brown = Hex("5D4037");       // tombol utama (cokelat kayu)
        public static readonly Color BrownDark = Hex("3E2723");   // teks utama
        public static readonly Color BrownMid = Hex("8D6E63");
        public static readonly Color Gold = Hex("FFB300");        // aksen
        public static readonly Color GoldSoft = Hex("FFE2A0");
        public static readonly Color Cream = Hex("FDF6E7");
        public static readonly Color OnGold = Hex("3E2724");      // teks/ikon di atas emas (tidak ikut tema gelap)
        public static readonly Color Muted = Hex("9A8B7E");
        public static readonly Color Mint = Hex("7ED9B8");
        public static readonly Color SwitchOff = Hex("D9CFC0");
        public static readonly Color DarkScrim = new Color(0.12f, 0.08f, 0.06f, 0.55f);
    }
}
