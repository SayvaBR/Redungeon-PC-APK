using Microsoft.Xna.Framework;

namespace Knighter.Messages;

public class PlayWorldSoundMessage : Message
{
	public readonly string Name;

	public Vector2 WorldPosition;

	public readonly float Volume;

	public readonly float Pitch;

	public PlayWorldSoundMessage(string name, Vector2 worldPosition, float volume = 1f, float pitch = 0f)
		: base(MessageType.PlayWorldSound)
	{
		Name = name;
		WorldPosition = worldPosition;
		Volume = volume;
		Pitch = pitch;
	}

	public PlayWorldSoundMessage(SoundName name, Vector2 worldPosition, float volume = 1f, float pitch = 0f)
		: base(MessageType.PlayWorldSound)
	{
		Name = name.ToString();
		WorldPosition = worldPosition;
		Volume = volume;
		Pitch = pitch;
	}
}
