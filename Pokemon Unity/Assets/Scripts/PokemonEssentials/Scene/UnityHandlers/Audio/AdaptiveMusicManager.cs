using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace PokemonUnity.Interface.UnityEngine
{
    /// <summary>
    /// Manages smooth crossfading between music contexts (overworld, battle, etc.)
    /// using two <see cref="AudioSource"/> components as a double-buffer.
    /// </summary>
    public class AdaptiveMusicManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static AdaptiveMusicManager Instance { get; private set; }

        // ── Context enum ──────────────────────────────────────────────────────
        public enum MusicContext
        {
            None,
            Menu,
            Overworld,
            Battle,
            BattleLowHp,
            Victory,
            Intro,
            Caught
        }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Serializable]
        public class MusicContextEntry
        {
            public MusicContext context;
            public AudioClip    clip;
            [Range(0f, 1f)] public float volume = 1f;
            public bool loop = true;
        }

        [Header("Sources")]
        [SerializeField] private AudioSource primarySource;
        [SerializeField] private AudioSource secondarySource;

        [Header("Mixer")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;

        [Header("Transitions")]
        [SerializeField] private float crossfadeDuration = 1.5f;

        [Header("Context Clips")]
        [SerializeField] private MusicContextEntry[] contextClips;

        // ── State ─────────────────────────────────────────────────────────────
        private MusicContext   _current    = MusicContext.None;
        private Coroutine      _fadeCo;
        private bool           _paused;

        public MusicContext CurrentContext => _current;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else                  { Destroy(gameObject); return; }

            if (musicMixerGroup != null)
            {
                primarySource.outputAudioMixerGroup   = musicMixerGroup;
                secondarySource.outputAudioMixerGroup = musicMixerGroup;
            }

            primarySource.volume   = 1f;
            secondarySource.volume = 0f;
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Transition to a new music context.</summary>
        public void TransitionTo(MusicContext context, bool immediate = false)
        {
            if (context == _current) return;

            MusicContextEntry entry = FindEntry(context);
            if (entry == null)
            {
                Debug.LogWarning($"[AdaptiveMusicManager] No clip registered for context: {context}");
                return;
            }

            _current = context;

            if (_fadeCo != null) StopCoroutine(_fadeCo);
            _fadeCo = StartCoroutine(immediate
                ? ImmediateSwitch(entry)
                : Crossfade(entry));
        }

        /// <summary>Fade master volume to <paramref name="volume"/> over <paramref name="dur"/> seconds.</summary>
        public void SetVolume(float volume, float dur = 0.5f)
            => StartCoroutine(FadeVolume(primarySource, primarySource.volume, volume, dur));

        /// <summary>Apply a low-pass filter effect (e.g., for underwater or menu blur).</summary>
        public void SetLowPass(bool active, float dur = 0.3f)
        {
            // AudioMixerGroup snapshot approach:
            // musicMixerGroup?.audioMixer.FindSnapshot(active ? "LowPass" : "Normal")?.TransitionTo(dur);
            // Implemented as a stub — set up snapshots in the AudioMixer asset.
            Debug.Log($"[AdaptiveMusicManager] Low-pass {(active ? "ON" : "OFF")} (set up AudioMixer snapshots).");
        }

        /// <summary>Pause the current track with a fade-out.</summary>
        public void Pause(float fadeDur = 0.5f)
        {
            if (_paused) return;
            _paused = true;
            StartCoroutine(FadeVolume(primarySource, primarySource.volume, 0f, fadeDur,
                onComplete: () => primarySource.Pause()));
        }

        /// <summary>Resume a paused track with a fade-in.</summary>
        public void Resume(float fadeDur = 0.5f)
        {
            if (!_paused) return;
            _paused = false;
            primarySource.UnPause();
            StartCoroutine(FadeVolume(primarySource, 0f, 1f, fadeDur));
        }

        // ── Coroutines ────────────────────────────────────────────────────────
        private IEnumerator Crossfade(MusicContextEntry entry)
        {
            // Set up the secondary source with the new clip
            secondarySource.clip   = entry.clip;
            secondarySource.loop   = entry.loop;
            secondarySource.volume = 0f;
            secondarySource.Play();

            float elapsed  = 0f;
            float startVol = primarySource.volume;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t  = elapsed / crossfadeDuration;
                primarySource.volume   = Mathf.Lerp(startVol,         0f,              t);
                secondarySource.volume = Mathf.Lerp(0f,               entry.volume,    t);
                yield return null;
            }

            primarySource.Stop();
            primarySource.volume = entry.volume;

            // Swap sources so primary is always the active one
            (primarySource, secondarySource) = (secondarySource, primarySource);

            _fadeCo = null;
        }

        private IEnumerator ImmediateSwitch(MusicContextEntry entry)
        {
            primarySource.Stop();
            primarySource.clip   = entry.clip;
            primarySource.loop   = entry.loop;
            primarySource.volume = entry.volume;
            primarySource.Play();
            _fadeCo = null;
            yield break;
        }

        private IEnumerator FadeVolume(AudioSource src, float from, float to, float dur,
                                        Action onComplete = null)
        {
            float elapsed = 0f;
            while (elapsed < dur)
            {
                elapsed    += Time.deltaTime;
                src.volume  = Mathf.Lerp(from, to, elapsed / dur);
                yield return null;
            }
            src.volume = to;
            onComplete?.Invoke();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private MusicContextEntry FindEntry(MusicContext ctx)
        {
            if (contextClips == null) return null;
            foreach (var e in contextClips)
                if (e.context == ctx) return e;
            return null;
        }
    }
}
