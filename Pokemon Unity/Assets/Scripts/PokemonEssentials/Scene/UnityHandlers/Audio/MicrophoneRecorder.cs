using PokemonEssentials.Interface;
using UnityEngine;

namespace PokemonUnity.Interface.UnityEngine
{
	/// <summary>
	/// Enregistrement microphone via Unity — compatible Unity 6.
	/// </summary>
	/// <remarks>
	/// Correction Unity 6 :
	/// - [RequireComponent(typeof(AudioClip))] supprimé : AudioClip n'est PAS un Component Unity.
	///   AudioClip est un Asset (ScriptableObject/Object), non un MonoBehaviour/Component.
	///   On utilise [RequireComponent(typeof(AudioSource))] à la place.
	/// </remarks>
	[RequireComponent(typeof(AudioSource))]
	public class MicrophoneRecorder : global::UnityEngine.MonoBehaviour, IWaveData
	{
		public AudioClip recordedClip;
		/// <summary>
		/// Niveau sonore moyen (intensité) du flux audio enregistré.
		/// </summary>
		public byte intensity { get; private set; }
		/// <summary>
		/// Durée de l'enregistrement en millisecondes.
		/// </summary>
		public int time { get { return recordedClip ? (int)(recordedClip.length * 1000) : 0; } }

		/// <summary>
		/// Lecture de l'audio enregistré.
		/// </summary>
		public void play()
		{
			if (recordedClip)
			{
				AudioSource audioSource = GetComponent<AudioSource>();
				if (!audioSource)
				{
					audioSource = gameObject.AddComponent<AudioSource>();
				}
				audioSource.clip = recordedClip;
				audioSource.Play();
			}
			else
			{
				Core.Logger?.LogError("No recorded audio clip available.");
			}
		}

		#region Microphone Recording Logic
		/// <summary>
		/// Lance l'enregistrement depuis le microphone par défaut.
		/// </summary>
		/// <param name="recordLengthSeconds">Durée maximale en secondes.</param>
		public void StartRecording(float recordLengthSeconds = 10)
		{
			if (Microphone.devices.Length > 0)
			{
				recordedClip = Microphone.Start(null, false, Mathf.CeilToInt(recordLengthSeconds), 44100);
			}
			else
			{
				Core.Logger?.LogError("Failed to start recording: Microphone not available");
			}
		}

		/// <summary>
		/// Arrête l'enregistrement et conserve le clip AudioClip.
		/// </summary>
		public void StopRecording()
		{
			if (Microphone.IsRecording(null))
			{
				Microphone.End(null);
			}
		}
		#endregion

		/// <summary>
		/// Calcul de l'intensité sonore (version simplifiée).
		/// </summary>
		private void CalculateIntensity()
		{
			if (recordedClip)
			{
				float[] samples = new float[recordedClip.samples * recordedClip.channels];
				recordedClip.GetData(samples, 0);
				float sum = 0;
				for (int i = 0; i < samples.Length; i++)
				{
					sum += samples[i] * samples[i];
				}
				intensity = (byte)(Mathf.Sqrt(sum / samples.Length) * 256);
			}
		}

		/// <summary>
		/// Unity ne supporte pas nativement la sauvegarde d'AudioClip en fichier WAV.
		/// Une implémentation custom est requise.
		/// </summary>
		/// <param name="clip">Le clip à sauvegarder.</param>
		/// <param name="path">Chemin de destination.</param>
		public static void SaveToWav(AudioClip clip, string path)
		{
			// Implémenter le format WAV manuellement ou utiliser une bibliothèque tierce
		}
	}
}
