using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SandboxChar : PlayerEntity
{
	[Preserve]
	public SandboxChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.095f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "knight_n", "1234");
		animation.Add("e", "knight_e", "1234");
		animation.Add("w", "knight_w", "1234");
		animation.Add("s", "knight_s", "1234");
		animation.Add("spin", "knight_fall_", "1111122222");
		AnimateUTurns = false;
		PosShift = new Vector2(-1.5f, -7f);
		ShadowShift = new Vector2(0f, 3f);
	}

	public override bool SpawnFallFragments()
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0.2f, -0.2f), SpriteName.knight_shield, -1, new Vector4(0.07f, 0f, 1.8f, 0.2f)), null), 2);
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.2f, -0.2f), SpriteName.knight_sword, -1, new Vector4(-0.07f, 0f, 1.8f, -0.4f)), null), 2);
		return true;
	}

	public override void CollideWith(Entity other)
	{
		base.CollideWith(other);
	}

	public override bool TryResist(InjuryType injuryType, Entity offender)
	{
		if (!base.TryResist(injuryType, offender))
		{
			return injuryType == InjuryType.Flame;
		}
		return true;
	}

	public override void TryTriggerAbility()
	{
		if (!base.Falling)
		{
			base.TryTriggerAbility();
		}
	}

	public override bool Paralized()
	{
		return base.Paralized();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Draw()
	{
		base.Draw();
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlayWorldSoundMessage(SoundName.spikes_break, base.WorldPosition));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_helmet), null));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_shield), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.knight_sword), null));
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.knight_shield), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.knight_sword), null));
		return true;
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.knight_shot;
	}
}
