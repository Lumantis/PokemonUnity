using System;
using System.Collections;
using System.Collections.Generic;
using PokemonEssentials.Interface;
using PokemonUnity;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
	/// <summary>
	/// Gestionnaire audio Unity — compatible Unity 6.
	/// </summary>
	/// <remarks>
	/// Corrections Unity 6 :
	/// - new AudioSource() est interdit ; utiliser gameObject.AddComponent&lt;AudioSource&gt;()
	/// - LateUpdate : ne pas supprimer des éléments d'une liste pendant une itération foreach
	/// - Méthodes d'interface explicites correctement implémentées
	/// </remarks>
	public class AudioManager : MonoBehaviour, IAudio,
		PokemonEssentials.Interface.IAudioBGM,
		PokemonEssentials.Interface.IAudioBGS,
		PokemonEssentials.Interface.IAudioSE,
		PokemonEssentials.Interface.IAudioME
	{
		public static IAudio AudioHandler;
		public int bgm_position
		{
			get
			{
				int ret = 0;
				ret = (int)(bgmSource.time * 1000);
				return ret;
			}
			set { bgmSource.time = value / 1000f; }
		}
		public int volume { get; set; }
		public float pitch { get; set; }
		private int loopStartSamples;
		private int loopEndSamples;
		private bool isLooping = false;
		public AudioSource bgmSource;
		public AudioSource bgsSource;
		public AudioSource meSource;
		public List<AudioSource> seSources = new List<AudioSource>();

		private void Awake()
		{
			if (AudioHandler == null)
			{
				AudioHandler = this;
			}
			else if ((object)AudioHandler != this)
			{
				Destroy(gameObject);
				return;
			}
			// Unity 6 : les AudioSource doivent être créés via AddComponent
			bgmSource = gameObject.AddComponent<AudioSource>();
			bgsSource = gameObject.AddComponent<AudioSource>();
			meSource  = gameObject.AddComponent<AudioSource>();
		}

		private void LateUpdate()
		{
			// Unity 6 : ne jamais supprimer d'éléments dans un foreach — utiliser RemoveAll
			seSources.RemoveAll(source => source == null || !source.isPlaying);
		}

		void IAudio.bgm_play(string filename, float volume, float pitch) { bgm_play(filename, volume, pitch); }
		public void bgm_play(string filename, float volume, float pitch, int position = 0)
		{
			PlayAudio(bgmSource, filename, volume, pitch, true, position);
		}
		public void bgm_play(IAudioObject audio, int position = 0)
		{
			PlayAudio(bgmSource, audio, true, position);
		}

		public void bgm_stop()
		{
			bgmSource.Stop();
		}

		public void bgm_fade(float time)
		{
			StartCoroutine(FadeOut(bgmSource, time));
		}

		public void bgs_play(string filename, float volume, float pitch)
		{
			PlayAudio(bgsSource, filename, volume, pitch, true);
		}
		public void bgs_play(IAudioObject audio)
		{
			PlayAudio(bgsSource, audio, true);
		}

		public void bgs_stop()
		{
			bgsSource.Stop();
		}

		public void bgs_fade(float time)
		{
			StartCoroutine(FadeOut(bgsSource, time));
		}

		public void me_play(string filename, float volume, float pitch)
		{
			PlayAudio(meSource, filename, volume, pitch, false);
		}
		public void me_play(IAudioObject audio)
		{
			PlayAudio(meSource, audio, false);
		}

		public void me_stop()
		{
			meSource.Stop();
		}

		public void me_fade(float time)
		{
			StartCoroutine(FadeOut(meSource, time));
		}

		public void se_play(string filename, float volume, float pitch)
		{
			// Unity 6 : AudioSource créé via AddComponent, pas via new AudioSource()
			AudioSource seSource = gameObject.AddComponent<AudioSource>();
			seSources.Add(seSource);
			PlayAudio(seSource, filename, volume, pitch, false);
		}
		public void se_play(IAudioObject audio)
		{
			// Unity 6 : AudioSource créé via AddComponent, pas via new AudioSource()
			AudioSource seSource = gameObject.AddComponent<AudioSource>();
			seSources.Add(seSource);
			PlayAudio(seSource, audio.name, audio.volume, audio.pitch, false);
		}

		public void se_stop()
		{
			foreach (var source in seSources)
			{
				if (source != null)
					source.Stop();
			}
			// Supprimer les composants AudioSource créés dynamiquement pour les SE
			foreach (var source in seSources)
			{
				if (source != null)
					Destroy(source);
			}
			seSources.Clear();
		}

		private void PlayAudio(AudioSource source, string filename, float volume, float pitch, bool loop, int position = 0)
		{
			AudioClip clip = Resources.Load<AudioClip>(filename);
			if (clip == null)
			{
				Core.Logger?.LogError("Audio file not found: " + filename);
				return;
			}

			IAudioObject audio = new AudioTrack(clip).initialize(filename, volume, pitch);
			PlayAudio(source, audio, loop, position);
		}

		private void PlayAudio(AudioSource source, IAudioObject audio, bool loop, int position = 0)
		{
			AudioClip clip = (audio as AudioTrack).clip;

			source.clip = clip;
			source.volume = audio.volume;
			source.pitch = audio.pitch;
			source.loop = loop;
			source.timeSamples = position;
			source.Play();
			StopCoroutine("CheckMusicLoop");
			StartCoroutine(CheckMusicLoop());
		}

		private IEnumerator FadeOut(AudioSource source, float fadeTime)
		{
			float startVolume = source.volume;

			while (source.volume > 0)
			{
				source.volume -= startVolume * Time.deltaTime / fadeTime;
				yield return null;
			}

			source.Stop();
			source.volume = startVolume;
		}

		/// <summary>
		/// Vérifie la boucle de la musique de fond.
		/// </summary>
		/// <remarks>
		/// Toutes les pistes musicales sont bouclées par défaut.
		/// Utilisez le volume ou <see cref="bgm_stop"/> pour arrêter la musique.
		/// </remarks>
		private IEnumerator CheckMusicLoop()
		{
			while (bgmSource.isPlaying)
			{
				if(loopEndSamples == 0)
				{
					loopEndSamples = bgmSource.clip.samples;
				}
				if(loopStartSamples >= loopEndSamples && loopEndSamples != 0)
				{
					loopStartSamples = 0;
				}
				if(loopEndSamples > 0)
				{
					if (!bgmSource.loop && bgmSource.timeSamples >= loopEndSamples)
					{
						bgmSource.timeSamples = loopStartSamples;
						bgmSource.loop = true;
					}
					else if (bgmSource.loop && bgmSource.timeSamples < loopStartSamples)
					{
						bgmSource.timeSamples = loopStartSamples;
					}
				}
				yield return null;
			}
		}

		#region Explicit Interface
		IAudioBGM IAudioBGM.last()
		{
			if (bgmSource.isPlaying)
			{
				IAudioObject audio = new AudioTrack(bgmSource.clip).initialize(bgmSource.clip.name, bgmSource.volume, bgmSource.pitch);
				return audio as IAudioBGM;
			}
			return null;
		}

		void IAudioBGM.stop()
		{
			bgm_stop();
		}

		void IAudioBGM.fade(int time)
		{
			bgm_fade(time);
		}

		void IAudioBGM.play()
		{
			if (bgmSource.clip != null)
				bgmSource.Play();
		}

		IAudioBGS IAudioBGS.last()
		{
			if (bgsSource.isPlaying)
			{
				IAudioObject audio = new AudioTrack(bgsSource.clip).initialize(bgsSource.clip.name, bgsSource.volume, bgsSource.pitch);
				return audio as IAudioBGS;
			}
			return null;
		}

		void IAudioBGS.stop()
		{
			bgs_stop();
		}

		void IAudioBGS.fade(int time)
		{
			bgs_fade(time);
		}

		void IAudioBGS.play()
		{
			if (bgsSource.clip != null)
				bgsSource.Play();
		}

		void IAudioSE.stop()
		{
			se_stop();
		}

		void IAudioSE.play()
		{
		}

		void IAudioME.stop()
		{
			me_stop();
		}

		void IAudioME.fade(int time)
		{
			me_fade(time);
		}

		void IAudioME.play()
		{
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
		#endregion
	}
}
