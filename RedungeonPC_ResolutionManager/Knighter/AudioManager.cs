using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Knighter;

public sealed class AudioManager : Component
{
	private readonly Dictionary<string, SoundEffect> sounds;

	private readonly Dictionary<string, Song> music;

	public HandleBox MusicVolumeBox;

	private string currentMusic;

	private MusicPlayer musicPlayer;

	public bool IsPlayingMusic { get; private set; }

	public AudioManager()
	{
		sounds = new Dictionary<string, SoundEffect>();
		music = new Dictionary<string, Song>();
		MusicVolumeBox = new HandleBox(0.3f);
		musicPlayer = new MusicPlayer();
	}

	public override void Update()
	{
		MusicVolumeBox.Update();
		musicPlayer.SetVolume(MusicVolumeBox.Value * core.OptionsData.MusicVolume);
		SoundEffect.MasterVolume = core.OptionsData.PlaySounds ? core.OptionsData.EffectsVolume : 0f;
		base.Update();
	}

	public override void Load()
	{
		Subscribe(MessageType.PlaySound);
		Subscribe(MessageType.PlayWorldSound);
		LoadSoundsFromFile("Content/Sounds/sounds.json");
		LoadMusicFromFile("Content/Music/music.json");
		musicPlayer.StartThread();
		base.Load();
	}

	public override void Unload()
	{
		musicPlayer.StopThread();
		Unsubscribe(MessageType.PlaySound);
		Unsubscribe(MessageType.PlayWorldSound);
		base.Unload();
	}

	private void PlaySound(string name, float volume = 1f, float pitch = 0f, float pan = 0f)
	{
		if (name == "none" || !base.core.OptionsData.PlaySounds || volume.IsZero())
		{
			return;
		}
		try
		{
			sounds[name].Play(volume, pitch, pan);
		}
		catch (Exception)
		{
		}
	}

	public float VolumeInWorld(Vector2 worldPosition)
	{
		float num = (base.core.CurrentPlayState.Player.WorldCenter - worldPosition).Length();
		float num2 = 0f;
		if (num > 120f)
		{
			return 0f;
		}
		if (num > 0f)
		{
			return MathHelper.Lerp(1f, 0f, (num - 0f) / 120f);
		}
		return 1f;
	}

	public float PanInWorld(Vector2 worldPosition)
	{
		Vector2 worldCenter = base.core.CurrentPlayState.Player.WorldCenter;
		return Component._M(Component._m((worldPosition.X - worldCenter.X) / 96f, 1f), -1f);
	}

	private void PlayWorldSound(string name, Vector2 worldPosition, float volumeFactor = 1f, float pitch = 0f)
	{
		if (base.core.OptionsData.PlaySounds && !(name == "none") && !volumeFactor.IsZero() && base.core.CurrentPlayState.IsTopState)
		{
			float volume = VolumeInWorld(worldPosition) * volumeFactor;
			float pan = PanInWorld(worldPosition);
			PlaySound(name, volume, pitch + (base.core.CurrentPlayState.SloMo ? (-0.5f) : 0f), pan);
		}
	}

	public void Apply2D(SoundEffectInstance sound, Vector2 worldPosition, float volumeFactor = 1f)
	{
		float volume = (base.core.CurrentPlayState.IsTopState ? (VolumeInWorld(worldPosition) * volumeFactor) : 0f);
		float pan = PanInWorld(worldPosition);
		float pitch = (base.core.CurrentPlayState.SloMo ? (-0.5f) : 0f);
		sound.Volume = volume;
		sound.Pan = pan;
		sound.Pitch = pitch;
	}

	public SoundEffectInstance CreateSoundInstance(string name)
	{
		return sounds[name].CreateInstance();
	}

	public SoundEffectInstance CreateSoundInstance(SoundName name)
	{
		return CreateSoundInstance(name.ToString());
	}

	public void PlayMusic(string name)
	{
		if (!(currentMusic == name))
		{
			musicPlayer.ChangeMusic(music[name]);
			IsPlayingMusic = true;
			currentMusic = name;
		}
	}

	public void StopMusic()
	{
		MediaPlayer.Stop();
		IsPlayingMusic = false;
		currentMusic = "";
	}

	public void PlayRandomMusic()
	{
		List<string> list = music.Keys.ToList();
		if (list.Count != 0)
		{
			string name = list[SciHelper.GetRandom(0, list.Count - 1)];
			PlayMusic(name);
		}
	}

	private void LoadSoundsFromFile(string filePath)
	{
		foreach (JsonObject item in JsonReader.FromFile(filePath)["sounds"].ToListOfObjects())
		{
			string key = item["name"].ToString().Replace("\"", "");
			string arg = item["path"].ToString().Replace("\"", "");
			SoundEffect value = base.core.Content.Load<SoundEffect>($"Sounds/{arg}");
			sounds.Add(key, value);
		}
	}

	private void LoadMusicFromFile(string filePath)
	{
		foreach (JsonObject item in JsonReader.FromFile(filePath)["songs"].ToListOfObjects())
		{
			string key = item["name"].ToString().Replace("\"", "");
			string text = item["file-name"].ToString().Replace("\"", "");
			Song value = base.core.Content.Load<Song>(string.Format("Music/{0}", text));
			music.Add(key, value);
		}
	}

	public override void OnMessage(Message message, object sender)
	{
		switch (message.Type)
		{
		case MessageType.PlaySound:
		{
			PlaySoundMessage playSoundMessage = message as PlaySoundMessage;
			PlaySound(playSoundMessage.Name, playSoundMessage.Volume, playSoundMessage.Pitch, playSoundMessage.Pan);
			break;
		}
		case MessageType.PlayWorldSound:
		{
			PlayWorldSoundMessage playWorldSoundMessage = message as PlayWorldSoundMessage;
			PlayWorldSound(playWorldSoundMessage.Name, playWorldSoundMessage.WorldPosition, playWorldSoundMessage.Volume, playWorldSoundMessage.Pitch);
			break;
		}
		}
		base.OnMessage(message, sender);
	}
}
