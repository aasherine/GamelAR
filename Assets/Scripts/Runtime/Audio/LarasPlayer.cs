using UnityEngine;

namespace GamelAR
{
    /// <summary>Memutar nada laras (Pelog/Slendro) dan contoh suara di halaman Informasi.</summary>
    public class LarasPlayer : MonoBehaviour
    {
        AudioSource noteSrc;
        AudioSource sampleSrc;

        public bool SamplePlaying => sampleSrc != null && sampleSrc.isPlaying;

        void Awake()
        {
            noteSrc = gameObject.AddComponent<AudioSource>();
            noteSrc.playOnAwake = false;
            noteSrc.spatialBlend = 0f;
            sampleSrc = gameObject.AddComponent<AudioSource>();
            sampleSrc.playOnAwake = false;
            sampleSrc.loop = true;
            sampleSrc.spatialBlend = 0f;
        }

        /// <summary>pos = urutan tombol pada profil instrumen (bukan indeks skala penuh).</summary>
        public void PlayNote(InstrumentData d, bool pelog, int pos)
        {
            var prof = InstrumentProfiles.Get(d != null ? d.id : null);
            var map = pelog ? prof.pelog : prof.slendro;
            if (map == null || pos < 0 || pos >= map.Length) return;
            int scaleIdx = map[pos];

            AudioClip clip = null;
            if (d != null)
            {
                var arr = pelog ? d.larasPelog : d.larasSlendro;
                if (arr != null && pos < arr.Length) clip = arr[pos];
            }
            if (clip == null)
            {
                var global = LarasData.Load();
                if (global != null)
                {
                    var arr = pelog ? global.pelog : global.slendro;
                    if (arr != null && scaleIdx < arr.Length) clip = arr[scaleIdx];
                }
            }
            if (clip == null) clip = PlaceholderAudio.GetTone(d != null ? d.id : null, pelog, scaleIdx); // TODO: ganti dengan audio asli
            noteSrc.PlayOneShot(clip, Mathf.Clamp01(GameSettings.SfxVolume + 0.2f));
        }

        /// <summary>Untuk instrumen tanpa laras (kendang): pukulan ke-pos.</summary>
        public void PlayDrum(InstrumentData d, int pos)
        {
            noteSrc.PlayOneShot(PlaceholderAudio.GetDrum(pos), Mathf.Clamp01(GameSettings.SfxVolume + 0.2f));
        }

        public void ToggleSample(InstrumentData d)
        {
            if (sampleSrc.isPlaying) { sampleSrc.Stop(); return; }
            if (d == null) return;
            sampleSrc.clip = d.sampleLoop != null ? d.sampleLoop : PlaceholderAudio.GetLoop(d);
            sampleSrc.volume = d.loopVolume;
            sampleSrc.Play();
        }

        public void StopSample()
        {
            if (sampleSrc != null) sampleSrc.Stop();
        }
    }
}
