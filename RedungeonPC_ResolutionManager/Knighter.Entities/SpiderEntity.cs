using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class SpiderEntity : Entity
{
	private Animation anim;

	private Sprite eyes;

	private string program;

	private int delay;

	private int defaultDelay;

	private int jumpTime;

	private List<int> xShifts;

	private List<int> yShifts;

	private List<int> delays;

	private int cycle;

	private Vector2 spawn;

	private float jumpPos;

	private bool jumping;

	private List<Vector2> trailNodes;

	private int trailLength = 10;

	public SpiderEntity(int x, int y, TileDesc desc)
		: base((float)x + 0.25f, (float)y + 0.25f, 0.5f, 0.5f)
	{
		xShifts = new List<int>();
		yShifts = new List<int>();
		delays = new List<int>();
		spawn = new Vector2((float)x + 0.25f, (float)y + 0.25f);
		Init(desc.Str("program"), desc["delay"], desc["default-delay"], desc["jump-time"], desc.Flipped);
		anim = new Animation(0.15f);
		anim.Add("crawl", "spider_", "1234");
		anim.Play("crawl");
		eyes = _(SpriteName.spider_eyes);
		trailNodes = new List<Vector2>();
	}

	public override void Load()
	{
		base.Load();
	}

	private void Init(string program, int delay, int defaultDelay, int jumpTime, bool flipped = false)
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
		ReadProgram();
	}

	private void ReadProgram()
	{
		string text = program;
		xShifts.Clear();
		yShifts.Clear();
		delays.Clear();
		cycle = 0;
		int num = 0;
		int num2 = 0;
		int i = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		bool flag = false;
		for (; i < text.Length; i++)
		{
			char c = text[i];
			if (c == 'n' || c == 'e' || c == 's' || c == 'w')
			{
				switch (c)
				{
				case 'n':
					num4--;
					break;
				case 'e':
					num3++;
					break;
				case 's':
					num4++;
					break;
				case 'w':
					num3--;
					break;
				}
			}
			if (c == '.')
			{
				int num6 = (flag ? num5 : defaultDelay);
				xShifts.Add(num3);
				yShifts.Add(num4);
				delays.Add(num6);
				cycle += num6 + jumpTime;
				num += num3;
				num2 += num4;
				num3 = 0;
				num4 = 0;
				num5 = 0;
				flag = false;
			}
			if (c >= '0' && c <= '9')
			{
				num5 = num5 * 10 + (c - 48);
				flag = true;
			}
		}
		if (num != 0 || num2 != 0)
		{
			cycle *= 2;
			for (int num7 = xShifts.Count - 1; num7 >= 0; num7--)
			{
				int item = -xShifts[num7];
				int item2 = -yShifts[num7];
				xShifts.Add(item);
				yShifts.Add(item2);
				delays.Add((num7 > 0) ? delays[num7 - 1] : delays[delays.Count - 1]);
			}
		}
	}

	public override void Update()
	{
		int num = (base.worldTicks - delay).Mod(cycle);
		x = spawn.X;
		y = spawn.Y;
		bool flag = jumping;
		jumping = false;
		int num2 = 0;
		bool flag2 = false;
		while (!flag2)
		{
			if (num2 >= xShifts.Count)
			{
				flag2 = true;
				continue;
			}
			int num3 = xShifts[num2];
			int num4 = yShifts[num2];
			int num5 = delays[num2];
			if (num5 == -1)
			{
				num5 = defaultDelay;
			}
			if (num >= jumpTime)
			{
				num -= jumpTime;
				x += num3;
				y += num4;
				jumpPos = 0f;
			}
			else
			{
				flag2 = true;
				x += MathHelper.Lerp(0f, num3, (float)num / (float)jumpTime);
				y += MathHelper.Lerp(0f, num4, (float)num / (float)jumpTime);
				jumpPos = (float)num / (float)jumpTime;
				jumping = true;
			}
			if (!flag2)
			{
				if (num >= num5)
				{
					num -= num5;
				}
				else
				{
					flag2 = true;
				}
			}
			num2++;
		}
		if (flag != jumping)
		{
			SendMessage(new SpawnEntityMessage(new EffectEntity(base.CenterCoordinates, "dust_", "1234"), CurrentPlatform));
		}
		UpdateTiles();
		foreach (Tile occupiedTile in OccupiedTiles)
		{
			EnterTile(occupiedTile);
		}
		anim.Update();
		base.Update();
	}

	public override void Draw()
	{
		float num = 40f * Component._sin(jumpPos * (float)Math.PI);
		int depth = ((num > 0f) ? base.Z : (base.Z + 10));
		Vector2 vector = base.WorldCenter.Shift(0f, 0f - num);
		trailNodes.Add(vector);
		if (trailNodes.Count > trailLength)
		{
			trailNodes.RemoveAt(0);
		}
		Sprite sprite = ((!jumping) ? anim.GetCurrentFrame() : ((jumpPos < 0.3f) ? _(SpriteName.spider_6) : ((jumpPos < 0.4f) ? _(SpriteName.spider_7) : ((jumpPos < 0.6f) ? _(SpriteName.spider_8) : _(SpriteName.spider_9)))));
		base.core.Renderer[depth].DrawSpriteW(sprite, vector, null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer[depth].DrawSpriteW(eyes, vector - sprite.Size * 0.5f + sprite.Link - eyes.Link, Color.Gold);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(sprite, base.WorldCenter.Shift(0f, 5f + num * 0.3f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (!jumping && other is PlayerEntity playerEntity)
		{
			playerEntity.Hurt(InjuryType.Spider, this);
		}
		base.CollideWith(other);
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
