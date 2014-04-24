#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.IO;

using SDL2;
#endregion

namespace Microsoft.Xna.Framework.Media
{
	public sealed class Song : IEquatable<Song>, IDisposable
	{
		#region SDL_mixer Open/Close Routines

		// Has SDL_mixer already been opened?
		private static bool initialized = false;

		private static void initializeMixer()
		{
			if (!initialized)
			{
				SDL.SDL_InitSubSystem(SDL.SDL_INIT_AUDIO);
				SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 1024);
				initialized = true;
			}
		}

		internal static void closeMixer()
		{
			if (initialized)
			{
				SDL_mixer.Mix_CloseAudio();
				initialized = false;
			}
		}

		#endregion

		#region Private Member Data

		private IntPtr INTERNAL_mixMusic;

		SDL_mixer.MusicFinishedDelegate musicFinishedDelegate;

		#endregion

		#region Internal Member Data

		internal delegate void FinishedPlayingHandler(object sender, EventArgs args);

		#endregion

		#region Public Properties

		// TODO: A real Vorbis stream would have this info.
		public TimeSpan Duration
		{
			get;
			private set;
		}

		// TODO: A real Vorbis stream would have this info.
		public TimeSpan Position
		{
			get
			{
				return new TimeSpan(0);
			}
		}

		public bool IsProtected
		{
			get
			{
				return false;
			}
		}

		public bool IsRated
		{
			get
			{
				return false;
			}
		}

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(FilePath);
			}
		}

		public int PlayCount
		{
			get;
			private set;
		}

		public int Rating
		{
			get
			{
				return 0;
			}
		}

		// TODO: Could be obtained with Vorbis metadata
		public int TrackNumber
		{
			get
			{
				return 0;
			}
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Properties

		internal string FilePath
		{
			get;
			private set;
		}

		internal float Volume
		{
			get
			{
				return SDL_mixer.Mix_VolumeMusic(-1) / 128.0f;
			}
			set
			{
				SDL_mixer.Mix_VolumeMusic((int) (value * 128));
			}
		}

		#endregion

		#region Constructors, Deconstructor, Dispose()

		internal Song(string fileName, int durationMS) : this(fileName)
		{
			Duration = TimeSpan.FromMilliseconds(durationMS);
		}

		internal Song(string fileName)
		{
			FilePath = fileName;
			initializeMixer();
			INTERNAL_mixMusic = SDL_mixer.Mix_LoadMUS(fileName);
			IsDisposed = false;
		}

		~Song()
		{
			SDL_mixer.Mix_HookMusicFinished(null);
			Dispose(true);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (INTERNAL_mixMusic != IntPtr.Zero)
				{
					SDL_mixer.Mix_FreeMusic(INTERNAL_mixMusic);
				}
			}
			IsDisposed = true;
		}

		#endregion

		#region Internal Playback Methods

		internal void Play()
		{
			if (INTERNAL_mixMusic == IntPtr.Zero)
			{
				return;
			}
			musicFinishedDelegate = OnFinishedPlaying;
			SDL_mixer.Mix_HookMusicFinished(musicFinishedDelegate);
			SDL_mixer.Mix_PlayMusic(INTERNAL_mixMusic, 0);
			PlayCount += 1;
		}

		internal void Resume()
		{
			SDL_mixer.Mix_ResumeMusic();
		}

		internal void Pause()
		{
			SDL_mixer.Mix_PauseMusic();
		}

		internal void Stop()
		{
			SDL_mixer.Mix_HookMusicFinished(null);
			SDL_mixer.Mix_HaltMusic();
			PlayCount = 0;
		}

		#endregion

		#region Internal Event Handler Methods

		internal void OnFinishedPlaying()
		{
			MediaPlayer.OnSongFinishedPlaying(null, null);
		}

		#endregion

		#region Public Comparison Methods/Operators

		public bool Equals(Song song) 
		{
			return (((object) song) != null) && (Name == song.Name);
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return Equals(obj as Song);
		}

		public static bool operator ==(Song song1, Song song2)
		{
			if (((object) song1) == null)
			{
				return ((object) song2) == null;
			}
			return song1.Equals(song2);
		}

		public static bool operator !=(Song song1, Song song2)
		{
			return !(song1 == song2);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
