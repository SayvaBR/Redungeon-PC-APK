using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class RotobladeEntity : Entity
{
	private Sprite baseSprite;

	private Sprite topSprite;

	private Sprite swordSprite0;

	private Sprite swordSprite1;

	private Sprite swordSprite2;

	private Sprite swordSprite3;

	private bool shardLeft;

	private string program;

	private int delay;

	private int startDir;

	private int defaultDelay;

	private int turnTime;

	private List<int> sequence;

	private int cycle;

	private float angle;

	private float? blurAngle;

	private Vector2 centerCoordinates;

	private Vector2 center;

	private bool isStill;

	private int baseZ;

	private int ticksSinceSound = 60;

	public RotobladeEntity(int x, int y, TileDesc desc)
		: base(x, y, 0.2f, 0.2f)
	{
		sequence = new List<int>();
		centerCoordinates = new Vector2(x, y);
		center = centerCoordinates.Clone();
		baseZ = base.Z;
		Init(desc.Str("program"), desc["delay"], desc["start-dir"], desc["default-delay"], desc["turn-time"], desc.Flipped);
		baseSprite = _(SpriteName.rotoblade_base);
		topSprite = _(SpriteName.rotoblade_top);
		swordSprite0 = _(SpriteName.rotoblade_blade_0);
		swordSprite1 = _(SpriteName.rotoblade_blade_1);
		swordSprite2 = _(SpriteName.rotoblade_blade_2);
		swordSprite3 = _(SpriteName.rotoblade_blade_3);
	}

	public override void Load()
	{
		ObstacleEntity obstacleEntity = new ObstacleEntity((int)x, (int)y, invisible: true);
		SendMessage(new SpawnEntityMessage(obstacleEntity, CurrentPlatform));
		obstacleEntity.Host = this;
		base.Load();
	}

	private void Init(string program, int delay, int startDir, int defaultDelay, int turnTime, bool flipped = false)
	{
		if (flipped)
		{
			program = program.Replace("+", "!");
			program = program.Replace("-", "+");
			program = program.Replace("!", "-");
			switch (startDir)
			{
			case 0:
				startDir = 2;
				break;
			case 2:
				startDir = 0;
				break;
			}
		}
		this.program = program;
		this.startDir = startDir;
		this.delay = delay;
		this.defaultDelay = defaultDelay;
		this.turnTime = turnTime;
		ReadProgram();
	}

	private void ReadProgram()
	{
		string text = program;
		isStill = program == "";
		if (isStill)
		{
			return;
		}
		sequence.Clear();
		cycle = 0;
		int num = 0;
		int i = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		for (; i < text.Length; i++)
		{
			char c = text[i];
			if (c == '-' || c == '+')
			{
				if (num2 != 0)
				{
					int num4 = (flag ? num3 : defaultDelay);
					sequence.Add(num2 * num4);
					cycle += num4 + turnTime;
					num += num2;
					num3 = 0;
					flag = false;
				}
				switch (c)
				{
				case '-':
					num2 = -1;
					break;
				case '+':
					num2 = 1;
					break;
				}
			}
			if (c >= '0' && c <= '9')
			{
				num3 = num3 * 10 + (c - 48);
				flag = true;
			}
		}
		if (num2 != 0)
		{
			int num5 = (flag ? num3 : defaultDelay);
			sequence.Add(num2 * num5);
			cycle += num5 + turnTime;
			num += num2;
		}
		num = Math.Abs(num);
		switch (num.Mod(4))
		{
		case 2:
			cycle *= 2;
			break;
		case 1:
		case 3:
			cycle *= 4;
			break;
		}
	}

	public override void Update()
	{
		ticksSinceSound++;
		center = centerCoordinates.Shift(0.5f, 0.5f);
		if (CurrentPlatform != null)
		{
			center += CurrentPlatform.Coordinates;
		}
		center *= 16f;
		if (!isStill)
		{
			int num = base.worldTicks;
			num -= delay;
			if (cycle != 0)
			{
				num = num.Mod(cycle);
			}
			int num2 = startDir;
			int num3 = 0;
			bool flag = false;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			while (!flag)
			{
				num5 = sequence[num3];
				num6 = ((num5 > 0) ? 1 : (-1));
				num5 = Math.Abs(num5);
				flag = num < turnTime;
				if (!flag)
				{
					num -= turnTime;
					num2 += num6;
					flag = num < num5;
					if (!flag)
					{
						num -= num5;
					}
				}
				else
				{
					num4 = num6;
				}
				if (!flag)
				{
					num3++;
					if (num3 >= sequence.Count)
					{
						num3 = 0;
					}
				}
			}
			num2 %= 4;
			angle = (float)num2 * (float)Math.PI / 2f;
			if (num4 != 0)
			{
				angle += (float)num4 * MathHelper.Lerp(0f, (float)Math.PI / 2f, (float)num / (float)turnTime);
			}
			if (num4 != 0 && num > 0)
			{
				blurAngle = angle - (float)num4 * (float)Math.PI / 12f;
				int num7 = num3 - 1;
				if (num7 < 0)
				{
					num7 = sequence.Count - 1;
				}
				int num8 = sequence[num7];
				int num9 = ((num8 > 0) ? 1 : (-1));
				num8 = Math.Abs(num8);
				if (!IsBroken && (num8 > 5 || num9 != num6) && num == 1 && ticksSinceSound > 5)
				{
					SendMessage(new PlayWorldSoundMessage(SoundName.rotoblade, base.WorldCenter, 0.8f));
					ticksSinceSound = 0;
				}
			}
			else
			{
				blurAngle = null;
			}
		}
		else
		{
			angle = (float)startDir * (float)Math.PI / 2f;
		}
		while (angle < 0f)
		{
			angle += (float)Math.PI * 2f;
		}
		while ((double)angle > Math.PI * 2.0)
		{
			angle -= (float)Math.PI * 2f;
		}
		x = centerCoordinates.X + 0.5f + 1.1f * (float)Math.Cos(angle);
		y = centerCoordinates.Y + 0.5f + 1.1f * (float)Math.Sin(angle);
		UpdateTiles();
		UpdateWorldTilesFromPlatform();
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(baseSprite, center.Shift(0f, 2f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		if (!IsBroken)
		{
			Sprite sprite = swordSprite0;
			if ((double)angle > Math.PI / 6.0)
			{
				sprite = swordSprite1;
			}
			if ((double)angle > Math.PI * 5.0 / 6.0)
			{
				sprite = swordSprite2;
			}
			if ((double)angle > 3.665191429188092)
			{
				sprite = swordSprite3;
			}
			if ((double)angle > 5.759586531581287)
			{
				sprite = swordSprite0;
			}
			var _ = blurAngle.HasValue;
			base.core.Renderer[base.Z].DrawSpriteW(sprite, center.Shift(0f, -5f), null, null, angle, SpriteFlip.None, SpriteOrigin.CenterLeft);
			base.core.Renderer["bg", 2, false].DrawSpriteW(sprite, center.Shift(0f, 0f), Color.Black * 0.2f, null, angle, SpriteFlip.None, SpriteOrigin.CenterLeft);
		}
		else if (shardLeft)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.rotoblade_shard_0), center.Shift(0f, -5f), null, null, angle, SpriteFlip.None, SpriteOrigin.CenterLeft);
		}
		base.core.Renderer[base.Z].DrawSpriteW(topSprite, center.Shift(0f, -1f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (!IsBroken)
		{
			if (other is PlayerEntity playerEntity)
			{
				playerEntity.Hurt(InjuryType.Sword, this);
			}
			base.CollideWith(other);
		}
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		base.core.ParticleManager.AddSmoke(center, base.Z);
		Vector2 v = center / 16f;
		SendMessage(new SpawnEntityMessage(new FragmentEntity(v.Shift(0.5f + 1f * (float)Math.Cos(angle), 0.5f + 1f * (float)Math.Sin(angle)), SpriteName.rotoblade_shard_1, 50, new Vector4(0f)), null));
		SendMessage(new SpawnEntityMessage(new FragmentEntity(v.Shift(0.5f + 1.5f * (float)Math.Cos(angle), 0.5f + 1.5f * (float)Math.Sin(angle)), SpriteName.rotoblade_shard_2, 50, new Vector4(0f)), null));
		SendMessage(new PlayWorldSoundMessage(SoundName.rotoblade_break, base.WorldPosition));
		shardLeft = true;
		_inc(Stat.RotobladesBroken);
		base.Break(offender);
	}
}
