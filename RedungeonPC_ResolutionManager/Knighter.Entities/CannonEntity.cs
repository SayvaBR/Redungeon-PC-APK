using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class CannonEntity : Entity
{
	private int delay;

	private int defaultSpeed;

	private int defaultDistance;

	private int defaultPause;

	private readonly List<int> xDirs;

	private readonly List<int> yDirs;

	private readonly List<int> pauses;

	private readonly List<int> speeds;

	private readonly List<int> distances;

	private int cycle;

	private bool n;

	private bool e;

	private bool w;

	private bool s;

	private bool flipped;

	private readonly List<Sprite> baseSprites;

	private readonly List<Sprite> nSprites;

	private readonly List<Sprite> eSprites;

	private readonly List<Sprite> wSprites;

	private readonly List<Sprite> sSprites;

	private readonly List<Sprite> sPassiveSprites;

	private const int MaxFireT = 30;

	private int fireT;

	private bool nFiring;

	private bool eFiring;

	private bool wFiring;

	private bool sFiring;

	private Animation smokeAnimation;

	private int chargesLeft = 4;

	private Light light;

	private bool hasCharge => chargesLeft > 0;

	public BallType Type { get; private set; }

	public CannonEntity(int x, int y, TileDesc desc, BallType type)
		: base(x, y, 1f, 1f)
	{
		Type = type;
		xDirs = new List<int>();
		yDirs = new List<int>();
		pauses = new List<int>();
		speeds = new List<int>();
		distances = new List<int>();
		delay = desc["delay"];
		defaultSpeed = desc["default-speed"];
		defaultDistance = desc["default-distance"];
		defaultPause = desc["default-pause"];
		flipped = desc.Flipped;
		smokeAnimation = new Animation(0.13f);
		smokeAnimation.Add("smoke", "grill_smoke_", "1234");
		smokeAnimation.Play("smoke");
		smokeAnimation.SkipToRandomFrame();
		ReadProgram(desc.Str("program"));
		baseSprites = new List<Sprite>
		{
			_(SpriteName.cannon_base_1),
			_(SpriteName.cannon_base_2),
			_(SpriteName.cannon_base_3),
			_(SpriteName.cannon_base_1)
		};
		nSprites = new List<Sprite>
		{
			_(SpriteName.cannon_n_1),
			_(SpriteName.cannon_n_2),
			_(SpriteName.cannon_n_3),
			_(SpriteName.cannon_n_1)
		};
		eSprites = new List<Sprite>
		{
			_(SpriteName.cannon_e_1),
			_(SpriteName.cannon_e_2),
			_(SpriteName.cannon_e_3),
			_(SpriteName.cannon_e_1)
		};
		wSprites = new List<Sprite>
		{
			_(SpriteName.cannon_w_1),
			_(SpriteName.cannon_w_2),
			_(SpriteName.cannon_w_3),
			_(SpriteName.cannon_w_1)
		};
		sSprites = new List<Sprite>
		{
			_(SpriteName.cannon_s_1),
			_(SpriteName.cannon_s_2),
			_(SpriteName.cannon_s_3),
			_(SpriteName.cannon_s_1)
		};
		sPassiveSprites = new List<Sprite>
		{
			_(SpriteName.cannon_s_1),
			_(SpriteName.cannon_s_passive_2),
			_(SpriteName.cannon_s_passive_3),
			_(SpriteName.cannon_s_1)
		};
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(Color.Red, 1.5f, 0.8f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 1f;
		light.Active = false;
		base.Load();
	}

	private void ReadProgram(string program)
	{
		xDirs.Clear();
		yDirs.Clear();
		pauses.Clear();
		speeds.Clear();
		distances.Clear();
		cycle = 0;
		int num = 0;
		int num2 = 0;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		n = false;
		e = false;
		w = false;
		s = false;
		string[] array = program.Split(new char[1] { ',' });
		for (int i = 0; i < array.Length; i++)
		{
			string cmd = array[i];
			bool flag = true;
			do
			{
				char c = cmd[0];
				if (flipped)
				{
					switch (c)
					{
					case 'e':
						c = 'w';
						break;
					case 'w':
						c = 'e';
						break;
					}
				}
				if (c == 'n' || c == 'e' || c == 's' || c == 'w')
				{
					switch (c)
					{
					case 'n':
						num2 = ((num2 == 0) ? (-1) : 2);
						n = true;
						break;
					case 's':
						num2 = ((num2 == 0) ? 1 : 2);
						s = true;
						break;
					case 'w':
						num = ((num == 0) ? (-1) : 2);
						w = true;
						break;
					case 'e':
						num = ((num == 0) ? 1 : 2);
						e = true;
						break;
					}
					cmd = cmd.Substring(1);
				}
				else
				{
					flag = false;
				}
			}
			while (flag && cmd.Length > 0);
			while (cmd.Length > 0)
			{
				char c = cmd[0];
				cmd = cmd.Substring(1);
				int num6 = ReadInt(ref cmd);
				switch (c)
				{
				case '@':
					num5 = num6;
					break;
				case '>':
					num4 = num6;
					break;
				case ':':
					num3 = num6;
					break;
				}
			}
			xDirs.Add(num);
			yDirs.Add(num2);
			speeds.Add((num5 >= 0) ? num5 : defaultSpeed);
			distances.Add((num4 >= 0) ? num4 : defaultDistance);
			num3 = ((num3 >= 0) ? num3 : defaultPause);
			pauses.Add(num3);
			cycle += num3;
			num4 = -1;
			num3 = -1;
			num5 = -1;
			num = 0;
			num2 = 0;
		}
	}

	private int ReadInt(ref string cmd)
	{
		int i;
		for (i = 0; i < cmd.Length && char.IsDigit(cmd[i]); i++)
		{
		}
		int result = int.Parse(cmd.Substring(0, i));
		cmd = cmd.Substring(i);
		return result;
	}

	public void LoseCharge()
	{
		chargesLeft--;
		if (chargesLeft == 0)
		{
			light.Active = true;
		}
	}

	public override void Update()
	{
		if (IsBroken)
		{
			base.Age++;
			smokeAnimation.Update();
			return;
		}
		if (!hasCharge)
		{
			nFiring = (eFiring = (wFiring = (sFiring = false)));
			fireT = 0;
			return;
		}
		int num = Math.Max(base.worldTicks - delay, 0) % cycle;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		while (!flag)
		{
			int num4 = pauses[num2];
			if (num3 + num4 > num)
			{
				flag = true;
				continue;
			}
			num2++;
			num3 += num4;
		}
		if (num3 == num)
		{
			int num5 = xDirs[num2];
			int num6 = yDirs[num2];
			int[] array = new int[4]
			{
				(num5 == 1 || num5 == 2) ? 1 : 0,
				(num5 == -1 || num5 == 2) ? (-1) : 0,
				0,
				0
			};
			int[] array2 = new int[4]
			{
				0,
				0,
				(num6 == 1 || num6 == 2) ? 1 : 0,
				(num6 == -1 || num6 == 2) ? (-1) : 0
			};
			bool flag2 = fireT == 0;
			for (int i = 0; i < 4; i++)
			{
				int num7 = array[i];
				int num8 = array2[i];
				if (flag2 && (num7 != 0 || num8 != 0))
				{
					CannonballEntity entity = new CannonballEntity(this, base.WorldCenterCoordinates.X, base.WorldCenterCoordinates.Y, num7, num8, speeds[num2], distances[num2], Type);
					nFiring |= num8 < 0;
					eFiring |= num7 > 0;
					wFiring |= num7 < 0;
					sFiring |= num8 > 0;
					SendMessage(new SpawnEntityMessage(entity, null));
					SendMessage(new PlayWorldSoundMessage((Type == BallType.Fire) ? SoundName.cannon_fire : SoundName.cannon_zap, base.WorldCenter));
					fireT = 25;
				}
			}
		}
		if (fireT > 0)
		{
			fireT--;
		}
		else
		{
			nFiring = (eFiring = (wFiring = (sFiring = false)));
		}
		base.Update();
	}

	public override void Draw()
	{
		int num = (30 - fireT - 1) * (baseSprites.Count + 2) / 30;
		if (num == 2)
		{
			num--;
		}
		if (num >= 3)
		{
			num -= 2;
		}
		
		// Corrigido: Substituída a atribuição inválida por variável segura local
		var dummyBase = baseSprites[num];
		
		if (!hasCharge)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.panicbot_charge_icon), base.WorldCenter.Shift(0f, -22f + 2f * Component._sin((float)base.worldTicks * 0.1f)), Color.Red * (0.9f - Component._rnd(0f, 0.3f)), rotation: 0.3f + 0.1f * Component._cos((float)base.worldTicks * 0.1f), scale: Vector2.One * (0.9f + 0.1f * Component._sin((float)base.worldTicks * 0.1f)), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		}
		if (n && !IsBroken)
		{
			Vector2 vector = (nFiring ? new Vector2(0f, -5f) : (num switch
			{
				2 => new Vector2(0f, -6f), 
				1 => new Vector2(0f, -4f), 
				_ => new Vector2(0f, -5f), 
			}));
			base.core.Renderer[base.Z, !hasCharge].DrawSpriteW(nFiring ? nSprites[num] : nSprites[0], base.WorldCenter + vector, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (nFiring && num >= 2)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_("cannon_flash_n_" + ((Type == BallType.Fire) ? "fire" : "zap")), base.WorldCenter.Shift(0f, -16.5f), Color.White * ((num == 2) ? 1f : 0.5f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		Sprite sprite = ((!IsBroken) ? baseSprites[num] : _(SpriteName.cannon_base_broken));
		base.core.Renderer[base.Z, !hasCharge].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, -5f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		if (e && !IsBroken)
		{
			Vector2 vector2 = (eFiring ? new Vector2(0f, -5f) : (num switch
			{
				2 => new Vector2(-1f, -6f), 
				1 => new Vector2(1f, -4f), 
				_ => new Vector2(0f, -5f), 
			}));
			base.core.Renderer[base.Z, !hasCharge].DrawSpriteW(eFiring ? eSprites[num] : eSprites[0], base.WorldCenter + vector2, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (eFiring && num >= 2)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_("cannon_flash_e_" + ((Type == BallType.Fire) ? "fire" : "zap")), base.WorldCenter.Shift(14f, -5f), Color.White * ((num == 2) ? 1f : 0.5f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		if (w && !IsBroken)
		{
			Vector2 vector3 = (wFiring ? new Vector2(0f, -5f) : (num switch
			{
				2 => new Vector2(1f, -6f), 
				1 => new Vector2(-1f, -4f), 
				_ => new Vector2(0f, -5f), 
			}));
			base.core.Renderer[base.Z, !hasCharge].DrawSpriteW(wFiring ? wSprites[num] : wSprites[0], base.WorldCenter + vector3, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (wFiring && num >= 2)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_("cannon_flash_w_" + ((Type == BallType.Fire) ? "fire" : "zap")), base.WorldCenter.Shift(-14f, -5f), Color.White * ((num == 2) ? 1f : 0.5f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		if (s && !IsBroken)
		{
			base.core.Renderer[base.Z, !hasCharge].DrawSpriteW(sFiring ? sSprites[num] : sPassiveSprites[num], base.WorldCenter.Shift(0f, -5f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (sFiring && num >= 2)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_("cannon_flash_s_" + ((Type == BallType.Fire) ? "fire" : "zap")), base.WorldCenter.Shift(0f, 4f), Color.White * ((num == 2) ? 1f : 0.5f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		if (IsBroken)
		{
			base.core.Renderer[base.Z].DrawSpriteW(smokeAnimation.GetCurrentFrame(), base.WorldCenter.Shift(0f, -9f), Color.White * 0.4f, new Vector2(1f, 0.8f + 0.2f * Component._sin(((float)base.Age + 20f * x + 20f * y) * 0.1f)), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldCenter.Shift(-12.5f, -3f), Color.Black * 0.2f, new Vector2(1f, 0.7f), 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!(other is FragmentEntity))
		{
			if (other is PlayerEntity)
			{
				return other.Flying;
			}
			return false;
		}
		return true;
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
		if (n)
		{
			SpawnBarrelShards(new Vector2(0f, -10f));
		}
		if (s)
		{
			SpawnBarrelShards(new Vector2(0f, 10f));
		}
		if (w)
		{
			SpawnBarrelShards(new Vector2(-10f, 0f));
		}
		if (e)
		{
			SpawnBarrelShards(new Vector2(10f, 0f));
		}
		base.Break(offender);
	}

	private void SpawnBarrelShards(Vector2 shift)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates + shift / 16f, SpriteName.cannon_shard_1, 70, new Vector4(shift.X / 150f + Component._rnd(-0.02f, 0.01f), shift.Y / 150f + Component._rnd(-0.02f, 0.01f), Component._rnd(1f, 1.5f), Component._rnd(-0.05f, 0.05f))), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates + shift / 16f, SpriteName.cannon_shard_2, 70, new Vector4(shift.X / 150f + Component._rnd(-0.02f, 0.01f), shift.Y / 150f + Component._rnd(-0.02f, 0.01f), Component._rnd(1f, 1.5f), Component._rnd(-0.05f, 0.05f))), null));
	}

	public override void CollideWith(Entity other)
	{
		base.CollideWith(other);
	}
}