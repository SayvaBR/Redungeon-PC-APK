using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Helpers;
using Knighter.Messages;

namespace Knighter.Entities;

public class ItemRailEntity : Entity
{
	private class RailCmd
	{
		public int type;

		public int pause;

		public float dx;

		public float dy;

		public int quarters;

		public int dir;

		public float r;

		public float cdx;

		public float cdy;

		public float sx;

		public float sy;

		public int duration;
	}

	private List<RailCmd> commands;

	private List<Pair<int, Entity>> items;

	private List<Pair<int, Entity>> toRemove;

	private string itemsS;

	private string path;

	private int delay;

	private int ticksPerTile;

	private int pauseDuration;

	private int cycle;

	private float offx;

	private float offy;

	private bool valid;

	private int moduleNumber;

	private int moduleGroup;

	public ItemRailEntity(int x, int y, TileDesc desc, int moduleNumber)
		: base((float)x + 0.5f, (float)y + 0.5f, 0f, 0f)
	{
		this.moduleNumber = moduleNumber;
		moduleGroup = desc.ParentModule.Group;
		Init(desc.Str("path"), desc.Str("items"), desc["delay"], desc["ticks-per-tile"], desc["pause-duration"], desc.Flipped);
	}

	private void Init(string path, string items, int delay, int ticksPerTile, int pauseDuration, bool flipped)
	{
		if (flipped)
		{
			path = path.Replace("e", "!");
			path = path.Replace("w", "e");
			path = path.Replace("!", "w");
			path = path.Replace("<", "!");
			path = path.Replace(">", "<");
			path = path.Replace("!", ">");
		}
		this.path = path;
		this.delay = delay;
		this.ticksPerTile = ticksPerTile;
		this.pauseDuration = pauseDuration;
		itemsS = items;
		commands = new List<RailCmd>();
		ReadProgram();
	}

	public override void Load()
	{
		SpawnItems();
		base.Load();
	}

	private void SpawnItems()
	{
		itemsS = ReadItems(itemsS);
		float num = (float)cycle / (float)itemsS.Length;
		int num2 = -1;
		items = new List<Pair<int, Entity>>();
		toRemove = new List<Pair<int, Entity>>();
		int num3 = 0;
		SerpentEntity lastSerpent = null;
		string text = itemsS;
		foreach (char c in text)
		{
			num2++;
			Entity entity = null;
			switch (c)
			{
			case '0':
				entity = base.core.CurrentPlayState.LevelGenerator.MakeLoot(0, 0);
				break;
			case '1':
				entity = new ItemEntity(0f, 0f, ItemType.GoldCoin);
				break;
			case '2':
				entity = new ItemEntity(0f, 0f, ItemType.GreenCoin);
				break;
			case '3':
				entity = new ItemEntity(0f, 0f, ItemType.BlueCoin);
				break;
			case '4':
				entity = new ItemEntity(0f, 0f, ItemType.RedCoin);
				break;
			case 'b':
				entity = new BatEntity(0, 0, null);
				break;
			case 'f':
			case 'g':
			case 'x':
			case 'z':
			{
				bool flag = c == 'f' || (c == 'g' && moduleNumber % 2 == 1) || (c == 'x' && moduleNumber % 2 == 0);
				entity = new FireballEntity(this, 0f, 0f, (!flag) ? BallType.Zap : BallType.Fire);
				break;
			}
			case 's':
				entity = new SerpentEntity(this, 0f, 0f, num3, moduleGroup == 12);
				(entity as SerpentEntity).Prev = lastSerpent;
				if (lastSerpent != null)
				{
					lastSerpent.Next = entity as SerpentEntity;
				}
				lastSerpent = entity as SerpentEntity;
				num3++;
				break;
			}
			items.Add(new Pair<int, Entity>((int)((float)num2 * num), entity));
			if (entity != null)
			{
				entity.SetFlying(value: true);
				SendMessage(new SpawnEntityMessage(entity, CurrentPlatform));
			}
			if (c == 's')
			{
				entity = new SerpentEntity(this, 0f, 0f, num3, moduleGroup == 12);
				(entity as SerpentEntity).Prev = lastSerpent;
				if (lastSerpent != null)
				{
					lastSerpent.Next = entity as SerpentEntity;
				}
				float num4 = 0.5f;
				if (itemsS.Length <= num2 + 1 || itemsS[num2 + 1] != 's')
				{
					(entity as SerpentEntity).Part = SerpentEntity.SerpentPart.Tail2;
					if ((entity as SerpentEntity).Prev != null && (entity as SerpentEntity).Prev.Part != SerpentEntity.SerpentPart.Head)
					{
						(entity as SerpentEntity).Prev.Part = SerpentEntity.SerpentPart.Tail1;
					}
					num4 = 0.3f;
					items.Find((Pair<int, Entity> pair) => pair.B == lastSerpent).A -= (int)(0.1f * num);
				}
				lastSerpent = entity as SerpentEntity;
				items.Add(new Pair<int, Entity>((int)(((float)num2 + num4) * num), entity));
				entity.SetFlying(value: true);
				SendMessage(new SpawnEntityMessage(entity, CurrentPlatform));
				num3++;
			}
			if (c != 's')
			{
				num3 = 0;
				lastSerpent = null;
			}
		}
	}

	private string ReadItems(string items)
	{
		string text = "";
		if (items.Length < 1)
		{
			return text;
		}
		while (items.Length > 0)
		{
			char c = items[0];
			items = items.Substring(1);
			if (c == '~')
			{
				int num = ReadInt(ref items);
				if (num == 0)
				{
					num = 1;
				}
				char c2 = items[0];
				items = items.Substring(1);
				for (int i = 0; i < num; i++)
				{
					text += c2;
				}
			}
			else
			{
				text += c;
			}
		}
		return text;
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

	private void ReadProgram()
	{
		commands.Clear();
		cycle = 0;
		valid = false;
		float num = 0f;
		float num2 = 0f;
		if (path.Length == 0)
		{
			return;
		}
		string[] array = path.Split(',');
		foreach (string obj in array)
		{
			RailCmd railCmd = new RailCmd();
			string text = obj;
			char c = text[0];
			text = text.Substring(1);
			switch (c)
			{
			case '-':
				railCmd.type = 0;
				break;
			case 'e':
			case 'n':
			case 's':
			case 'w':
			{
				railCmd.type = 1;
				int num5 = ReadInt(ref text);
				if (c == 'n')
				{
					railCmd.dy = -num5;
				}
				if (c == 's')
				{
					railCmd.dy = num5;
				}
				if (c == 'e')
				{
					railCmd.dx = num5;
				}
				if (c == 'w')
				{
					railCmd.dx = -num5;
				}
				if (text.Length > 0)
				{
					c = text[0];
					text = text.Substring(1);
					num5 = ReadInt(ref text);
					if (c == 'n')
					{
						railCmd.dy = -num5;
					}
					if (c == 's')
					{
						railCmd.dy = num5;
					}
					if (c == 'e')
					{
						railCmd.dx = num5;
					}
					if (c == 'w')
					{
						railCmd.dx = -num5;
					}
				}
				railCmd.sx = railCmd.dx;
				railCmd.sy = railCmd.dy;
				break;
			}
			case 'a':
			{
				railCmd.type = 2;
				int num3 = ReadInt(ref text);
				railCmd.r = ((num3 > 0) ? ((float)num3) : 0.5f);
				char num4 = text[0];
				text = text.Substring(1);
				if (num4 == 'n')
				{
					railCmd.dy = -1f;
				}
				if (num4 == 's')
				{
					railCmd.dy = 1f;
				}
				if (num4 == 'e')
				{
					railCmd.dx = 1f;
				}
				if (num4 == 'w')
				{
					railCmd.dx = -1f;
				}
				railCmd.dir = ((text[0] != '<') ? 1 : (-1));
				text = text.Substring(1);
				railCmd.quarters = ReadInt(ref text);
				railCmd.cdx = ((railCmd.dy == 0f) ? 0f : ((0f - railCmd.dy) * (float)railCmd.dir * railCmd.r));
				railCmd.cdy = ((railCmd.dx == 0f) ? 0f : (railCmd.dx * (float)railCmd.dir * railCmd.r));
				if (railCmd.quarters == 1)
				{
					railCmd.sx = railCmd.r * (railCmd.dx - railCmd.dy * (float)railCmd.dir);
					railCmd.sy = railCmd.r * (railCmd.dy + railCmd.dx * (float)railCmd.dir);
				}
				else if (railCmd.quarters == 2)
				{
					railCmd.sx = ((railCmd.dx != 0f) ? 0f : ((0f - railCmd.r) * 2f * (float)railCmd.dir * railCmd.dy));
					railCmd.sy = ((railCmd.dy != 0f) ? 0f : (railCmd.r * 2f * (float)railCmd.dir * railCmd.dx));
				}
				else if (railCmd.quarters == 3)
				{
					railCmd.sx = railCmd.r * (0f - railCmd.dx - railCmd.dy * (float)railCmd.dir);
					railCmd.sy = railCmd.r * (0f - railCmd.dy + railCmd.dx * (float)railCmd.dir);
				}
				break;
			}
			}
			num += railCmd.sx;
			num2 += railCmd.sy;
			railCmd.duration = CommandDuration(railCmd);
			commands.Add(railCmd);
			cycle += railCmd.duration;
		}
		valid = num == 0f && num2 == 0f;
		offx = num;
		offy = num2;
	}

	private int CommandDuration(RailCmd cmd)
	{
		return cmd.type switch
		{
			0 => pauseDuration, 
			1 => (int)(Math.Sqrt(cmd.dx * cmd.dx + cmd.dy * cmd.dy) * (double)ticksPerTile), 
			2 => (int)((double)(3f * cmd.r) * 0.5 * (double)cmd.quarters * (double)ticksPerTile), 
			_ => 0, 
		};
	}

	public override void Update()
	{
		foreach (Pair<int, Entity> item in items)
		{
			int a = item.A;
			Entity b = item.B;
			if (b == null)
			{
				continue;
			}
			if (b.IsBroken || b.Unloaded)
			{
				toRemove.Add(item);
			}
			else if ((!(b is ItemEntity) || !((ItemEntity)b).HasTarget) && (!(b is BatEntity) || !((BatEntity)b).Fleeing))
			{
				int num = (base.worldTicks - (delay + a)).Mod(cycle);
				float num2 = x;
				float num3 = y;
				int num4 = 0;
				RailCmd railCmd = commands[num4];
				while (railCmd.duration < num)
				{
					num -= railCmd.duration;
					num2 += railCmd.sx;
					num3 += railCmd.sy;
					num4++;
					railCmd = commands[num4];
				}
				switch (railCmd.type)
				{
				case 1:
					num2 += railCmd.sx * (float)num / (float)railCmd.duration;
					num3 += railCmd.sy * (float)num / (float)railCmd.duration;
					break;
				case 2:
				{
					float num5 = (float)((railCmd.cdx < 0f) ? 0.0 : ((railCmd.cdy < 0f) ? 4.71238898038469 : ((railCmd.cdx > 0f) ? Math.PI : (Math.PI / 2.0))));
					float num6 = (float)((double)num5 + (double)(railCmd.dir * railCmd.quarters) * Math.PI / 2.0);
					float num7 = 0f - num5 + (num6 - num5) * (float)num / (float)railCmd.duration;
					num2 += railCmd.cdx + Component._cos(num7) * railCmd.r;
					num3 += railCmd.cdy + Component._sin(num7) * railCmd.r;
					break;
				}
				}
				b.UpdatePosition(num2, num3);
			}
		}
		foreach (Pair<int, Entity> item2 in toRemove)
		{
			items.Remove(item2);
		}
		toRemove.Clear();
		base.Update();
	}
}
