using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace MafiaUnity
{
	public class MusicManager : MonoBehaviour
	{
		AudioSource player = null;
		int selectedIndex = 0;

		public List<AudioEntry> audioEntries = new List<AudioEntry>();

		public bool repeatTrack = false;

		public void AddEntry(string name, AudioEntry.LoadTrackToAudioClip cb)
		{
			audioEntries.Add(new AudioEntry{name = name, loadCallback = cb, clip = null});
		}

		public void PlayAt(int index)
		{
			Debug.Assert(index >= 0 && index < audioEntries.Count, "MusicManager:PlayAt() invalid index.");

			selectedIndex = index;
			Play();
		}

		public void Play()
		{
			if (audioEntries.Count == 0) return;
            
			var entry = audioEntries[selectedIndex];
            var clip = entry?.clip;

            if (clip == null && entry != null)
            {
                entry.loadCallback(entry);
                clip = entry.clip;
            }

            player.clip = clip;
            player.Play();
		}

		public void Play(string name)
		{
			var entry = audioEntries.First(x => x.name == name);
			var clip = entry?.clip;

			if (clip == null && entry != null)
			{
				entry.loadCallback(entry);
				clip = entry.clip;
			}

			Debug.Assert(clip != null, "MusicManager:Play() specified track not found.");

			player.clip = clip;
			player.Play();
		}

		private void Start()
		{
            player = gameObject.AddComponent<AudioSource>();
		}

		private void FixedUpdate()
		{
			if (player == null) return;

            float volume = float.Parse(GameAPI.instance.cvarManager.Get("musicVolume", "0.35"), CultureInfo.InvariantCulture);
            player.volume = volume;

			if (player.isPlaying) return;

            if (audioEntries.Count == 0) return;
			
			if (!repeatTrack)
			{
				selectedIndex++;

				if (selectedIndex == audioEntries.Count)
					selectedIndex = 0;
			}

			Play();
		}

		[Serializable]
		public class AudioEntry
		{
			public delegate void LoadTrackToAudioClip(AudioEntry self);
			public LoadTrackToAudioClip loadCallback;
			public AudioClip clip;
			public string name;
		}
	}
}