using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ZapperEntity : Entity
{
	private string program;

	private int delay;

	private int defaultDelay;

	private List<int> xDirs;

	private List<int> yDirs;

	private List<int> delays;

	private int cycle;

	private int xDir;

	private int yDir;

	private bool zapping;

	private string lastSet = "";

	private Sprite pole;

	private Sprite glow;

	private Sprite brokenPole;

	private Sprite brokenGlow;

	private List<ZapperEntity> targets;

	private bool incoming;

	private Light light;

	private ZapperEntity secondPlayerZapper;

	private bool depletedThisTick;

	private int power = 80;

	public bool IsPanicBotStation;

	public LevelGenerator.GeneratedModule ParentModule { get; private set; }

	public ZapperEntity(int x, int y, TileDesc desc)
		: base((float)x + 0.25f, (float)y + 0.25f, 0.5f, 0.5f)
	{
		Init(desc.Str("program"), desc["delay"], desc["default-delay"], desc.Flipped);
	}

	public ZapperEntity(int x, int y, string program, int delay, int defaultDelay, bool flipped)
		: base((float)x + 0.25f, (float)y + 0.25f, 0.5f, 0.5f)
	{
		Init(program, delay, defaultDelay, flipped);
	}

	public ZapperEntity SetModule(LevelGenerator.GeneratedModule module)
	{
		ParentModule = module;
		return this;
	}

	private void Init(string program, int delay, int defaultDelay, bool flipped = false)
	{
		xDir = 0;
		yDir = 0;
		xDirs = new List<int>();
		yDirs = new List<int>();
		delays = new List<int>();
		targets = new List<ZapperEntity>();
		if (flipped)
		{
			program = program.Replace("e", "!");
			program = program.Replace("w", "e");
			program = program.Replace("!", "w");
		}
		this.program = program;
		this.delay = delay;
		this.defaultDelay = defaultDelay;
		ReadProgram();
		pole = _(SpriteName.zapper);
		glow = _(SpriteName.zapper_glow);
		brokenPole = _(SpriteName.zapper_broken);
		brokenGlow = _(SpriteName.zapper_broken_glow);
	}

	private void ReadProgram()
	{
		xDirs.Clear();
		yDirs.Clear();
		delays.Clear();
		cycle = 0;
		int i = 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		for (; i < program.Length; i++)
		{
			char c = program[i];
			if (c == 'n' || c == 'e' || c == 's' || c == 'w' || c == '-')
			{
				switch (c)
				{
				case 'n':
					num2 = ((num2 == 0) ? (-1) : 2);
					break;
				case 's':
					num2 = ((num2 == 0) ? 1 : 2);
					break;
				case 'w':
					num = ((num == 0) ? (-1) : 2);
					break;
				case 'e':
					num = ((num == 0) ? 1 : 2);
					break;
				case '-':
					num = 0;
					num2 = 0;
					break;
				}
			}
			if (c >= '0' && c <= '9')
			{
				num3 = num3 * 10 + (c - 48);
				flag = true;
			}
			if (c == ',' || i + 1 == program.Length)
			{
				int num4 = (flag ? num3 : defaultDelay);
				delays.Add(num4);
				xDirs.Add(num);
				yDirs.Add(num2);
				cycle += num4;
				num = 0;
				num2 = 0;
				num3 = 0;
				flag = false;
			}
		}
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(6996223), 1.5f, 0.7f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 1f;
		light.Active = false;
		UpdateTiles();
		base.Load();
	}

	public override void Update()
	{
		depletedThisTick = false;
		if (!HasPower())
		{
			zapping = false;
			targets.Clear();
			if (!IsPanicBotStation)
			{
				light.Color = Color.Red;
				light.Active = true;
				light.Intencity = 0.8f;
			}
			else
			{
				light.TargetIntencity = 0f;
			}
			return;
		}
		int num = (int)Component._M(base.worldTicks - delay, 0f) % cycle;
		int num2 = 0;
		bool flag = false;
		xDir = 0;
		yDir = 0;
		while (!flag)
		{
			if (num2 >= xDirs.Count)
			{
				flag = true;
				continue;
			}
			int num3 = xDirs[num2];
			int num4 = yDirs[num2];
			int num5 = delays[num2];
			if (num5 == -1)
			{
				num5 = defaultDelay;
			}
			if (num >= num5)
			{
				num -= num5;
			}
			else
			{
				flag = true;
				xDir = num3;
				yDir = num4;
			}
			num2++;
		}
		targets.Clear();
		string text = "";
		if (xDir == -1 || xDir == 2)
		{
			FindZapper(-1, 0);
			text += "l";
		}
		if (xDir == 1 || xDir == 2)
		{
			FindZapper(1, 0);
			text += "r";
		}
		if (yDir == -1 || yDir == 2)
		{
			FindZapper(0, -1);
			text += "u";
		}
		if (yDir == 1 || yDir == 2)
		{
			FindZapper(0, 1);
			text += "d";
		}
		var _ = zapping;
		zapping = targets.Count > 0 || incoming;
		incoming = false;
		if (!IsBroken && lastSet != text)
		{
			SendMessage(new PlayWorldSoundMessage(SoundName.zap_on, base.WorldCenter));
		}
		lastSet = text;
		PlayerEntity player = base.core.CurrentPlayState.Player;
		secondPlayerZapper = null;
		if (!IsBroken)
		{
			float num6 = player.WorldCenterCoordinates.X;
			float num7 = player.WorldCenterCoordinates.Y;
			if (player != null && !player.Dead && (!player.Flying || !player.FlightIgnoresObstacles) && !player.TeleportPending)
			{
				foreach (ZapperEntity target in targets)
				{
					if (target.IsBroken)
					{
						continue;
					}
					float num8 = Component._m(target.WorldCenterCoordinates.X, base.WorldCenterCoordinates.X);
					float num9 = Component._M(target.WorldCenterCoordinates.X, base.WorldCenterCoordinates.X);
					float num10 = Component._m(target.WorldCenterCoordinates.Y, base.WorldCenterCoordinates.Y);
					float num11 = Component._M(target.WorldCenterCoordinates.Y, base.WorldCenterCoordinates.Y);
					if (num6 + 0.2f >= num8 && num6 - 0.2f <= num9 && num7 + 0.2f >= num10 && num7 - 0.2f <= num11)
					{
						secondPlayerZapper = target;
						player.Hurt(InjuryType.Zap, this);
						if (player.Dead)
						{
							SendMessage(new SpawnEntityMessage(new ZappedEffectEntity(player), null));
						}
						break;
					}
				}
			}
		}
		light.Active = zapping;
		base.Update();
	}

	private void FindZapper(int dx, int dy)
	{
		Vector2 v = base.WorldCoordinates;
		for (int i = 1; i <= 5; i++)
		{
			v = v.Shift(dx, dy);
			Tile tile = base.levelMap[(int)Math.Round(v.X - 0.5f), (int)Math.Round(v.Y - 0.5f)];
			if (tile == null)
			{
				break;
			}
			Entity entity = tile.Entities.Find((Entity e) => e is ZapperEntity && ((ZapperEntity)e).HasPower() && ((ZapperEntity)e).ParentModule == ParentModule);
			if (entity != null)
			{
				ZapperEntity zapperEntity = entity as ZapperEntity;
				if (!zapperEntity.HasAsTarget(this))
				{
					targets.Add(zapperEntity);
					zapperEntity.Incoming();
				}
				break;
			}
		}
	}

	public void Deplete()
	{
		if (!depletedThisTick && base.core.CurrentPlayState.Started)
		{
			power--;
			if (power == 0 && !IsPanicBotStation)
			{
				_inc(Stat.PanicBotZappersDepleted);
			}
			if (power < 0)
			{
				power = 0;
			}
			if (secondPlayerZapper != null)
			{
				secondPlayerZapper.Deplete();
			}
			depletedThisTick = true;
		}
	}

	public bool HasPower()
	{
		return power > 0;
	}

	public bool HasAsTarget(ZapperEntity zapper)
	{
		return targets.Contains(zapper);
	}

	public void Incoming()
	{
		incoming = true;
	}

	public override void Draw()
	{
		bool flag = !HasPower() && !IsPanicBotStation;
		base.core.Renderer[base.Z, flag].DrawSpriteW(IsBroken ? brokenPole : pole, base.WorldCenter.Shift(-8f, -16f));
		if (flag)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.panicbot_charge_icon), base.WorldCenter.Shift(0f, -22f + 2f * Component._sin((float)base.worldTicks * 0.1f)), Color.Red * (0.9f - Component._rnd(0f, 0.3f)), rotation: 0.3f + 0.1f * Component._cos((float)base.worldTicks * 0.1f), scale: Vector2.One * (0.9f + 0.1f * Component._sin((float)base.worldTicks * 0.1f)), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		}
		if (zapping)
		{
			base.core.Renderer[base.Z].DrawSpriteW(IsBroken ? brokenGlow : glow, base.WorldCenter.Shift(-8f, -16f), Color.White * (0.85f + 0.15f * Component._sin((float)base.worldTicks * 0.5f)));
			if (!IsBroken)
			{
				base.core.Renderer[base.Z].DrawSpriteW(_("zapball_" + (base.Age / 3 % 6 + 1)), base.WorldCenter.Shift(-10f, -21f), Color.White);
			}
			if (!IsBroken)
			{
				foreach (ZapperEntity target in targets)
				{
					if (!target.IsBroken)
					{
						base.core.Renderer[base.Z].DrawLineW(base.WorldCenter.Shift(0f + Component._rnd(-2f, 2f), -11f + Component._rnd(-2f, 2f)), target.WorldCenter.Shift(0f + Component._rnd(-2f, 2f), -11f + Component._rnd(-2f, 2f)), default(Color).FromRgb(9356269) * (0.3f + Component._rnd(0f, 0.3f)));
						base.core.Renderer[base.Z].DrawLineW(base.WorldCenter.Shift(0f + Component._rnd(-0.5f, 0.5f), -11f + Component._rnd(-0.5f, 0.5f)), target.WorldCenter.Shift(0f + Component._rnd(-0.5f, 0.5f), -11f + Component._rnd(-0.5f, 0.5f)), Color.White * (0.8f + Component._rnd(0f, 0.2f)));
					}
				}
			}
		}
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(IsBroken ? brokenPole : pole, base.WorldCenter.Shift(-8f, -2f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return other is FragmentEntity;
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		SendMessage(new PlayWorldSoundMessage(SoundName.rotoblade_break, base.WorldPosition));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(-0.1f, -0.4f), SpriteName.zapper_ball, Component._rnd(120, 180), new Vector4(Component._rnd(-0.13f, 0.13f), Component._rnd(-0.13f, 0.13f), Component._rnd(1.5f, 2.5f), 0f), "", 0.2f, 0.9f, 0.99f, SoundName.zapper_bounce), null));
		_inc(Stat.ZappersBroken);
		base.Break(offender);
	}
}
