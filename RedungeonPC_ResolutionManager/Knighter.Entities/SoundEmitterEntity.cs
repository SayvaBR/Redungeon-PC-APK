using Knighter.Messages;

namespace Knighter.Entities;

public class SoundEmitterEntity : Entity
{
	public SoundEmitterEntity(int x, int y)
		: base(x, y, 0f, 0f)
	{
	}

	public override void Update()
	{
		if (base.core.CurrentPlayState.Started)
		{
			float num = base.core.AudioManager.VolumeInWorld(base.WorldPosition);
			base.core.AudioManager.MusicVolumeBox.Set("festival-fade", 1f - num, inWorld: true);
			if (base.Age % 185 == 0)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.festival, base.WorldCenter));
			}
			base.Update();
		}
	}
}
