using System.Threading;
using Knighter.Helpers;
using Microsoft.Xna.Framework.Media;

namespace Knighter;

public class MusicPlayer
{
	private Thread thread;

	private Song musicToPlay;

	private float volumeToSet;

	private AutoResetEvent wait;

	private bool running;

	public MusicPlayer()
	{
		running = false;
	}

	public void StartThread()
	{
		wait = new AutoResetEvent(initialState: true);
		running = true;
		thread = new Thread(MusicThread);
		thread.Name = "Music Thread";
		thread.Start();
	}

	public void StopThread()
	{
		running = false;
		wait.Set();
	}

	public void ChangeMusic(Song music)
	{
		musicToPlay = music;
		wait.Set();
	}

	public void SetVolume(float volume)
	{
		volumeToSet = volume;
		wait.Set();
	}

	private void MusicThread()
	{
		while (running)
		{
			if (musicToPlay != null)
			{
				MediaPlayer.IsRepeating = true;
				MediaPlayer.Play(musicToPlay);
				musicToPlay = null;
			}
			if (volumeToSet >= 0f)
			{
				MediaPlayer.IsMuted = volumeToSet.IsZero();
				MediaPlayer.Volume = volumeToSet;
				volumeToSet = -1f;
			}
			wait.WaitOne();
		}
	}
}
