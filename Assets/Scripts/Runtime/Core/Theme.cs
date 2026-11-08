using UnityEngine;

namespace GamelAR
{
    /// <summary>Pemetaan warna tema terang ke tema gelap. Warna palet dipetakan otomatis oleh ThemedGraphic.</summary>
    public static class Theme
    {
        public static bool Dark => GameSettings.DarkTheme;

        static bool Same(Color a, Color b) => ColorUtility.ToHtmlStringRGB(a) == ColorUtility.ToHtmlStringRGB(b);

        /// <summary>Warna untuk tema aktif. 'text' = warna dipakai sebagai warna huruf (bukan isi/latar).</summary>
        public static Color Map(Color c, bool text = false)
        {
            if (!Dark) return c;
            Color r = c;
            if (Same(c, Palette.Bg)) r = Palette.Hex("1C1512");
            else if (Same(c, Palette.Surface)) r = Palette.Hex("2A201A");
            else if (Same(c, Palette.SurfaceAlt)) r = Palette.Hex("3A2D25");
            else if (Same(c, Palette.SurfaceLine)) r = Palette.Hex("4D3D32");
            else if (Same(c, Palette.Brown)) r = text ? Palette.Hex("E8CFA8") : Palette.Hex("8A6652");
            else if (Same(c, Palette.BrownDark)) r = Palette.Hex("F4EADB");
            else if (Same(c, Palette.BrownMid)) r = Palette.Hex("C7AB95");
            else if (Same(c, Palette.Muted)) r = Palette.Hex("B1A294");
            else if (Same(c, Palette.SwitchOff)) r = Palette.Hex("5A4B40");
            r.a = c.a;
            return r;
        }
    }
}
