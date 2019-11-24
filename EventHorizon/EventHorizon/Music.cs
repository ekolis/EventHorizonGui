using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EventHorizon
{
	/// <summary>
	/// Music support for the game.
	/// </summary>
	public static class Music
	{
		static Music()
		{
			waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
			mixer = new MixingSampleProvider(waveFormat);

			waveout.Init(mixer);
			waveout.PlaybackStopped += waveout_PlaybackStopped;
			waveout.Play();
		}

		public static Mood CurrentMood
		{
			get
			{
				return currentMood;
			}
			set
			{
				bool newtrack = false;
				if (currentMood != value)
					newtrack = true;
				currentMood = value;
				if (newtrack)
					StartNewTrack();
			}
		}

		public static bool IsPlaying { get; private set; }
		private const int FadeDuration = 5000;
		private static Mood currentMood;
		private static FadeInOutSampleProvider curTrack, prevTrack;
		private static MixingSampleProvider mixer;
		private static float musicVolume = 1.0f;
		private static WaveFormat waveFormat;
		private static WaveOutEvent waveout = new WaveOutEvent();

		public static void Play(Mood mood)
		{
			if (mood == CurrentMood)
				return;

			currentMood = mood;
			StartNewTrack();
		}

		public static void setVolume(float volume)
		{
			musicVolume = volume;
		}

		public static void StartNewTrack()
		{
			// find out what to play
			var tracks = FindTracks().ToArray();
			var track = tracks.Where(t => t.Mood == CurrentMood).PickRandom();
			if (track == null)
			{
				// no music? try another mood
				var others = tracks;
				if (others.Any())
					track = others.PickRandom();
			}
			if (track == null)
			{
				// no music at all :(
				return;
			}

			// prepare the new track
			var tl = track.Path.ToLower();
			WaveChannel32 wc = null;
			if (tl.EndsWith("ogg"))
				wc = new WaveChannel32(new VorbisWaveReader(track.Path));
			else if (tl.EndsWith("mp3"))
				wc = new WaveChannel32(new Mp3FileReader(track.Path));
			else if (tl.EndsWith("wav"))
				wc = new WaveChannel32(new WaveFileReader(track.Path));
			else
				throw new Exception("Unknown audio format for file " + track.Path);

			// convert to a standard format so we can mix them (e.g. a mp3 with an ogg)
			var resampler = new MediaFoundationResampler(wc, waveFormat);
			var sp = resampler.ToSampleProvider();

			// setup our track
			wc.Volume = musicVolume;
			wc.PadWithZeroes = false; // to allow PlaybackStopped event to fire

			// fade between the two tracks
			mixer.RemoveMixerInput(prevTrack);
			prevTrack = curTrack;
			if (prevTrack != null)
				prevTrack.BeginFadeOut(FadeDuration);
			curTrack = new FadeInOutSampleProvider(sp, true);
			curTrack.BeginFadeIn(FadeDuration);
			mixer.AddMixerInput(curTrack);
			waveout.Play();
			IsPlaying = true;
		}

		private static IEnumerable<Track> FindTracks()
		{
			foreach (Mood mood in Enum.GetValues(typeof(Mood)))
			{
				var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Music", mood.ToString());
				IEnumerable<string> files = Enumerable.Empty<string>();
				try
				{
					files = Directory.GetFiles(folder, "*.ogg").Union(
						Directory.GetFiles(folder, "*.mp3")).Union(
						Directory.GetFiles(folder, "*.wav")).Union(
						Directory.GetFiles(folder, "*.aiff")).Union(
						Directory.GetFiles(folder, "*.aif")).Union(
						Directory.GetFiles(folder, "*.aifc"));
				}
				catch
				{
					Console.Error.WriteLine("Cannot find music folder " + folder + ".");
				}
				foreach (var file in files)
					yield return new Track(mood, file);
			}
		}

		private static void waveout_PlaybackStopped(object sender, StoppedEventArgs e)
		{
			// play another song
			StartNewTrack();
		}

		private class Track
		{
			public Track(Mood mood, string path)
			{
				Mood = mood;
				Path = path;
			}


			public Mood Mood { get; set; }
			public string Path { get; set; }
		}
	}
}
