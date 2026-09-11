using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BraggsParrotEntity : Entity
{
	private Entity host;

	private PlayerEntity player;

	private Animation anim;

	private Animation featherAnim;

	private Light light;

	private Vector2 dPos;

	private int playerDeathTick = -1;

	private bool flip;

	private int ticksActive;

	private int ticksEscaping;

	private float dz = 15f;

	private bool first;

	public static BagOf<SoundName> squawks;

	static BraggsParrotEntity()
	{
		squawks = new BagOf<SoundName>().Put(SoundName.bragg_parrot_voice_1).Put(SoundName.bragg_parrot_voice_2).Put(SoundName.bragg_parrot_voice_3)
			.Put(SoundName.bragg_parrot_voice_4)
			.Put(SoundName.bragg_parrot_voice_5);
	}

	public BraggsParrotEntity(int x, int y, PlayerEntity player, bool first)
		: base(x, y, 0f, 0f)
	{
		SetFlying(value: true);
		this.player = player;
		host = player;
		this.first = first;
		anim = new Animation();
		anim.Add("front", "parrot_front_", "1234");
		anim.Add("back", "parrot_back_", "1234");
		anim.Play("front");
		featherAnim = new Animation();
		featherAnim.Add("fall", "parrot_feather_", "11123332");
		featherAnim.Play("fall");
		dPos = Vector2.Zero;
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(5746728), 2f, 0.6f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 0.2f;
		base.core.ParticleManager.AddEmitter(inWorld: true, base.Position).OnSpawn(delegate(Particle p)
		{
			p.Position += dPos.Shift(0f, (0f - dPos.Y) * 2f - 15f);
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age > 80;
			p.Position.Y += 0.2f;
		})
			.OnDraw(delegate(Particle p)
			{
				float num = ((p.Age < 50) ? 1f : (1f - (float)(p.Age - 50) / 30f));
				float value = ((p.Age > 20) ? 1f : ((float)p.Age / 20f));
				base.core.Renderer[base.Z + 10].DrawSpriteW(featherAnim.GetCurrentFrame(), p.Position, Color.White * num, new Vector2(value));
			})
			.AttachTo(this)
			.Start(60);
		SendMessage(new PlayWorldSoundMessage(squawks.DrawDifferent(), base.WorldCenter));
		base.Load();
	}

	public override void Update()
	{
		dz *= 0.8f;
		anim.Update();
		featherAnim.Update();
		light.Position += dPos;
		if (host == player)
		{
			if (base.core.CurrentPlayState.Started)
			{
				ticksActive++;
			}
			if (ticksActive >= (first ? 240 : 600) || player.Dead)
			{
				host = null;
				SendMessage(new PlayWorldSoundMessage(squawks.DrawDifferent(), base.WorldCenter));
				(player as BraggChar)?.ParrotEscaped(first);
				anim.Speed = 0.4f;
			}
		}
		float num2;
		float num3;
		if (host != null)
		{
			float num = ((host == player) ? 0.04f : 0.02f);
			num2 = (host.WorldCenterCoordinates.X - x + 0.5f) * num * ((host == player) ? 0.4f : 1f);
			num3 = (host.WorldCenterCoordinates.Y - y + 0f) * num;
		}
		else
		{
			ticksEscaping++;
			if (ticksEscaping == 600)
			{
				SendMessage(new RemoveEntityMessage(this));
			}
			num2 = 0f;
			num3 = ((ticksEscaping < 90) ? 0f : (-0.3f));
		}
		x += num2;
		y += num3;
		if (num3 < 0f)
		{
			anim.Play("back");
		}
		else
		{
			anim.Play("front");
		}
		flip = num2 > 0f;
		base.Update();
	}

	public override void Draw()
	{
		Sprite currentFrame = anim.GetCurrentFrame();
		dPos = new Vector2(Component._cos((float)base.worldTicks * 0.03f) * 10f, Component._sin((float)base.worldTicks * 0.03f) * 5f);
		base.core.Renderer[base.Z + 10].DrawSpriteW(currentFrame, base.WorldPosition.Shift(0f, -20f + dz) + dPos, null, null, 0f, flip ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(currentFrame, base.WorldPosition.Shift(0f, 10f - dz / 2f) + dPos.Shift(0f, (0f - dPos.Y) * 2f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, flip ? (SpriteFlip.Horizontal | SpriteFlip.Vertical) : SpriteFlip.Vertical, SpriteOrigin.Center);
		base.Draw();
	}
}
