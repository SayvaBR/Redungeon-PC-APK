using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class TeleportEntity : Entity
{
	public int Index;

	private int delay;

	private int pause;

	private string program;

	private List<int> sequence;

	private int cycle;

	private List<TeleportEntity> teleports;

	private List<int> teleportColors;

	private int lastUseAgo = 100;

	private Animation anim;

	private Light light;

	private int flashIn = -1;

	private ParticleEmitter emitter;

	protected Entity teleportee;

	public bool IsSender { get; private set; }

	public TeleportEntity(int x, int y, TileDesc desc, List<TeleportEntity> teleports, List<int> teleportColors)
		: base(x, y, 1f, 1f)
	{
		Init(desc.Str("program"), desc["delay"], desc["out"] == 1, desc["pause"]);
		this.teleports = teleports;
		this.teleportColors = teleportColors;
	}

	private void Init(string program, int delay, bool isSender, int pause)
	{
		IsSender = isSender;
		this.program = program;
		this.delay = delay;
		this.pause = pause;
		sequence = new List<int>();
		ReadProgram();
		anim = new Animation();
		anim.Add("off", "teleport_off", "");
		anim.Add("r", "teleport_red_", "123432");
		anim.Add("b", "teleport_blue_", "123432");
		anim.Add("y", "teleport_yellow_", "123432");
		anim.Add("g", "teleport_green_", "123432");
		anim.Add("offx", "teleport_x_off", "");
		anim.Add("rx", "teleport_x_red_", "123432");
		anim.Add("bx", "teleport_x_blue_", "123432");
		anim.Add("yx", "teleport_x_yellow_", "123432");
		anim.Add("gx", "teleport_x_green_", "123432");
		anim.Play("off");
		light = base.core.CurrentPlayState.LightManager.AddLight(Color.White, (!IsSender) ? 0.7f : 1.5f, 0.7f, this);
		light.ChangeRate = 0.05f;
		light.FollowRate = 1f;
		light.TargetIntencity = 0f;
		light.Intencity = 0f;
		emitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.Center, 8f).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Aux.X = p.Parent.Count;
			if (isSender)
			{
				p.Velocity = p.Position.Clone();
				p.Position += p.Offset * 3f;
				p.Position -= new Vector2(0f, 100f - p.Offset.LengthSquared());
			}
			else
			{
				p.Velocity = p.Position.Clone();
				p.Velocity -= new Vector2(0f, 15 + Component._rnd(-5, 5));
			}
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += (p.Velocity - p.Position) * 0.15f;
				p.Dead = p.Age > 15;
			})
			.OnDraw(delegate(Particle p)
			{
				float num = (float)p.Age / 15f;
				float num2 = ((p.Age > 10) ? ((float)(p.Age - 10) / 5f) : 0f);
				base.core.Renderer[base.Z + 5, true].DrawSpriteW(_(SpriteName.spark), p.Position, Color.White * (1f - num2), rotation: num + p.Aux.X * 20f, scale: new Vector2(num * 1f));
			});
	}

	private void ReadProgram()
	{
		sequence.Clear();
		string[] array = program.Split(',');
		foreach (string s in array)
		{
			sequence.Add(int.Parse(s));
		}
		cycle = pause * sequence.Count;
	}

	public override void Update()
	{
		lastUseAgo++;
		if (flashIn > 0)
		{
			flashIn--;
			if (flashIn == 0)
			{
				Flash();
				flashIn = -1;
			}
		}
		int index = Index;
		int num = (base.worldTicks - delay) % cycle;
		if (num < 0)
		{
			num += cycle;
		}
		Index = sequence[num / pause];
		Index = teleportColors[Index];
		if (Index != index)
		{
			EnterTile(base.Tile);
			string text = "";
			switch (Index)
			{
			case 0:
				text = "off";
				break;
			case 1:
				text = "g";
				light.Color = default(Color).FromRgb(9952056);
				break;
			case 2:
				text = "b";
				light.Color = default(Color).FromRgb(5805015);
				break;
			case 3:
				text = "y";
				light.Color = default(Color).FromRgb(15707392);
				break;
			case 4:
				text = "r";
				light.Color = default(Color).FromRgb(15474967);
				break;
			}
			if (!IsSender)
			{
				text += "x";
			}
			anim.Play(text);
			light.TargetIntencity = ((Index == 0 || !IsSender) ? 0f : 1.6f);
		}
		anim.Update();
		base.Update();
	}

	public void Flash()
	{
		if (teleportee == null)
		{
			return;
		}
		lastUseAgo = 0;
		light.Intencity = 2f;
		light.Radius = 2f;
		if (IsSender)
		{
			SendMessage(new SpawnEntityMessage(new EffectEntity(base.Coordinates.Shift(0.43f, 0.34f), "teleport_out_", "12345678").Speed(0.4f).SetLayer("default", base.Z + 10), CurrentPlatform));
			return;
		}
		SendMessage(new SpawnEntityMessage(new EffectEntity(base.Coordinates.Shift(0.47f, 0.28f), "teleport_in_", "12345678").Speed(0.4f).SetLayer("default", base.Z + 10), CurrentPlatform));
		if (teleportee is PlayerEntity)
		{
			SendMessage(new PlaySoundMessage(SoundName.teleport));
		}
		else
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.teleport, base.WorldCenter, 0.8f));
		}
	}

	public void FlashIn(int timeout)
	{
		flashIn = (int)Component._M(timeout, 1f);
	}

	public override void Draw()
	{
		base.core.Renderer["bg", Index == 0].DrawSpriteW(anim.GetCurrentFrame(), base.WorldPosition.Shift(0f, 0.1f) + (IsSender ? Vector2.Zero : (-Vector2.One)), Color.White);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (!other.CanTeleport() || other.TeleportPending)
		{
			return;
		}
		if (IsSender && Index != 0)
		{
			List<TeleportEntity> list = teleports.FindAll((TeleportEntity t) => t.Index == Index && !t.IsSender);
			TeleportEntity teleportEntity = ((list.Count == 0) ? null : list[Component._rnd(0, list.Count - 1)]);
			if (teleportEntity != null)
			{
				other.TeleportTo(teleportEntity.CurrentMap, teleportEntity.Coordinates + ((other is PlayerEntity) ? Vector2.Zero : new Vector2(other.WorldCoordinates.X - base.WorldCoordinates.X, other.WorldCoordinates.Y - base.WorldCoordinates.Y)), this, teleportEntity, other.TeleportDelay());
				FlashIn(other.TeleportDelay() - 19);
				teleportEntity.FlashIn(other.TeleportDelay() - 19);
				emitter.Emit(4, 1, once: false, 3);
				teleportEntity.emitter.Emit(7, 1, once: false, 3);
				teleportee = other;
				teleportEntity.teleportee = other;
			}
		}
		base.CollideWith(other);
	}
}
