using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BlockerEntity : Entity
{
	private readonly string actions;

	private int level;

	private readonly int trigger;

	private readonly bool flipped;

	private const int StepsPerAction = 10;

	private const float blockerHeight = 11f;

	private int steps;

	private bool started;

	private bool ended;

	private Vector2 drawShift;

	private Color drawTint;

	public BlockerEntity(int x, int y, int level, string action, int trigger, bool flipped)
		: base(x, y, 1f, 1f)
	{
		this.level = level;
		actions = action;
		this.flipped = flipped;
		this.trigger = trigger;
		drawShift.Y = (float)(-level) * 11f;
		drawTint = Color.Lerp(Color.White, Color.Black, 0.3f);
	}

	public override void Load()
	{
		Subscribe(MessageType.ButtonTrigger);
		base.Load();
	}

	public override void Unload()
	{
		Unsubscribe(MessageType.ButtonTrigger);
		base.Unload();
	}

	private void Step()
	{
		int num = steps / 10;
		if (num >= actions.Length)
		{
			End();
			return;
		}
		bool flag = steps % 10 == 0;
		bool flag2 = steps % 10 == 9;
		switch (actions[num])
		{
		case 'n':
			y -= 0.1f;
			break;
		case 'e':
			x += 0.1f * (float)((!flipped) ? 1 : (-1));
			break;
		case 'w':
			x -= 0.1f * (float)((!flipped) ? 1 : (-1));
			break;
		case 's':
			y += 0.1f;
			break;
		case 'd':
		case 'u':
			drawShift.Y += 1.1f * ((actions[num] == 'u') ? (-1f) : 1f);
			if (flag)
			{
				level += ((actions[num] == 'u') ? 1 : (-1));
			}
			if ((level == -1 && actions[num] == 'd') || (level == 0 && actions[num] == 'u'))
			{
				float num2 = (float)(steps % 10) / 10f;
				if (actions[num] == 'u')
				{
					num2 = 1f - num2;
				}
				drawTint = Color.Lerp(Color.White, Color.Black, 0.3f * num2);
			}
			break;
		}
		if (flag2)
		{
			x = (float)Math.Round(x);
			y = (float)Math.Round(y);
			UpdateTiles();
		}
		steps++;
	}

	public override void Update()
	{
		if (!started && base.Tile.Type == TileType.Pit && level == -1)
		{
			base.Tile.Type = TileType.Blocker;
		}
		if (started && !ended)
		{
			Step();
		}
		base.Update();
	}

	public override void Draw()
	{
		if (level < 0)
		{
			base.R["bg", (base.Tile.Y + (int)base.Tile.Map.Y) * 16, false].DrawSpriteW(_(SpriteName.blocker), base.WorldPosition.Shift(-1f, -11f) + drawShift, drawTint);
		}
		else
		{
			base.R[base.Z].DrawSpriteW(_(SpriteName.blocker), base.WorldPosition.Shift(-1f, -11f) + drawShift);
		}
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return level < 0;
	}

	public override void OnMessage(Message message, object sender)
	{
		if (message is ButtonTriggerMessage buttonTriggerMessage)
		{
			int num = base.core.CurrentPlayState.LevelGenerator.FindGeneratedModuleIndex((int)base.WorldCoordinates.Y);
			if (buttonTriggerMessage.Id == trigger && buttonTriggerMessage.ModuleIndex == num && !started)
			{
				Start();
			}
		}
		base.OnMessage(message, sender);
	}

	private void Start()
	{
		started = true;
		if (base.Tile.Type == TileType.Blocker)
		{
			base.Tile.Type = TileType.Pit;
		}
	}

	private void End()
	{
		ended = true;
		base.Tile.Type = TileType.Blocker;
	}
}
