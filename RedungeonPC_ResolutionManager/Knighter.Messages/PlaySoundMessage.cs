namespace Knighter.Messages;

public class PlaySoundMessage : Message
{
	public readonly string Name;

	public readonly float Volume;

	public readonly float Pitch;

	public readonly float Pan;

	public PlaySoundMessage(string name, float volume = 1f, float pitch = 0f, float pan = 0f)
		: base(MessageType.PlaySound)
	{
		Name = name;
		Volume = volume;
		Pitch = pitch;
		Pan = pan;
	}

	public PlaySoundMessage(SoundName name, float volume = 1f, float pitch = 0f, float pan = 0f)
		: base(MessageType.PlaySound)
	{
		Name = name.ToString();
		Volume = volume;
		Pitch = pitch;
		Pan = pan;
	}
}
