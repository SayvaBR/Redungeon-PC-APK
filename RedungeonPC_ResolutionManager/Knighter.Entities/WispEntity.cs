using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class WispEntity : Entity
{
	private int delay;

	private int defaultPause;

	private int speed;

	private int cycle;

	private readonly List<int> dxs;

	private readonly List<int> dys;

	private readonly List<int> pauses;

	private bool lookAtPlayer;

	private readonly float ox;

	private readonly float oy;

	private Color cBase;

	private Color cLight;

	private Color cMain;

	private Color cGlow;

	private Color cParticleTint = Color.White;

	private string particlePrefix;

	private int particleFrames;

	private Sprite bodySprite;

	private Light light;

	private ParticleEmitter emitter;

	private float angle;

	private float targetAngle;

	private float nextX;

	private float nextY;

	private bool moving;

	private static BagOf<SoundName> idleSounds;

	private int idleSoundDelay;

	public WispType Type { get; private set; }

	static WispEntity()
	{
		idleSounds = new BagOf<SoundName>().Put(SoundName.wisp_idle_1).Put(SoundName.wisp_idle_2).Put(SoundName.wisp_idle_3)
			.Put(SoundName.wisp_idle_4)
			.Put(SoundName.wisp_idle_5)
			.Put(SoundName.wisp_idle_6);
	}

	public WispEntity(int x, int y, TileDesc desc)
		: base(x, y, 0.1f, 0.1f)
	{
		dxs = new List<int>();
		dys = new List<int>();
		pauses = new List<int>();
		Init(desc);
		ox = (float)x + 0.45f;
		oy = (float)y + 0.45f;
		base.x = ox;
		base.y = oy;
	}

	private void Init(TileDesc desc)
	{
		delay = desc["delay"];
		defaultPause = desc["default-pause"];
		speed = desc["speed"];
		lookAtPlayer = desc["look-at-player"] == 1;
		ReadPath(desc.Str("path"), desc.Flipped);
		BagOf<WispType> bagOf = new BagOf<WispType>();
		bagOf.Put(WispType.Fire, desc["fire"]);
		bagOf.Put(WispType.Zap, desc["zap"]);
		bagOf.Put(WispType.Snow, desc["snow"]);
		bagOf.Put(WispType.Dark, desc["dark"]);
		bagOf.Put(WispType.Poison, desc["poison"]);
		bagOf.Put(WispType.Confusion, desc["confusion"]);
		Type = bagOf.Draw();
		switch (Type)
		{
		case WispType.Dark:
			cBase = default(Color).FromRgb(3211280);
			cLight = default(Color).FromRgb(16738262);
			cMain = default(Color).FromRgb(14876838);
			cGlow = cLight;
			cParticleTint = Color.White;
			bodySprite = _(SpriteName.wisp_body_darkness);
			particlePrefix = "wisp_particle_dark_";
			particleFrames = 5;
			break;
		case WispType.Zap:
			cBase = default(Color).FromRgb(204870);
			cLight = default(Color).FromRgb(6996223);
			cMain = default(Color).FromRgb(7518707);
			cGlow = cLight;
			cParticleTint = Color.White;
			bodySprite = _(SpriteName.wisp_body_zap);
			particlePrefix = "zap_particle_";
			particleFrames = 6;
			break;
		case WispType.Fire:
			cLight = default(Color).FromRgb(16759608);
			cMain = default(Color).FromRgb(16565559);
			cBase = default(Color).FromRgb(8856320);
			cGlow = cLight;
			cParticleTint = cMain;
			bodySprite = _(SpriteName.wisp_body_fire);
			particlePrefix = "circle_";
			particleFrames = 6;
			break;
		case WispType.Snow:
			cBase = default(Color).FromRgb(6386852);
			cLight = default(Color).FromRgb(12638456);
			cMain = default(Color).FromRgb(13558783);
			cGlow = cLight;
			cParticleTint = Color.White;
			bodySprite = _(SpriteName.wisp_body_snow);
			particlePrefix = "wisp_particle_snow_";
			particleFrames = 6;
			break;
		case WispType.Poison:
			cBase = default(Color).FromRgb(1323264);
			cLight = default(Color).FromRgb(11062016);
			cMain = default(Color).FromRgb(8169755);
			cGlow = cLight;
			cParticleTint = Color.White;
			bodySprite = _(SpriteName.wisp_body_poison);
			particlePrefix = "wisp_particle_bubble_";
			particleFrames = 6;
			break;
		case WispType.Confusion:
			cLight = default(Color).FromRgb(16640817);
			cMain = default(Color).FromRgb(16565559);
			cBase = default(Color).FromRgb(16777215);
			cGlow = default(Color).FromRgb(16565559);
			cParticleTint = Color.White;
			bodySprite = _(SpriteName.wisp_body_confusion);
			particlePrefix = "wisp_particle_star_";
			particleFrames = 5;
			break;
		default:
			cBase = default(Color).FromRgb(6750271);
			cLight = default(Color).FromRgb(14821508);
			cMain = default(Color).FromRgb(14821508);
			break;
		}
		angle = (float)Math.PI / 2f;
		targetAngle = angle;
		idleSoundDelay = Component._rnd(60, 120);
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(cLight, 2f, 0.7f, this);
		emitter = base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldPosition).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			int num = Component._rnd(0, 100);
			p.Aux.X = ((num >= 60) ? 1 : 0);
			p.Aux.Z = base.Z;
			p.Offset = (moving ? (new Vector2(nextX + 0.05f, nextY + 0.05f) * 16f) : base.WorldCenter);
			float num2 = Component._rnd(0f, (float)Math.PI * 2f);
			if (p.Aux.X.IsEqualTo(0f))
			{
				float num3 = (float)Component._rnd(6, 8) * (IsBroken ? Component._rnd(0.5f, 2f) : 1f);
				p.Velocity = p.Position + new Vector2(Component._cos(num2), Component._sin(num2)) * num3;
				p.Position += new Vector2(Component._cos(num2), Component._sin(num2)) * 3f;
				p.Offset += new Vector2(Component._cos(num2), Component._sin(num2)) * 3f;
			}
			else if (p.Aux.X.IsEqualTo(1f))
			{
				float num4 = (float)Component._rnd(8, 10) * (IsBroken ? Component._rnd(0.5f, 2f) : 1f);
				p.Velocity = p.Position + new Vector2(Component._cos(num2), Component._sin(num2)) * num4;
				p.Position += new Vector2(Component._cos(num2), Component._sin(num2)) * 4.5f;
				p.Offset += new Vector2(Component._cos(num2), Component._sin(num2)) * 4.5f;
			}
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Position += (p.Velocity - p.Position) * ((p.Age < 5) ? 0.15f : 0.015f);
				p.Dead = p.Age >= 50;
				if (p.Age >= 5 && !IsBroken)
				{
					p.Velocity = p.Offset;
				}
			})
			.OnDraw(delegate(Particle p)
			{
				int num = (int)((float)particleFrames * (1f - (float)p.Age / 50f));
				num = (int)Component._M(num, 1f);
				base.core.Renderer[(int)p.Aux.Z - 2].DrawSpriteW(_(particlePrefix + num), p.Position.Shift(0f, -7f), cParticleTint * ((p.Aux.X > 0.5f) ? 0.5f : 1f), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Max(26)
			.Start(count: 2, interval: 6);
		base.Load();
	}

	public void ParticleBurst()
	{
		emitter.Max(60).Emit(8, 2, once: true, 20);
	}

	public override void Update()
	{
		bool enhancedGlow = base.core.OptionsData.EnhancedWispGlow;
		float glowPulse = 0.5f + 0.5f * Component._sin((float)base.Age * 0.075f);
		light.TargetRadius = enhancedGlow ? 3.15f + glowPulse * 0.35f : 2f;
		light.TargetIntencity = enhancedGlow ? 0.95f + glowPulse * 0.15f : 0.7f;
		light.ChangeRate = 0.12f;
		FollowPath();
		UpdateTiles();
		Vector2 vector = new Vector2(nextX + 0.05f, nextY + 0.05f) - base.WorldCenterCoordinates;
		if (cycle == 0 || lookAtPlayer)
		{
			vector = base.core.CurrentPlayState.Player.WorldCenterCoordinates - base.WorldCenterCoordinates;
		}
		targetAngle = 0f - (float)Math.Atan2(vector.X, vector.Y) + (float)Math.PI / 2f;
		float num = targetAngle;
		if (Math.Abs(num - angle) > Math.Abs(num + (float)Math.PI * 2f - angle))
		{
			num += (float)Math.PI * 2f;
		}
		else if (Math.Abs(num - angle) > Math.Abs(num - (float)Math.PI * 2f - angle))
		{
			num -= (float)Math.PI * 2f;
		}
		angle += (num - angle) * 0.3f;
		if (angle > (float)Math.PI * 2f)
		{
			angle -= (float)Math.PI * 2f;
		}
		if (angle < 0f)
		{
			angle += (float)Math.PI * 2f;
		}
		base.Update();
	}

	private void DrawEye(Vector2 center, float angle)
	{
		float num = Component._sin(angle) * 3f;
		float num2 = Component._cos(angle) * 5f;
		base.core.Renderer[base.Z + ((!(num < 0f)) ? 1 : (-1)) - 1].DrawSpriteW(_(SpriteName.wisp_eye_glow), center.Shift(num2, num - 0.5f), cMain, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z + ((!(num < 0f)) ? 1 : (-1))].DrawSpriteW(_(SpriteName.circle_2), center.Shift(num2, num - 0.5f), Color.White, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
	}

	public override void Draw()
	{
		DrawWisp();
	}

	private void DrawFloorLight()
	{
		int x = (int)WorldCenterCoordinates.X, y = (int)WorldCenterCoordinates.Y;
		for (int dy = -3; dy <= 3; dy++)
		for (int dx = -3; dx <= 3; dx++)
			if (levelMap[x + dx, y + dy] is Knighter.Tiles.DungeonTile floor)
				floor.DrawWispLight(WorldCenter, cGlow);
	}

	private void DrawWisp()
	{
		float num = 7.5f + Component._sin((float)base.worldTicks * 0.05f);
		Vector2 vector = base.WorldCenter.Shift(0f, 0f - num);
		if (base.core.OptionsData.EnhancedWispGlow) DrawFloorLight();
		base.core.Renderer[base.Z - 2].DrawSpriteW(_(SpriteName.glow_big), vector, cGlow * 0.8f, new Vector2(0.6f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z - 2].DrawSpriteW(bodySprite, vector, Color.White, new Vector2(1.1f + 0.05f * Component._sin((float)base.Age * 0.05f)), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.circle_6), vector, cBase, new Vector2(1.1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		DrawEye(vector, angle + 0.5f);
		DrawEye(vector, angle - 0.5f);
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		if (IsBroken) return;
		if (offender is PlayerEntity)
		{
			_inc(Stat.WispsKilled);
		}
		IsBroken = true;
		if (offender == this)
		{
			emitter.AttachTo(base.core.CurrentPlayState.Player).Max(60).Emit(5, 2, once: true, 12);
		}
		else
		{
			emitter.AttachTo(null).Max(60).Emit(5, 2, once: true, 12);
			SendMessage(new PlayWorldSoundMessage(idleSounds.DrawDifferent(), base.WorldCenter));
		}
		SendMessage(new RemoveEntityMessage(this));
		base.Break(offender);
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity playerEntity)
		{
			switch (Type)
			{
			case WispType.Zap:
				playerEntity.Hurt(InjuryType.Zap, this);
				if (playerEntity.Dead)
				{
					SendMessage(new SpawnEntityMessage(new ZappedEffectEntity(playerEntity), null));
				}
				break;
			case WispType.Fire:
				playerEntity.Hurt(InjuryType.Flame, this);
				break;
			case WispType.Poison:
				playerEntity.ApplySpell(SpellType.Poison);
				break;
			case WispType.Snow:
				playerEntity.ApplySpell(SpellType.Ice);
				break;
			case WispType.Dark:
				playerEntity.ApplySpell(SpellType.Darkness);
				break;
			case WispType.Confusion:
				playerEntity.ApplySpell(SpellType.Confusion);
				break;
			}
			Break(this);
		}
		base.CollideWith(other);
	}

	private int ReadInt(ref string node)
	{
		int i;
		for (i = 0; i < node.Length && char.IsDigit(node[i]); i++)
		{
		}
		int result = int.Parse(node.Substring(0, i));
		node = node.Substring(i);
		return result;
	}

	private void ReadPath(string path, bool flipped)
	{
		dxs.Clear();
		dys.Clear();
		pauses.Clear();
		cycle = 0;
		if (path == "")
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		string[] array = path.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string node = array[i];
			int num3 = 0;
			int num4 = 0;
			while (node.Length > 0)
			{
				char c = node[0];
				if (flipped)
				{
					switch (c)
					{
					case 'w':
						c = 'e';
						break;
					case 'e':
						c = 'w';
						break;
					}
				}
				node = node.Substring(1);
				int num5 = ReadInt(ref node);
				switch (c)
				{
				case '-':
					pauses.Add(num5);
					break;
				case 'e':
				case 'n':
				case 's':
				case 'w':
					if (c == 'n')
					{
						num4 = -num5;
					}
					if (c == 's')
					{
						num4 = num5;
					}
					if (c == 'e')
					{
						num3 = num5;
					}
					if (c == 'w')
					{
						num3 = -num5;
					}
					break;
				}
			}
			num += num3;
			num2 += num4;
			dxs.Add(num3);
			dys.Add(num4);
		}
		if (num != 0 || num2 != 0)
		{
			dxs.Add(-num);
			dys.Add(-num2);
		}
		for (int j = 0; j < dxs.Count; j++)
		{
			int num6 = dxs[j];
			int num7 = dys[j];
			double num8 = Math.Sqrt(num6 * num6 + num7 * num7);
			cycle += (int)(num8 * (double)speed);
			if (num6 != 0 || num7 != 0)
			{
				cycle += defaultPause;
			}
		}
		foreach (int pause in pauses)
		{
			cycle += pause;
		}
	}

	private void FollowPath()
	{
		if (cycle == 0)
		{
			nextX = base.WorldCoordinates.X;
			nextY = base.WorldCoordinates.Y;
			moving = false;
			return;
		}
		int num = (base.worldTicks - delay) % cycle;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		x = ox;
		y = oy;
		while (!flag && num2 < dxs.Count)
		{
			int num4 = dxs[num2];
			int num5 = dys[num2];
			int num6 = 0;
			num6 = ((num4 != 0 || num5 != 0) ? ((int)(Math.Sqrt(num4 * num4 + num5 * num5) * (double)speed)) : pauses[num3++]);
			if (num6 <= num)
			{
				num -= num6;
				x += num4;
				y += num5;
				num2++;
				if (num4 != 0 || num5 != 0)
				{
					num -= defaultPause;
				}
				flag = num <= 0;
				if (flag)
				{
					int num7 = num2;
					if (num7 >= dxs.Count)
					{
						num7 = 0;
					}
					nextX = x + (float)dxs[num7];
					nextY = y + (float)dys[num7];
					moving = false;
				}
				continue;
			}
			int num8 = num2;
			if (dxs[num8] == 0 && dys[num8] == 0)
			{
				num8++;
				if (num8 >= dxs.Count)
				{
					num8 = 0;
				}
			}
			nextX = x + (float)dxs[num8];
			nextY = y + (float)dys[num8];
			x += (float)num4 * (float)num / (float)num6;
			y += (float)num5 * (float)num / (float)num6;
			flag = true;
			moving = true;
		}
	}
}
