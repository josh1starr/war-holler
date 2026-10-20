using System.Collections.Generic;
using UnityEngine;
using WarHoller.Core;

namespace WarHoller.Audio
{
    /// <summary>
    /// Plays kill / streak / combat bark clips. Generates procedural WAV-like AudioClips at runtime
    /// if no assets are assigned — phrases are labeled so real VO can drop in later.
    /// </summary>
    public class VoiceLinePlayer : MonoBehaviour
    {
        public AudioSource Source;

        readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        public static readonly string[] KillPhrases = { "Yeehaw!" };
        public static readonly string[] TriplePhrases = { "We eatin good tonight boy!" };
        public static readonly string[] FivePhrases = { "Yippie ka-yay mother fucker" };
        public static readonly string[] CombatBarks =
        {
            "[bark] Git from my land!",
            "[bark] That all you got, cousin?",
            "[bark] Hold still now!",
            "[bark] Whoo-wee!"
        };

        void Awake()
        {
            if (Source == null)
            {
                Source = gameObject.AddComponent<AudioSource>();
                Source.playOnAwake = false;
                Source.spatialBlend = 0f;
            }
            EnsureClips();
        }

        void EnsureClips()
        {
            // Prefer Resources/Voice WAVs when present; else procedural tones.
            TryLoad("kill_yeehaw", "Voice/kill_yeehaw");
            TryLoad("streak_triple", "Voice/streak_triple");
            TryLoad("streak_five", "Voice/streak_five");
            TryLoad("bark_0", "Voice/bark_git_from_my_land");
            TryLoad("bark_1", "Voice/bark_that_all_you_got");
            TryLoad("bark_2", "Voice/bark_hold_still");
            TryLoad("bark_3", "Voice/bark_whoo_wee");

            RegisterTone("kill_yeehaw", 520f, 0.28f);
            RegisterTone("streak_triple", 380f, 0.55f);
            RegisterTone("streak_five", 300f, 0.7f);
            RegisterTone("bark_0", 440f, 0.22f);
            RegisterTone("bark_1", 360f, 0.25f);
            RegisterTone("bark_2", 480f, 0.2f);
            RegisterTone("bark_3", 400f, 0.24f);
        }

        void TryLoad(string key, string resourcesPath)
        {
            if (_clips.ContainsKey(key)) return;
            var clip = Resources.Load<AudioClip>(resourcesPath);
            if (clip != null) _clips[key] = clip;
        }

        void RegisterTone(string key, float hz, float seconds)
        {
            if (_clips.ContainsKey(key)) return;
            _clips[key] = MakeToneClip(key, hz, seconds);
        }

        public void PlayKill()
        {
            Debug.Log("[Voice] On kill: Yeehaw!");
            Play("kill_yeehaw", SettingsStore.VoiceVolume);
        }

        public void PlayTripleStreak()
        {
            Debug.Log("[Voice] Triple streak: We eatin good tonight boy!");
            Play("streak_triple", SettingsStore.VoiceVolume);
        }

        public void PlayFiveStreak()
        {
            Debug.Log("[Voice] Five streak: Yippie ka-yay mother fucker");
            Play("streak_five", SettingsStore.VoiceVolume);
        }

        public void PlayRandomBark()
        {
            var i = Random.Range(0, CombatBarks.Length);
            Debug.Log("[Voice] " + CombatBarks[i]);
            Play("bark_" + i, SettingsStore.VoiceVolume * 0.8f);
        }

        void Play(string key, float volume)
        {
            if (Source == null || !_clips.TryGetValue(key, out var clip)) return;
            Source.PlayOneShot(clip, Mathf.Clamp01(volume) * SettingsStore.SfxVolume);
        }

        static AudioClip MakeToneClip(string name, float frequency, float duration)
        {
            const int sampleRate = 22050;
            var samples = Mathf.CeilToInt(sampleRate * duration);
            var data = new float[samples];
            for (var i = 0; i < samples; i++)
            {
                var t = i / (float)sampleRate;
                var env = 1f - (t / duration);
                // Slight frequency warble so "phrases" are distinguishable tones.
                var wobble = 1f + 0.08f * Mathf.Sin(t * 18f);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * wobble * t) * env * 0.35f;
            }

            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
