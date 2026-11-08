using System.Collections.Generic;

namespace GamelAR
{
    /// <summary>Profil "Coba Laras" tiap instrumen: nada yang tersedia, register (tinggi-rendah) dan jenis bunyi.</summary>
    public class InstrumentProfile
    {
        public float baseHz = 392f;
        public int[] pelog;      // indeks pada skala pelog (0..7 = 1,2,3,4,5,6,7,1')
        public int[] slendro;    // indeks pada skala slendro (0..5 = 1,2,3,5,6,1')
        public string[] drum;    // bila bukan null: instrumen tanpa laras, tampilkan pukulan

        public bool IsDrum => drum != null;
        public string[] PelogLabels => Labels(pelog, PelogNames);
        public string[] SlendroLabels => Labels(slendro, SlendroNames);

        public static readonly string[] PelogNames = { "1", "2", "3", "4", "5", "6", "7", "1'" };
        public static readonly string[] SlendroNames = { "1", "2", "3", "5", "6", "1'" };

        static string[] Labels(int[] idx, string[] names)
        {
            var l = new string[idx.Length];
            for (int i = 0; i < idx.Length; i++) l[i] = names[idx[i]];
            return l;
        }
    }

    public static class InstrumentProfiles
    {
        static int[] R(int a, int b) { var r = new int[b - a + 1]; for (int i = 0; i < r.Length; i++) r[i] = a + i; return r; }

        static readonly Dictionary<string, InstrumentProfile> map = new Dictionary<string, InstrumentProfile>
        {
            { "BonangBarung",  new InstrumentProfile { baseHz = 523f,  pelog = R(0, 6), slendro = R(0, 4) } },
            { "BonangPenerus", new InstrumentProfile { baseHz = 1046f, pelog = R(0, 6), slendro = R(0, 4) } },
            { "Demung",        new InstrumentProfile { baseHz = 262f,  pelog = R(0, 6), slendro = R(0, 5) } },
            { "SaronBarung",   new InstrumentProfile { baseHz = 523f,  pelog = R(0, 7), slendro = R(0, 5) } },
            { "Slenthem",      new InstrumentProfile { baseHz = 131f,  pelog = R(0, 6), slendro = R(0, 5) } },
            { "GenderBarung",  new InstrumentProfile { baseHz = 330f,  pelog = R(0, 6), slendro = R(0, 5) } },
            { "Gambang",       new InstrumentProfile { baseHz = 392f,  pelog = R(0, 7), slendro = R(0, 5) } },
            { "Kenong",        new InstrumentProfile { baseHz = 196f,  pelog = R(0, 6), slendro = R(0, 4) } },
            { "Kempul",        new InstrumentProfile { baseHz = 98f,   pelog = new[] { 1, 2, 4, 5, 6 }, slendro = new[] { 1, 2, 3, 4 } } },
            { "GongAgeng",     new InstrumentProfile { baseHz = 65f,   pelog = new[] { 5 }, slendro = new[] { 4 } } },
            { "Kethuk",        new InstrumentProfile { baseHz = 392f,  pelog = new[] { 5 }, slendro = new[] { 4 } } },
            { "Kendhang",      new InstrumentProfile { baseHz = 110f,  drum = new[] { "Tak", "Dhah", "Tung", "Dhet" } } },
            { "Rebab",         new InstrumentProfile { baseHz = 392f,  pelog = R(0, 6), slendro = R(0, 5) } },
            { "Suling",        new InstrumentProfile { baseHz = 523f,  pelog = R(0, 7), slendro = R(0, 5) } },
            { "Siter",         new InstrumentProfile { baseHz = 262f,  pelog = R(0, 7), slendro = R(0, 5) } },
        };

        static readonly InstrumentProfile fallback = new InstrumentProfile { pelog = R(0, 7), slendro = R(0, 5) };

        public static InstrumentProfile Get(string id) => id != null && map.TryGetValue(id, out var p) ? p : fallback;
    }
}
