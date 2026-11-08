using System.Collections.Generic;
using UnityEngine;

namespace GamelAR
{
    /// <summary>
    /// Suara PLACEHOLDER yang dibuat lewat kode (sintesis), dipakai selama file audio asli belum ada.
    /// TODO: isi InstrumentData.sampleLoop (dan LarasData) dengan rekaman asli; placeholder otomatis berhenti dipakai.
    /// Semua loop berdurasi tepat 4 detik sehingga beberapa kartu selalu selaras (sinkron).
    /// </summary>
    public static class PlaceholderAudio
    {
        public const int Rate = 22050;
        public const float LoopSeconds = 4f;

        // Sentimeter nada (cent) dari nada pertama; urutan sesuai InstrumentData
        static readonly float[] PelogCents = { 0, 120, 270, 540, 670, 785, 950, 1200 };     // 1 2 3 4 5 6 7 1'
        static readonly float[] SlendroCents = { 0, 240, 480, 720, 960, 1200 };             // 1 2 3 5 6 1'
        const float BaseHz = 261.63f;

        enum Kind { Metal, Wood, Pluck, Bowed, Flute, Drum }

        struct Recipe
        {
            public Kind kind; public float baseHz; public float decay; public float step; public int[] pattern;
        }

        static readonly Dictionary<string, AudioClip> loops = new Dictionary<string, AudioClip>();
        static readonly Dictionary<int, AudioClip> tones = new Dictionary<int, AudioClip>();

        public static float Frequency(bool pelog, int index, float baseHz = BaseHz)
        {
            var cents = pelog ? PelogCents : SlendroCents;
            index = Mathf.Clamp(index, 0, cents.Length - 1);
            return baseHz * Mathf.Pow(2f, cents[index] / 1200f);
        }

        // ---------- Nada laras (Coba Laras) per instrumen ----------
        /// <summary>Nada placeholder untuk tombol Coba Laras; tinggi nada dan warna bunyi mengikuti instrumen.</summary>
        public static AudioClip GetTone(string instrumentId, bool pelog, int scaleIndex)
        {
            string key = instrumentId + (pelog ? "_p" : "_s") + scaleIndex;
            if (tones.TryGetValue(key.GetHashCode(), out var c) && c != null) return c;
            var prof = InstrumentProfiles.Get(instrumentId);
            var r = RecipeFor(instrumentId);
            float hz = Frequency(pelog, scaleIndex, prof.baseHz);
            float len = r.kind == Kind.Bowed || r.kind == Kind.Flute ? 1.3f : 2.2f;
            var buf = new float[Mathf.RoundToInt(len * Rate)];
            AddNote(buf, 0f, hz, r.kind, Mathf.Min(r.decay, 6f), 1f, false, len);
            Normalize(buf, 0.7f);
            c = AudioClip.Create("tone_" + key, buf.Length, 1, Rate, false);
            c.SetData(buf, 0);
            tones[key.GetHashCode()] = c;
            return c;
        }

        /// <summary>Pukulan kendhang placeholder (0 Tak, 1 Dhah, 2 Tung, 3 Dhet).</summary>
        public static AudioClip GetDrum(int stroke)
        {
            int key = 9000 + stroke;
            if (tones.TryGetValue(key, out var c) && c != null) return c;
            float[] hz = { 230f, 150f, 105f, 190f };
            float[] decay = { 22f, 9f, 6f, 30f };
            var buf = new float[Mathf.RoundToInt(0.7f * Rate)];
            AddDrumHit(buf, hz[Mathf.Clamp(stroke, 0, 3)], decay[Mathf.Clamp(stroke, 0, 3)]);
            Normalize(buf, 0.8f);
            c = AudioClip.Create("kendhang_" + stroke, buf.Length, 1, Rate, false);
            c.SetData(buf, 0);
            tones[key] = c;
            return c;
        }

        static void AddDrumHit(float[] buf, float hz, float decay)
        {
            uint seed = 12345u + (uint)hz;
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float f = hz * (1f + 1.2f * Mathf.Exp(-t * 35f));
                seed = seed * 1664525u + 1013904223u;
                float noise = ((seed >> 9) / 4194304f - 1f);
                buf[i] = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.9f + noise * 0.3f * Mathf.Exp(-t * 70f)) * Mathf.Exp(-t * decay);
            }
        }

        // ---------- Loop per instrumen ----------
        public static AudioClip GetLoop(InstrumentData d)
        {
            string id = d != null ? d.id : "default";
            if (loops.TryGetValue(id, out var c) && c != null) return c;

            var r = RecipeFor(id);
            int total = Mathf.RoundToInt(LoopSeconds * Rate);
            var buf = new float[total];
            int steps = Mathf.RoundToInt(LoopSeconds / r.step);
            for (int s = 0; s < steps; s++)
            {
                int deg = r.pattern[s % r.pattern.Length];
                if (deg < 0) continue;
                float hz = r.baseHz * Mathf.Pow(2f, SlendroCents[deg % SlendroCents.Length] / 1200f);
                AddNote(buf, s * r.step, hz, r.kind, r.decay, 1f, true);
            }
            Normalize(buf, 0.65f);
            c = AudioClip.Create("loop_" + id, total, 1, Rate, false);
            c.SetData(buf, 0);
            loops[id] = c;
            return c;
        }

        static Recipe RecipeFor(string id)
        {
            switch (id)
            {
                case "BonangBarung": return R(Kind.Metal, 523f, 4.5f, 0.5f, 0, 2, 4, 2, 1, 3, 5, 3);
                case "BonangPenerus": return R(Kind.Metal, 784f, 6f, 0.25f, 0, 4, 2, 4, 1, 5, 3, 5, 2, 4, 1, 3, 0, 2, 4, 5);
                case "Demung": return R(Kind.Metal, 262f, 3f, 1f, 0, 2, 1, 3);
                case "SaronBarung": return R(Kind.Metal, 523f, 5f, 0.5f, 0, -1, 2, -1, 1, -1, 4, -1);
                case "Slenthem": return R(Kind.Metal, 131f, 2.2f, 2f, 0, 3);
                case "Gambang": return R(Kind.Wood, 392f, 11f, 0.25f, 0, 2, 4, 2, 1, 3, 5, 3, 2, 4, 5, 4, 3, 1, 2, 0);
                case "GenderBarung": return R(Kind.Pluck, 330f, 3.5f, 0.25f, 0, 1, 2, 4, 3, 2, 1, 0, 2, 3, 4, 5, 4, 3, 2, 1);
                case "GongAgeng": return R(Kind.Metal, 65f, 0.9f, 4f, 0);
                case "Kempul": return R(Kind.Metal, 98f, 1.6f, 2f, 3, 1);
                case "Kenong": return R(Kind.Metal, 196f, 2.5f, 1f, 2, -1, 4, -1);
                case "Kethuk": return R(Kind.Metal, 392f, 14f, 0.5f, 2, 2, 2, 2, 2, 2, 2, 2);
                case "Kendhang": return R(Kind.Drum, 110f, 12f, 0.25f, 0, -1, 1, 0, -1, 2, 1, -1, 0, 0, 1, -1, 2, -1, 0, 1);
                case "Rebab": return R(Kind.Bowed, 392f, 0.8f, 0.5f, 0, 1, 2, 1, 2, 3, 2, 1);
                case "Siter": return R(Kind.Pluck, 262f, 3f, 0.25f, 0, 2, 4, 3, 1, 3, 5, 4, 2, 4, 5, 3, 1, 2, 0, 1);
                case "Suling": return R(Kind.Flute, 523f, 1.0f, 1f, 4, 3, 2, 1);
                default: return R(Kind.Metal, 392f, 4f, 0.5f, 0, 2, 4, 2);
            }
        }

        static Recipe R(Kind kind, float hz, float decay, float step, params int[] pattern) =>
            new Recipe { kind = kind, baseHz = hz, decay = decay, step = step, pattern = pattern };

        // Menambahkan satu nada ke buffer; ekor yang melewati batas dibungkus ke awal agar loop mulus.
        static void AddNote(float[] buf, float startSec, float hz, Kind kind, float decay, float amp, bool wrap, float durOverride = 0f)
        {
            int n = buf.Length;
            int start = Mathf.RoundToInt(startSec * Rate);
            float dur = kind == Kind.Bowed ? 0.7f : kind == Kind.Flute ? 0.9f : 2.4f;
            if (decay > 8f) dur = 0.6f;
            if (durOverride > 0f) dur = durOverride;
            int len = Mathf.RoundToInt(dur * Rate);
            float[] ratios; float[] amps;
            switch (kind)
            {
                case Kind.Metal: ratios = new[] { 1f, 2.76f, 5.4f, 8.9f }; amps = new[] { 1f, 0.5f, 0.28f, 0.12f }; break;
                case Kind.Wood: ratios = new[] { 1f, 3.9f, 7f }; amps = new[] { 1f, 0.45f, 0.2f }; break;
                case Kind.Pluck: ratios = new[] { 1f, 2f, 3f, 4f, 5f }; amps = new[] { 1f, 0.6f, 0.4f, 0.25f, 0.12f }; break;
                case Kind.Bowed: ratios = new[] { 1f, 2f, 3f, 4f }; amps = new[] { 1f, 0.38f, 0.16f, 0.06f }; break;
                case Kind.Flute: ratios = new[] { 1f, 2f, 3f }; amps = new[] { 1f, 0.18f, 0.06f }; break;
                default: ratios = new[] { 1f }; amps = new[] { 1f }; break;
            }
            uint seed = (uint)(start * 2654435761u + 17u);
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)Rate;
                float env;
                float v = 0f;
                if (kind == Kind.Drum)
                {
                    float f = hz * (1f + 1.6f * Mathf.Exp(-t * 30f));
                    seed = seed * 1664525u + 1013904223u;
                    float noise = ((seed >> 9) / 4194304f - 1f);
                    env = Mathf.Exp(-t * decay);
                    v = (Mathf.Sin(2f * Mathf.PI * f * t) * 0.9f + noise * 0.25f * Mathf.Exp(-t * 60f)) * env;
                }
                else
                {
                    float attack = kind == Kind.Bowed ? 0.14f : kind == Kind.Flute ? 0.08f : 0.004f;
                    float a = Mathf.Clamp01(t / attack);
                    env = a * Mathf.Exp(-t * decay);
                    if (kind == Kind.Bowed || kind == Kind.Flute)
                    {
                        float rel = Mathf.Clamp01((dur - t) / (kind == Kind.Bowed ? 0.3f : 0.2f));
                        env = a * rel;
                    }
                    float vib = kind == Kind.Bowed ? 1f + 0.003f * Mathf.Sin(2f * Mathf.PI * 5f * t) : 1f;
                    for (int p = 0; p < ratios.Length; p++)
                    {
                        float d = kind == Kind.Metal || kind == Kind.Wood ? Mathf.Exp(-t * decay * 0.35f * p) : 1f;
                        v += Mathf.Sin(2f * Mathf.PI * hz * ratios[p] * vib * t) * amps[p] * d;
                    }
                    if (kind == Kind.Flute)
                    {
                        seed = seed * 1664525u + 1013904223u;
                        v += ((seed >> 9) / 4194304f - 1f) * 0.05f;
                    }
                    v *= env;
                }
                int idx = start + i;
                if (idx >= n)
                {
                    if (!wrap) break;
                    idx %= n;
                }
                buf[idx] += v * amp * 0.35f;
            }
        }

        static void Normalize(float[] buf, float peak)
        {
            float max = 0.0001f;
            for (int i = 0; i < buf.Length; i++) max = Mathf.Max(max, Mathf.Abs(buf[i]));
            float g = peak / max;
            for (int i = 0; i < buf.Length; i++) buf[i] *= g;
        }
    }
}
