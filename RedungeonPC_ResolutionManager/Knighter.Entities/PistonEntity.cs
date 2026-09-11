using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class PistonEntity : Entity
{
	private Sprite rod;

	private Sprite pad;

	private Sprite rodEdge;

	private SpriteName padName;

	private int delay;

	private int direction;

	private int distance;

	private int addHalf;

	private int ticksPerTile1;

	private int ticksPerTile2;

	private int stopTime1;

	private int stopTime2;

	private float baseX;

	private float baseY;

	private int dx;

	private int dy;

	private float far;

	private int sinceLastSlam = 60;

	private int slamFrame;

	public bool Unbreakable { get; private set; }

	public bool Extended { get; private set; }

	public PistonEntity(int x, int y, TileDesc desc)
		: base(x, y, 0.5f, 0.5f)
	{
		Init(desc["delay"], desc["dir"], desc["distance"], desc["add-half"], desc["ticks-per-tile-1"], desc["ticks-per-tile-2"], desc["stop-time-1"], desc["stop-time-2"], desc["unbreakable"], desc.Flipped);
		baseX = x;
		baseY = y;
		padding = 0.3f;
	}

	private void Init(int delay, int direction, int distance, int addHalf, int ticksPerTile1, int ticksPerTile2, int stopTime1, int stopTime2, int unbreakable, bool flipped = false)
	{
		this.delay = delay;
		this.direction = direction;
		if (flipped)
		{
			this.direction = (4 - direction).Mod(4);
		}
		this.distance = distance;
		this.addHalf = addHalf;
		this.ticksPerTile1 = ticksPerTile1;
		this.ticksPerTile2 = ticksPerTile2;
		this.stopTime1 = stopTime1;
		this.stopTime2 = stopTime2;
		Unbreakable = unbreakable == 1;
		dx = 0;
		dy = 0;
		padName = SpriteName.font_bold;
		SpriteName name = SpriteName.font_bold;
		switch ((Direction)this.direction)
		{
		case Direction.North:
			padName = SpriteName.piston_pad_n;
			name = SpriteName.piston_rod_ns;
			dy = -1;
			break;
		case Direction.South:
			padName = SpriteName.piston_pad_s;
			name = SpriteName.piston_rod_ns;
			dy = 1;
			break;
		case Direction.East:
			padName = SpriteName.piston_pad_e;
			name = SpriteName.piston_rod_ew;
			dx = 1;
			break;
		case Direction.West:
			padName = SpriteName.piston_pad_w;
			name = SpriteName.piston_rod_ew;
			dx = -1;
			break;
		}
		pad = base.core.SpriteManager.GetSprite(padName);
		rod = base.core.SpriteManager.GetSprite(name);
		rodEdge = base.core.SpriteManager.GetSprite(SpriteName.piston_rod_edge);
		SendMessage(new SpawnEntityMessage(new PistonCoreEntity((int)x - dx, (int)y - dy, this), CurrentPlatform));
	}

	public override void Break(Entity offender)
	{
		if (!Unbreakable)
		{
			IsBroken = true;
			base.core.ParticleManager.AddSmoke(base.WorldCenter, base.Z);
			SendMessage(new PlayWorldSoundMessage(SoundName.piston_break, base.WorldPosition));
			SpriteName spriteName = ((dx != 0) ? SpriteName.piston_segment_ew : SpriteName.piston_segment_ns);
			for (int i = 0; (float)i < width; i++)
			{
				SendMessage(new SpawnEntityMessage(new FragmentEntity(new Vector2(baseX + (float)(i * dx), baseY + (float)(i * dy) + 0.3f), spriteName, 70, new Vector4(0f, 0f, Component._rnd(1, 2), Component._rnd(-0.05f, 0.05f))), null));
			}
			SendMessage(new SpawnEntityMessage(new FragmentEntity(new Vector2(baseX + width * (float)dx, baseY + height * (float)dy), padName, 70, new Vector4(0f, 0f, Component._rnd(1, 2), Component._rnd(-0.05f, 0.05f))), null));
			_inc(Stat.PistonsBroken);
			base.Break(offender);
		}
	}

	private float PosFromTime(int time)
	{
		far = (float)distance + (float)addHalf * 0.5f;
		float num = 0f;
		switch ((Direction)direction)
		{
		case Direction.East:
			num = 1f;
			break;
		case Direction.West:
			num = 4f;
			far += 0.3f;
			break;
		case Direction.South:
			num = 1f;
			break;
		case Direction.North:
			num = 3f;
			break;
		}
		if (distance != 0)
		{
			far -= num / 16f;
		}
		int num2 = stopTime1;
		if (time < num2)
		{
			return 0f;
		}
		time -= num2;
		num2 = distance * ticksPerTile1;
		if (time < num2)
		{
			return MathHelper.Lerp(0f, far, (float)time / (float)num2);
		}
		time -= num2;
		num2 = stopTime2;
		if (time < num2)
		{
			return far;
		}
		time -= num2;
		num2 = distance * ticksPerTile2;
		return MathHelper.Lerp(far, 0f, (float)time / (float)num2);
	}

	public override void Update()
	{
		int num = stopTime1 + stopTime2 + distance * ticksPerTile1 + distance * ticksPerTile2;
		int num2 = (base.worldTicks - delay).Mod(num);
		Extended = num2 >= stopTime1 + distance * ticksPerTile1 && num2 <= stopTime1 + distance * ticksPerTile1 + stopTime2;
		if (!IsBroken)
		{
			if (num2 == stopTime1)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.piston_extend, base.WorldCenter, 0.6f));
			}
			if (num2 == stopTime1 + distance * ticksPerTile1 + stopTime2)
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.piston_retract, base.WorldCenter, 0.3f));
			}
		}
		if (slamFrame > 0)
		{
			slamFrame++;
			if (slamFrame == 5)
			{
				slamFrame = 0;
			}
		}
		sinceLastSlam++;
		if (sinceLastSlam > 30 && !IsBroken && num2 == stopTime1 + distance * ticksPerTile1)
		{
			Vector2 vector = new Vector2(baseX, baseY);
			int num3 = distance + addHalf;
			switch ((Direction)direction)
			{
			case Direction.East:
				vector.X += num3;
				break;
			case Direction.West:
				vector.X -= num3;
				break;
			case Direction.South:
				vector.Y += num3;
				break;
			case Direction.North:
				vector.Y -= num3;
				break;
			}
			Tile tile = base.core.CurrentPlayState.TileMap[Convert.ToInt32(vector.X), Convert.ToInt32(vector.Y)];
			if (tile != null)
			{
				Entity entity = tile.Entities.Find((Entity e) => e is WallEntity || e is PistonEntity);
				bool flag = false;
				bool flag2 = false;
				if (entity is WallEntity)
				{
					flag = true;
					flag2 = true;
				}
				if (entity is PistonEntity)
				{
					PistonEntity pistonEntity = entity as PistonEntity;
					flag = pistonEntity.addHalf == addHalf && !pistonEntity.IsBroken;
				}
				if (flag)
				{
					sinceLastSlam = 0;
					float num4 = Component._m(Component._M(1.2f - (float)ticksPerTile1 / 10f, 0f), 1f);
					if (num4 > 0.4f)
					{
						slamFrame = 1;
					}
					if (flag2 || direction == 1 || direction == 2)
					{
						SendMessage(new PlayWorldSoundMessage(SoundName.piston_slam, vector * 16f, num4));
					}
					base.core.CurrentPlayState.Camera.Shake("piston slam", 3f * base.core.AudioManager.VolumeInWorld(base.WorldCenter) * num4);
				}
			}
		}
		float num5 = PosFromTime(num2);
		switch ((Direction)direction)
		{
		case Direction.East:
			width = num5;
			x = baseX;
			break;
		case Direction.West:
			width = num5;
			x = baseX + 1f - num5 + (width.IsZero() ? 0.1f : 0f);
			break;
		case Direction.North:
			height = num5;
			y = baseY + 1f - num5;
			break;
		case Direction.South:
			height = num5;
			y = baseY;
			break;
		}
		if (width.IsZero())
		{
			width = 0.1f;
		}
		if (height.IsZero())
		{
			height = 0.1f;
		}
		if (!IsBroken)
		{
			UpdateTiles();
		}
		else
		{
			x = baseX;
			y = baseY;
		}
		base.Update();
	}

	public override void Draw()
	{
		switch ((Direction)direction)
		{
		case Direction.East:
			if (!IsBroken)
			{
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(0f, -1f), null, new Vector2(width * 16f, 1f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(pad, base.WorldPosition + new Vector2((float)(1 - pad.Width) + width * 16f, -11f));
				if (slamFrame > 0)
				{
					base.core.Renderer[base.Z + 1].DrawSpriteW(_("piston_slam_h" + ((slamFrame < 3) ? 1 : 2)), base.WorldPosition.Shift(-4f, -5f) + new Vector2((float)(1 - pad.Width) + width * 16f, -11f));
				}
			}
			else
			{
				float num2 = Component._m(width, 0.2f) * 16f - 1f;
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(0f, -1f), null, new Vector2(num2, 1f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(_(SpriteName.piston_rod_сut_ew), base.WorldPosition + new Vector2(num2, -1f));
			}
			break;
		case Direction.West:
			if (!IsBroken)
			{
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(0f, -1f), null, new Vector2(width * 16f, 1f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(pad, base.WorldPosition + new Vector2((float)(2 - pad.Width + -1) + 6f * (width / far), -11f));
				if (slamFrame > 0)
				{
					base.core.Renderer[base.Z + 1].DrawSpriteW(_("piston_slam_h" + ((slamFrame < 3) ? 1 : 2)), base.WorldPosition.Shift(0f, -5f) + new Vector2((float)(2 - pad.Width + -1) + 6f * (width / far), -11f), null, null, 0f, SpriteFlip.Horizontal);
				}
			}
			else
			{
				float num4 = Component._m(width, 0.2f) * 16f + 1f;
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(16f - num4, -1f), null, new Vector2(num4 + 2f, 1f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(_(SpriteName.piston_rod_сut_ew), base.WorldPosition + new Vector2(16f - num4 - 1f, -1f), null, null, 0f, SpriteFlip.Horizontal);
			}
			break;
		case Direction.South:
			if (!IsBroken)
			{
				base.core.Renderer[base.Z].DrawSpriteW(rodEdge, base.WorldPosition + new Vector2(4f, -8f));
				base.core.Renderer[base.Z].DrawSpriteW(rod, base.WorldPosition + new Vector2(4f, -5f), null, new Vector2(1f, height * 16f));
				base.core.Renderer[base.Z].DrawSpriteW(pad, base.WorldPosition + new Vector2(-1f, -12f + height * 16f));
				if (slamFrame > 0)
				{
					base.core.Renderer[base.Z + 1].DrawSpriteW(_("piston_slam_v" + ((slamFrame < 3) ? 1 : 2)), base.WorldPosition.Shift(-4f, -3f) + new Vector2(-1f, -12f + height * 16f), null, null, 0f, SpriteFlip.Horizontal);
				}
			}
			else
			{
				float num3 = Component._m(height, 0.2f) * 16f - 1f;
				base.core.Renderer[base.Z].DrawSpriteW(rodEdge, base.WorldPosition + new Vector2(4f, -8f));
				base.core.Renderer[base.Z].DrawSpriteW(rod, base.WorldPosition + new Vector2(4f, -5f), null, new Vector2(1f, num3 + 2f));
				base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.piston_rod_сut_ns), base.WorldPosition + new Vector2(4f, -7f + num3));
			}
			break;
		case Direction.North:
			if (!IsBroken)
			{
				base.core.Renderer[base.Z + 1].DrawSpriteW(pad, base.WorldPosition + new Vector2(-1f, -13f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(rodEdge, base.WorldPosition + new Vector2(4f, -6f));
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(4f, -4f), null, new Vector2(1f, height * 16f));
				if (slamFrame > 0)
				{
					base.core.Renderer[base.Z + 1].DrawSpriteW(_("piston_slam_v" + ((slamFrame < 3) ? 1 : 2)), base.WorldPosition.Shift(-4f, -6f) + new Vector2(-1f, -13f), null, null, 0f, SpriteFlip.Horizontal);
				}
			}
			else
			{
				float num = Component._m(height, 0.2f) * 16f + 1f;
				base.core.Renderer[base.Z + 1].DrawSpriteW(rodEdge, base.WorldPosition + new Vector2(4f, 5f - num));
				base.core.Renderer[base.Z + 1].DrawSpriteW(rod, base.WorldPosition + new Vector2(4f, 7f - num), null, new Vector2(1f, num));
			}
			break;
		}
		base.Draw();
	}

	public override void OnEnterTile(Tile tile)
	{
		if (IsBroken)
		{
			return;
		}
		foreach (Entity item in tile.Entities.FindAll((Entity e) => e is IPushableEntity && (!e.Flying || !e.FlightIgnoresObstacles)))
		{
			if (!item.TryMoveToCoordinates(item.CurrentMap, item.Coordinates + new Vector2(dx, dy)))
			{
				(item as IPushableEntity).Crash(this);
			}
		}
		base.OnEnterTile(tile);
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!IsBroken)
		{
			if (!(other is PlayerEntity) && !(other is FollowerEntity))
			{
				return !(other is BoxEntity);
			}
			return false;
		}
		return true;
	}
}
