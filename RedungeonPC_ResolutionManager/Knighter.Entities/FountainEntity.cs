using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FountainEntity : Entity
{
	private Animation anim;

	private const int pushAnimMax = 30;

	private int pushAnim;

	private bool used;

	public FountainEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		anim = new Animation(0.14f);
		anim.Add("live", "fountain_", "1232");
		anim.Add("stop", "fountain_", "4567");
		anim.Play("live");
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 5f).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Position -= base.WorldCenter;
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position.Y -= 0.15f;
				p.Dead = p.Age > 30;
			})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z + 1].DrawSpriteW(_(SpriteName.spark), base.WorldCenter + p.Position.Shift(0f, -7f), null, new Vector2((float)(30 - p.Age) / 30f), (float)p.Age * 0.05f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Start(40);
	}

	public override void Update()
	{
		if (pushAnim > 0)
		{
			pushAnim--;
		}
		anim.Update();
		base.Update();
	}

	public override void Draw()
	{
		Vector2 value = new Vector2(1f + 0.3f * (float)pushAnim / 30f);
		base.core.Renderer[base.Z].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(0f, -5f), null, value, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(-8.5f, -4f), Color.Black * 0.2f, null, 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return other is FragmentEntity;
	}

	public override void InteractWith(Entity other)
	{
		if (other is PlayerEntity { Dead: false } playerEntity)
		{
			pushAnim = 30;
			if (!used)
			{
				playerEntity.ResetAbilities(refill: true);
				SendMessage(new PlaySoundMessage(SoundName.recharge));
				used = true;
				anim.Play("stop");
				anim.Loop = false;
				base.core.CurrentPlayState.Hud.ShowAlert("recharged", __(SId.OBJECT_FOUNTAIN_recharged), Color.DodgerBlue);
			}
		}
	}
}
