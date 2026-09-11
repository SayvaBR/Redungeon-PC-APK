using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SlimeEntity : Entity
{
	private Animation anim;

	private string program;

	private int delay;

	private int defaultDelay;

	private int jumpTime;

	private bool noLimit;

	private List<int> directions;

	private List<int> delays;

	private int cycle;

	private Vector2 spawn;

	private Vector2 firstSpawn;

	private PlatformEntity firstPlatform;

	private int consuming;

	private Sprite playerSprite;

	private Vector2 playerDeathPos;

	private bool fleeing;

	private int fleeDelay = 50;

	private bool animSoundPlayed;

	private BagOf<SoundName> bagOfSounds;

	public SlimeEntity(int x, int y, TileDesc desc)
		: base((float)x + 0.4f, (float)y + 0.4f, 0.1f, 0.1f)
	{
		directions = new List<int>();
		delays = new List<int>();
		spawn = new Vector2((float)x + 0.4f, (float)y + 0.4f);
		firstSpawn = spawn.Clone();
		firstPlatform = CurrentPlatform;
		Init(desc.Str("program"), desc["delay"], desc["default-delay"], desc["jump-time"], desc["no-limit"], desc.Flipped);
		anim = new Animation(0.15f);
		anim.Add("idle", "slime_", "22211");
		anim.Add("moving", "slime_", "12321");
		anim.Play("idle");
		bagOfSounds = new BagOf<SoundName>().Put(SoundName.slime_crawl_1).Put(SoundName.slime_crawl_2).Put(SoundName.slime_crawl_3);
		Sprite slimeTrail = _(SpriteName.slime_trail);
		base.core.ParticleManager.AddEmitter(inWorld: true, base.Position, 1f).OnSpawn(delegate(Particle p)
		{
			p.Platform = CurrentPlatform;
			p.Aux.Z = base.Z;
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age > 50;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer["bg", (int)p.Aux.Z + 33, true].DrawSpriteW(slimeTrail, p.Position.Shift(0f, -1f) + (p.Platform?.Position ?? Vector2.Zero), default(Color).FromRgb(1008705), new Vector2(Component._m(50 - p.Age, 20f) / 20f), 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["bg", (int)p.Aux.Z + 32, true].DrawSpriteW(slimeTrail, p.Position + (p.Platform?.Position ?? Vector2.Zero), default(Color).FromRgb(2656789), new Vector2(Component._m(50 - p.Age, 20f) / 20f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.AttachTo(this, local: true)
			.Start(2);
		base.core.ParticleManager.AddEmitter(inWorld: true, base.Position, 5f).OnSpawn(delegate(Particle p)
		{
			p.Platform = CurrentPlatform;
		}).OnUpdate(delegate(Particle p)
		{
			p.Dead = p.Age > 60;
		})
			.OnDraw(delegate(Particle p)
			{
				Vector2 vector = p.Position + (p.Platform?.Position ?? Vector2.Zero);
				base.core.Renderer["bg", 5, false].DrawDotW(vector.X + Component._sin((float)(base.ticks + p.Age) / 20f), vector.Y - (float)p.Age / 20f + Component._cos((float)(base.ticks + p.Age) / 20f), default(Color).FromRgb(2656789) * (Component._m(60 - p.Age, 10f) / 10f), 0.5f);
			})
			.AttachTo(this, local: true)
			.Start(10);
	}

	public override bool CanTeleport()
	{
		return true;
	}

	private void Init(string program, int delay, int defaultDelay, int jumpTime, int noLimit, bool flipped = false)
	{
		if (flipped)
		{
			program = program.Replace("e", "!");
			program = program.Replace("w", "e");
			program = program.Replace("!", "w");
		}
		this.program = program;
		this.delay = delay;
		this.defaultDelay = defaultDelay;
		this.jumpTime = jumpTime;
		this.noLimit = noLimit == 1;
		ReadProgram();
	}

	private void ReadProgram()
	{
		string text = program;
		directions.Clear();
		delays.Clear();
		cycle = 0;
		int num = 0;
		int num2 = 0;
		int i = 0;
		int num3 = -1;
		int num4 = 0;
		bool flag = false;
		for (; i < text.Length; i++)
		{
			char c = text[i];
			if (c == 'n' || c == 'e' || c == 's' || c == 'w')
			{
				if (num3 != -1)
				{
					int num5 = (flag ? num4 : defaultDelay);
					directions.Add(num3);
					delays.Add(num5);
					cycle += num5 + jumpTime;
					num += num3 switch
					{
						3 => -1, 
						1 => 1, 
						_ => 0, 
					};
					num2 += num3 switch
					{
						2 => 1, 
						0 => -1, 
						_ => 0, 
					};
					num3 = -1;
					num4 = 0;
					flag = false;
				}
				switch (c)
				{
				case 'n':
					num3 = 0;
					break;
				case 'e':
					num3 = 1;
					break;
				case 's':
					num3 = 2;
					break;
				case 'w':
					num3 = 3;
					break;
				}
			}
			if (c >= '0' && c <= '9')
			{
				num4 = num4 * 10 + (c - 48);
				flag = true;
			}
		}
		if (num3 != -1)
		{
			int num6 = (flag ? num4 : defaultDelay);
			directions.Add(num3);
			delays.Add(num6);
			cycle += num6 + jumpTime;
			num += num3 switch
			{
				3 => -1, 
				1 => 1, 
				_ => 0, 
			};
			num2 += num3 switch
			{
				2 => 1, 
				0 => -1, 
				_ => 0, 
			};
			num3 = -1;
			num4 = 0;
			flag = false;
		}
		if (!noLimit && (num != 0 || num2 != 0))
		{
			cycle *= 2;
			for (int num7 = directions.Count - 1; num7 >= 0; num7--)
			{
				int num8 = directions[num7];
				num8 = (num8 + 2).Mod(4);
				directions.Add(num8);
				delays.Add((num7 > 0) ? delays[num7 - 1] : delays[delays.Count - 1]);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (!fleeing)
		{
			int num = (base.worldTicks - delay).Mod(cycle);
			if (num == 0 && noLimit)
			{
				spawn = firstSpawn.Clone();
				CurrentPlatform = firstPlatform;
			}
			x = spawn.X;
			y = spawn.Y;
			int num2 = 0;
			bool flag = false;
			while (!flag)
			{
				if (num2 >= directions.Count)
				{
					flag = true;
					continue;
				}
				int num3 = directions[num2];
				int num4 = delays[num2];
				if (num4 == -1)
				{
					num4 = defaultDelay;
				}
				int num5 = num3 switch
				{
					3 => -1, 
					1 => 1, 
					_ => 0, 
				};
				int num6 = num3 switch
				{
					2 => 1, 
					0 => -1, 
					_ => 0, 
				};
				if (num >= jumpTime)
				{
					num -= jumpTime;
					x += num5;
					y += num6;
				}
				else
				{
					flag = true;
					if (anim.CurrentSequence != "moving")
					{
						anim.Play("moving");
					}
					x += MathHelper.Lerp(0f, num5, (float)num / (float)jumpTime);
					y += MathHelper.Lerp(0f, num6, (float)num / (float)jumpTime);
				}
				if (!flag)
				{
					if (num >= num4)
					{
						num -= num4;
					}
					else
					{
						anim.Play("idle");
						flag = true;
					}
				}
				num2++;
			}
			UpdateTiles();
			anim.Update();
			if (anim.CurrentSequence == "moving" && anim.GetCurrentFrameNumber() == 0 && !animSoundPlayed)
			{
				animSoundPlayed = true;
				SendMessage(new PlayWorldSoundMessage(bagOfSounds.DrawDifferent(), base.WorldPosition));
			}
			if (anim.CurrentSequence == "moving" && anim.GetCurrentFrameNumber() != 0)
			{
				animSoundPlayed = false;
			}
			if (consuming > 0)
			{
				consuming -= 2;
			}
			if (consuming == 20)
			{
				base.core.CurrentPlayState.Player.TrySpawnLeftovers(base.WorldCenterCoordinates);
			}
		}
		else
		{
			fleeDelay--;
			if (fleeDelay == 0)
			{
				DieAndSpawnFraments();
			}
		}
	}

	protected override bool OnDoTeleport()
	{
		spawn.X = DestTeleport.Coordinates.X + (spawn.X - SourceTeleport.Coordinates.X);
		spawn.Y = DestTeleport.Coordinates.Y + (spawn.Y - SourceTeleport.Coordinates.Y);
		FlushTiles();
		return false;
	}

	public override int TeleportDelay()
	{
		return jumpTime - 5;
	}

	private void DieAndSpawnFraments()
	{
		SendMessage(new RemoveEntityMessage(this));
		SendMessage(new PlayWorldSoundMessage(SoundName.slime_death, base.WorldPosition));
		_inc(Stat.SlimesKilled);
		for (int i = 0; i < 3; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.slime_drop_1, 30), null));
		}
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = SciHelper.GetRandomVectorInCircle(0.6f);
		}).OnUpdate(delegate(Particle p)
		{
			if (p.Age < 20)
			{
				p.Position += p.Velocity;
				p.Velocity += new Vector2(0f, 0.05f);
			}
			p.Dead = p.Age > 70;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z].DrawDotW(p.Position.X, p.Position.Y - 8f, default(Color).FromRgb(2656789) * ((float)(70 - p.Age) / 70f), 1f);
			})
			.Burst(20);
	}

	public override void Draw()
	{
		if (!fleeing)
		{
			base.core.Renderer[base.Z].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(0f, -4f), null, new Vector2(1f + (float)consuming * 0.01f, 1f + (float)consuming * 0.016f), 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (consuming > 0)
			{
				float amount = 1f - Component._M(consuming - 80, 0f) / 20f;
				Vector2 vector = new Vector2(MathHelper.Lerp(playerDeathPos.X, base.WorldCenter.X, amount), MathHelper.Lerp(playerDeathPos.Y, base.WorldCenter.Y, amount));
				base.core.Renderer[base.Z].DrawSpriteW(playerSprite, vector, scale: new Vector2(0.5f + (float)consuming * 0.005f), tint: default(Color).FromRgb(8439569) * ((float)consuming * 0.01f), rotation: (float)(100 - consuming) * 0.01f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			}
		}
		else
		{
			float num = (float)(70 - fleeDelay) / 50f;
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.slime_3), base.WorldCenter.Shift(0f, 4f), null, rotation: Component._sin((float)base.worldTicks * 1.3f) * 0.2f, scale: new Vector2(1f - 0.2f * num, 0.8f + 0.4f * num), flip: SpriteFlip.None, origin: SpriteOrigin.BottomCenter);
		}
		base.core.Renderer["bg", base.Z + 80, false].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(0f, 1f), Color.Black * 0.2f, new Vector2(1f + (float)consuming * 0.01f, 1f + (float)consuming * 0.016f), 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		if (IsBroken) return;
		if (offender is CreepChar)
		{
			fleeing = true;
		}
		else
		{
			IsBroken = true;
			DieAndSpawnFraments();
		}
		base.Break(offender);
	}

	public override void CollideWith(Entity other)
	{
		if (IsBroken)
		{
			return;
		}
		if (other is PlayerEntity playerEntity)
		{
			playerEntity.Hurt(InjuryType.Slime, this);
			if (playerEntity.Dead)
			{
				consuming = 100;
				playerSprite = playerEntity.CurrentSprite();
				playerDeathPos = playerEntity.WorldCenter;
				SendMessage(new PlayWorldSoundMessage(SoundName.slime_consume, base.WorldPosition));
				base.core.ParticleManager.AddEmitter(inWorld: true, Vector2.Zero, 5f).OnSpawn(delegate
				{
				}).OnUpdate(delegate(Particle p)
				{
					p.Dead = p.Age > 90;
				})
					.OnDraw(delegate(Particle p)
					{
						Vector2 position = p.Position;
						base.core.Renderer[base.Z + 1].DrawDotW(position.X + Component._sin((float)(base.worldTicks + p.Age) / 20f), position.Y - (float)p.Age + Component._cos((float)(base.worldTicks + p.Age) / 20f), Color.Lerp(default(Color).FromRgb(2656789), default(Color).FromRgb(8439569), 1f - (float)p.Age / 90f) * (Component._m(60 - p.Age, 10f) / 10f), 1f - (float)p.Age / 180f);
					})
					.AttachTo(this)
					.Emit(5, 5, once: true, 4);
			}
		}
		base.CollideWith(other);
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
