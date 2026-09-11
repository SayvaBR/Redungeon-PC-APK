using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FirewallEntity : Entity
{
	private Animation anim;

	private readonly List<List<FireballEntity>> rays;

	private readonly int delay;

	private readonly int startDir;

	private readonly int ticksPer90;

	private readonly bool flipped;

	private string programString;

	private string raysString;

	private bool isStill;

	private readonly List<int> shifts;

	private readonly List<int> durations;

	private int cycle;

	private int da;

	private int dt;

	private int breakT;

	private int breakD = 30;

	public BallType Type { get; private set; }

	public FirewallEntity(int x, int y, TileDesc desc, BallType type)
		: base(x, y, 1f, 1f)
	{
		Type = type;
		anim = new Animation();
		switch (Type)
		{
		case BallType.Fire:
			anim.Add("spin", "fireball_", "12345678");
			break;
		case BallType.Zap:
			anim.Add("spin", "zapball_", "123456");
			break;
		}
		anim.Play("spin");
		anim.SkipToRandomFrame();
		delay = desc["delay"];
		startDir = desc["start-dir"];
		ticksPer90 = desc["ticks-per-90"];
		flipped = desc.Flipped;
		programString = desc.Str("program");
		raysString = desc.Str("rays");
		rays = new List<List<FireballEntity>>();
		shifts = new List<int>();
		durations = new List<int>();
		ReadProgram(programString);
		SpawnBalls(raysString);
	}

	public override void Unload()
	{
		foreach (List<FireballEntity> ray in rays)
		{
			foreach (FireballEntity item in ray)
			{
				SendMessage(new RemoveEntityMessage(item));
			}
		}
		base.Unload();
	}

	private int ReadInt(ref string cmd)
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < cmd.Length; i++)
		{
			char c = cmd[i];
			if (c == '+')
			{
				flag = false;
			}
			else if (c == '-')
			{
				flag = true;
			}
			else
			{
				if (!char.IsDigit(c))
				{
					cmd = cmd.Substring(i);
					break;
				}
				num = num * 10 + (c - 48);
			}
			if (i == cmd.Length - 1)
			{
				cmd = string.Empty;
			}
		}
		return num * ((!flag) ? 1 : (-1));
	}

	private void ReadProgram(string program)
	{
		isStill = program.Equals(string.Empty) || program.Equals("0");
		if (isStill)
		{
			return;
		}
		shifts.Clear();
		durations.Clear();
		cycle = 0;
		da = 0;
		if (program.Equals("+") || program.Equals("-"))
		{
			shifts.Add(360 * (program.Equals("+") ? 1 : (-1)));
			durations.Add(4 * ticksPer90);
			cycle = 4 * ticksPer90;
			return;
		}
		string[] array = program.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			char c = text[0];
			text = text.Substring(1);
			switch (c)
			{
			case ':':
			{
				int item2 = ReadInt(ref text);
				shifts.Add(0);
				durations.Add(item2);
				break;
			}
			case '@':
			{
				char c2 = text[0];
				int num4 = 1;
				if (c2 == '-' || c2 == '+')
				{
					if (c2 == '-')
					{
						num4 = -1;
					}
					text = text.Substring(1);
				}
				int num5 = ReadInt(ref text);
				if (text.Length > 0)
				{
					text = text.Substring(1);
				}
				int num6 = ReadInt(ref text);
				durations.Add(num6);
				shifts.Add((int)((float)(num4 * 90) * ((float)num6 / (float)num5)));
				break;
			}
			case '+':
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			{
				int num = 1;
				if (c == '-' || c == '+')
				{
					if (c == '-')
					{
						num = -1;
					}
				}
				else
				{
					text = c + text;
				}
				int num2 = ReadInt(ref text);
				int num3 = ticksPer90;
				if (!text.Equals(string.Empty))
				{
					text = text.Substring(1);
					num3 = ReadInt(ref text);
				}
				shifts.Add(num * num2);
				int item = (int)((float)num2 * ((float)num3 / 90f));
				durations.Add(item);
				break;
			}
			}
			cycle += durations[i];
			int num7 = shifts[i];
			da += num7;
		}
		dt = cycle;
		if (da != 0)
		{
			cycle *= SciHelper.LCM(360, da) / Math.Abs(da);
		}
	}

	private void SpawnBalls(string ballsInRays)
	{
		foreach (char c in ballsInRays)
		{
			if (char.IsDigit(c))
			{
				rays.Add(new List<FireballEntity>());
				for (int j = 0; j < c - 48; j++)
				{
					FireballEntity fireballEntity = new FireballEntity(this, 0f, 0f, Type);
					rays[rays.Count - 1].Add(fireballEntity);
					SendMessage(new SpawnEntityMessage(fireballEntity, null));
				}
			}
		}
	}

	private void UpdateRays()
	{
		float num = startDir;
		if (!isStill)
		{
			int num2 = base.worldTicks;
			num2 -= delay;
			num2 %= cycle;
			if (dt != 0)
			{
				num += (float)(da * (num2 / dt));
				num2 %= dt;
			}
			for (int i = 0; i < shifts.Count; i++)
			{
				int num3 = shifts[i];
				int num4 = durations[i];
				if (num2 > num4)
				{
					num += (float)num3;
					num2 -= num4;
					continue;
				}
				num += (float)num3 * (float)num2 / (float)num4;
				break;
			}
		}
		int count = rays.Count;
		float num5 = num;
		foreach (List<FireballEntity> ray in rays)
		{
			for (int j = 0; j < ray.Count; j++)
			{
				FireballEntity fireballEntity = ray[j];
				float num6 = base.WorldCenterCoordinates.X + 0.5f * (float)Math.Cos(Math.PI * (double)num5 / 180.0) * (float)(j + 1) * (flipped ? (-1f) : 1f);
				float num7 = base.WorldCenterCoordinates.Y + 0.5f * (float)Math.Sin(Math.PI * (double)num5 / 180.0) * (float)(j + 1);
				fireballEntity.UpdatePosition(num6, num7);
			}
			num5 += 360f / (float)count;
		}
	}

	public override void OnPlatformMoved()
	{
		UpdateRays();
		base.OnPlatformMoved();
	}

	public override void Update()
	{
		anim.Update();
		UpdateRays();
		if (IsBroken && breakT < breakD)
		{
			breakT++;
		}
		base.Update();
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!(other is FragmentEntity) && !other.Flying)
		{
			return IsBroken;
		}
		return true;
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z, IsBroken].DrawSpriteW(_(SpriteName.firewall_base), base.WorldCenter.Shift(0f, 0f - (IsBroken ? 0f : (0.7f * Component._sin((float)base.Age * 0.04f)))), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[base.Z].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter.Shift(0f, -3f), null, IsBroken ? (Vector2.One * (1f - (float)breakT / (float)breakD)) : (new Vector2(1f + 0.1f * Component._sin((float)base.Age * 0.04f), 1f + 0.1f * Component._cos((float)base.Age * 0.04f)) * 0.75f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		if (!IsBroken)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.firewall_base), base.WorldCenter.Shift(0f, -7f + 0.7f * Component._sin((float)base.Age * 0.04f)), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.Draw();
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		foreach (List<FireballEntity> ray in rays)
		{
			foreach (FireballEntity item in ray)
			{
				item.Break(this);
			}
		}
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.firewall_base), null));
		SendMessage(new PlayWorldSoundMessage(SoundName.rotoblade_break, base.WorldCenter));
		base.Break(offender);
	}
}
