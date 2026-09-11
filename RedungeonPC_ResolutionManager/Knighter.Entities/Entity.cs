using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public abstract class Entity : Component
{
	public class SlideBehavior : Component
	{
		private static BagOf<SoundName> iceSlideSounds;

		private Entity owner;

		private ParticleEmitter iceEmitter;

		public bool SlowLanding;

		public Vector2 slideDir;

		public int slideTimer;

		public int slideDelay = 10;

		public bool Sliding { get; private set; }

		static SlideBehavior()
		{
			iceSlideSounds = new BagOf<SoundName>().Put(SoundName.slide_1).Put(SoundName.slide_2).Put(SoundName.slide_3)
				.Put(SoundName.slide_4);
		}

		public SlideBehavior(Entity owner)
		{
			this.owner = owner;
		}

		public override void Load()
		{
			iceEmitter = base.core.ParticleManager.AddEmitter(inWorld: true, owner.WorldPosition, 3f).AttachTo(owner).OnSpawn(delegate(Particle p)
			{
				p.Velocity = -slideDir.Clone();
				p.Position += owner.dAnim;
			})
				.OnUpdate(delegate(Particle p)
				{
					p.Position += p.Velocity * 0.8f + p.Offset * 0.1f + new Vector2(0f, -0.2f);
					p.Dead = p.Age > 20;
				})
				.OnDraw(delegate(Particle p)
				{
					float num = 1f - (float)p.Age / 20f;
					base.core.Renderer[owner.Z].DrawDotW(p.Position + p.Velocity * 5f, Color.White * num * 0.8f, num * 1f);
				});
			base.Load();
		}

		public void OnEnterTile(Tile tile)
		{
			if (tile.Map.Platform != owner.CurrentPlatform)
			{
				return;
			}
			if (tile.Type == TileType.Ice && !owner.Flying && !SlowLanding)
			{
				_inc(Stat.MetersSlided);
				if (!Sliding)
				{
					SendMessage(new PlaySoundMessage(iceSlideSounds.DrawDifferent()));
				}
				Sliding = true;
				slideDir = owner.LastMovementDir.Clone();
				slideTimer = slideDelay;
				owner.dxTween.Start((0f - slideDir.X) * 0.5f, 0f, slideDelay);
				owner.dyTween.Start((0f - slideDir.Y) * 0.5f, 0f, slideDelay);
				if (tile is DungeonTile dungeonTile && !owner.SlidingDisabled() && !owner.justTeleported)
				{
					dungeonTile.IceTrailN |= slideDir.Y > 0f;
					dungeonTile.IceTrailE |= slideDir.X < 0f;
					dungeonTile.IceTrailW |= slideDir.X > 0f;
					dungeonTile.IceTrailS |= slideDir.Y < 0f;
				}
			}
			else
			{
				Sliding = false;
				if (SlowLanding && !owner.Flying)
				{
					SlowLanding = false;
				}
			}
		}

		public override void Update()
		{
			if (Sliding && !owner.TeleportPending)
			{
				slideTimer--;
				if (slideTimer <= 0)
				{
					DungeonTile dungeonTile = owner.Tile as DungeonTile;
					if (dungeonTile != null && dungeonTile.Type != TileType.Ice)
					{
						dungeonTile = null;
					}
					bool num = owner.TryMoveToCoordinates(owner.CurrentMap, owner.Tile.Coordinates + slideDir);
					if (num)
					{
						owner.dxTween.Start(0f - slideDir.X, 0f, slideDelay);
						owner.dyTween.Start(0f - slideDir.Y, 0f, slideDelay);
						if (dungeonTile != null && !owner.SlidingDisabled())
						{
							dungeonTile.IceTrailN |= slideDir.Y < 0f;
							dungeonTile.IceTrailE |= slideDir.X > 0f;
							dungeonTile.IceTrailW |= slideDir.X < 0f;
							dungeonTile.IceTrailS |= slideDir.Y > 0f;
						}
						iceEmitter.Emit(5, 1, once: false, 3);
					}
					slideTimer = slideDelay;
					if (!num)
					{
						Sliding = false;
					}
				}
			}
			base.Update();
		}
	}

	public SlideBehavior BSlide;

	public readonly List<Tile> OccupiedTiles;

	public readonly List<Tile> OccupiedWorldTiles;

	public PlatformEntity CurrentPlatform;

	protected float padding;

	protected bool gridAligned;

	public Vector2 FlightStart;

	public Vector2? FlightTarget;

	public float FlightStep;

	public int FlightDuration;

	public int CurrentFlightDuration;

	public float FlightAngle;

	public bool FlightIgnoresObstacles;

	protected bool justStoppedFlight;

	public Vector2 LastMovementDir;

	protected float x;

	protected float y;

	protected float width;

	protected float height;

	protected int dAnimDuration = 13;

	protected FloatBox dxAnim;

	protected FloatBox dyAnim;

	protected TweenBox dxTween;

	protected TweenBox dyTween;

	public bool IsBroken;

	protected int teleportTimeout = -1;

	private TileMap teleportMap;

	protected float teleportX;

	protected float teleportY;

	public TeleportEntity SourceTeleport;

	public TeleportEntity DestTeleport;

	protected bool justTeleported;

	public bool CanSlide => BSlide != null;

	protected int worldTicks => base.core.CurrentPlayState.WorldTicks;

	public int Age { get; protected set; }

	protected TileMap levelMap => base.core.CurrentPlayState.TileMap;

	public TileMap CurrentMap
	{
		get
		{
			if (CurrentPlatform != null)
			{
				return CurrentPlatform.Map;
			}
			return levelMap;
		}
	}

	public Tile Tile
	{
		get
		{
			if (OccupiedTiles.Count <= 0)
			{
				return null;
			}
			return OccupiedTiles[0];
		}
	}

	public Tile WorldTile
	{
		get
		{
			if (CurrentPlatform != null)
			{
				if (OccupiedWorldTiles.Count <= 0)
				{
					return null;
				}
				return OccupiedWorldTiles[0];
			}
			return Tile;
		}
	}

	public virtual bool IsPlatform => false;

	public bool Flying { get; private set; }

	public bool FlyingFreely
	{
		get
		{
			if (Flying)
			{
				return !FlightTarget.HasValue;
			}
			return false;
		}
	}

	public Vector2 Origin
	{
		get
		{
			if (IsPlatform)
			{
				return Vector2.Zero;
			}
			if (CurrentPlatform != null)
			{
				return CurrentPlatform.Coordinates;
			}
			return Vector2.Zero;
		}
	}

	public Vector2 Coordinates => new Vector2(x, y);

	public Vector2 WorldCoordinates => Coordinates + Origin;

	public Vector2 Position => Coordinates * 16f;

	public Vector2 WorldPosition => WorldCoordinates * 16f;

	public Vector2 CenterCoordinates => Coordinates + new Vector2(width / 2f, height / 2f);

	public Vector2 WorldCenterCoordinates => WorldCoordinates + new Vector2(width / 2f, height / 2f);

	public Vector2 Center => CenterCoordinates * 16f;

	public Vector2 WorldCenter => WorldPosition + new Vector2(width / 2f, height / 2f) * 16f;

	public int Z => (int)Math.Round(WorldCoordinates.Y) * 16;

	public RectangleF Box => new RectangleF(x, y, width, height);

	public bool Tweening
	{
		get
		{
			if (!dxTween.Running)
			{
				return dyTween.Running;
			}
			return true;
		}
	}

	public Vector2 dAnim => new Vector2((float)dxAnim * 16f, (float)dyAnim * 16f);

	public bool Unloaded { get; private set; }

	public bool TeleportPending => teleportTimeout >= 0;

	public virtual bool SlidingDisabled()
	{
		return false;
	}

	public void InitSlideBehavior()
	{
		BSlide = new SlideBehavior(this);
	}

	protected Entity(float x, float y, float width, float height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
		padding = 0f;
		OccupiedTiles = new List<Tile>();
		OccupiedWorldTiles = new List<Tile>();
		CurrentPlatform = null;
		dxAnim = new FloatBox(0f);
		dyAnim = new FloatBox(0f);
		dxTween = new TweenBox(dxAnim);
		dyTween = new TweenBox(dyAnim);
	}

	public override void Load()
	{
		if (CanSlide)
		{
			BSlide.Load();
		}
		UpdateTiles();
		base.Load();
	}

	public override void Unload()
	{
		RemoveFromTiles();
		Unloaded = true;
		base.Unload();
	}

	public virtual void Pause()
	{
	}

	public virtual void Resume()
	{
	}

	public override void Update()
	{
		if (CanSlide)
		{
			BSlide.Update();
		}
		Age++;
		if (teleportTimeout > 0)
		{
			teleportTimeout--;
			if (teleportTimeout == 0)
			{
				DoTeleport();
				teleportTimeout = -1;
			}
		}
		if (FlightTarget.HasValue && !Flying)
		{
			SetFlying(value: true);
		}
		if (Flying && FlightTarget.HasValue)
		{
			CurrentFlightDuration++;
			float num = (FlightTarget.Value - FlightStart).Length();
			float num2;
			float num3;
			if (FlightDuration == 0)
			{
				num2 = x + (float)Math.Cos((double)FlightAngle - Math.PI / 2.0) * FlightStep;
				num3 = y + (float)Math.Sin((double)FlightAngle - Math.PI / 2.0) * FlightStep;
			}
			else
			{
				Vector2 flightStart = FlightStart;
				Vector2? vector = (FlightTarget - FlightStart) * ((float)CurrentFlightDuration / (float)FlightDuration);
				Vector2 value = (flightStart + vector).Value;
				num2 = value.X;
				num3 = value.Y;
			}
			UpdateTiles();
			float num4 = (new Vector2(num2, num3) - FlightStart).Length();
			bool flag = false;
			flag = ((FlightDuration != 0) ? (CurrentFlightDuration >= FlightDuration) : (num4.IsEqualTo(num) || num4 > num));
			bool flag2 = false;
			if (!FlightIgnoresObstacles)
			{
				flag2 = !TryMoveToCoordinates(CurrentMap, new Vector2(num2, num3));
			}
			else
			{
				flag2 = false;
				x = num2;
				y = num3;
			}
			if (flag || flag2)
			{
				StopFlying();
				OnReachTarget();
			}
		}
		dxTween.Update();
		dyTween.Update();
		justStoppedFlight = false;
	}

	public bool SuspendedStartFlying(float dx, float dy, float step = 0.1f, bool ignoreObstacles = false, bool changeCourse = false, int flightDuration = 0)
	{
		if (Flying && !changeCourse)
		{
			return false;
		}
		FlightStart = WorldCoordinates.Clone();
		FlightTarget = WorldCoordinates.Clone() + new Vector2(dx, dy);
		FlightAngle = (float)Math.Atan2(dx, 0f - dy);
		FlightStep = step;
		FlightDuration = flightDuration;
		FlightIgnoresObstacles = ignoreObstacles;
		CurrentFlightDuration = 0;
		return true;
	}

	protected virtual void StopFlying()
	{
		Flying = false;
		FlightTarget = null;
		justStoppedFlight = true;
		if (gridAligned)
		{
			x = (float)Math.Round(x);
			y = (float)Math.Round(y);
		}
		UpdateTiles();
		SetFlying(value: false);
	}

	protected virtual void OnReachTarget()
	{
	}

	public override void Draw()
	{
		if (Settings.DrawDebugShapes)
		{
			base.core.Renderer["fg"].DrawRectangleW(WorldPosition, width * 16f, height * 16f, Color.Blue * 0.3f);
		}
		base.Draw();
	}

	public void EnterTile(Tile tile)
	{
		if (tile == null)
		{
			return;
		}
		foreach (Entity entity in tile.Entities)
		{
			if (entity != this && (CurrentMap == tile.Map || entity.CurrentMap == tile.Map))
			{
				entity.CollideWith(this);
				CollideWith(entity);
			}
		}
		OnEnterTile(tile);
	}

	public void LeaveTile(Tile tile)
	{
		if (tile == null)
		{
			return;
		}
		foreach (Entity entity in tile.Entities)
		{
			if (entity != this)
			{
				entity.UnCollideWith(this);
				UnCollideWith(entity);
			}
		}
		OnLeaveTile(tile);
	}

	public virtual void OnEnterTile(Tile tile)
	{
		if (CanSlide)
		{
			BSlide.OnEnterTile(tile);
		}
		if (justTeleported)
		{
			justTeleported = false;
		}
	}

	public virtual void OnLeaveTile(Tile tile)
	{
	}

	public virtual bool CanEnterNullTiles()
	{
		return false;
	}

	public void DrawShadow(Vector2 position, float scaleH = 1f, float scaleV = 1f, float opacity = 1f)
	{
		base.core.Renderer["bg", 2, false].DrawSpriteW(base.core.SpriteManager.GetSprite(SpriteName.shadow), position - new Vector2(16f * scaleH * 0.5f, 8f * scaleV * 0.5f), Color.Black * 0.2f * opacity, new Vector2(scaleH, scaleV));
	}

	public virtual void CollideWith(Entity other)
	{
	}

	public virtual void UnCollideWith(Entity other)
	{
	}

	public virtual bool IsPassableFor(Entity other)
	{
		return true;
	}

	public virtual void InteractWith(Entity other)
	{
	}

	public void SetFlying(bool value)
	{
		Flying = value;
		foreach (Tile occupiedTile in OccupiedTiles)
		{
			occupiedTile.RemoveEntity(this);
			LeaveTile(occupiedTile);
		}
		foreach (Tile occupiedWorldTile in OccupiedWorldTiles)
		{
			occupiedWorldTile.RemoveEntity(this);
			LeaveTile(occupiedWorldTile);
		}
		x += Origin.X;
		y += Origin.Y;
		CurrentPlatform = null;
		UpdateTiles();
		TryMoveToCoordinates(levelMap, WorldCoordinates);
		EnterTile(Tile);
	}

	public void UpdatePosition(float x, float y)
	{
		if (!IsBroken)
		{
			this.x = x - width * 0.5f;
			this.y = y - height * 0.5f;
			UpdateTiles();
		}
	}

	private bool killRewardGranted;
	public bool GrantsEnemyCoins => this is BatEntity or SlimeEntity or WispEntity or FollowerEntity ||
		this is SerpentEntity serpent && serpent.Part == SerpentEntity.SerpentPart.Head && !serpent.IsChineseDragon;
	public virtual void Break(Entity offender)
	{
		// Contact evaporation, scenery deaths and fear are not player kills.
		if (killRewardGranted || !IsBroken || !GrantsEnemyCoins || offender == null || offender == this || offender is CreepChar) return;
		killRewardGranted = true;
		var player = core.CurrentPlayState?.Player;
		if (player == null || player.Dead) return;
		var coin = new ItemEntity(WorldCenterCoordinates.X - 0.5f, WorldCenterCoordinates.Y - 0.5f,
			ItemEntity.ValueToType(core.CurrentPlayState.LevelGenerator.AvgCoinValue()));
		coin.SetTarget(player, 12);
		SendMessage(new SpawnEntityMessage(coin, null));
	}

	public void RemoveFromTiles()
	{
		foreach (Tile occupiedTile in OccupiedTiles)
		{
			occupiedTile.RemoveEntity(this);
			LeaveTile(occupiedTile);
		}
		foreach (Tile occupiedWorldTile in OccupiedWorldTiles)
		{
			occupiedWorldTile.RemoveEntity(this);
			LeaveTile(occupiedWorldTile);
		}
	}

	public bool IsMostlyOnTile(Tile tile)
	{
		if (tile == null)
		{
			return false;
		}
		float num = 0.1f;
		if ((float)tile.X - num <= x && (float)tile.Y - num <= y && (float)(tile.X + 1) + num >= x + width)
		{
			return (float)(tile.Y + 1) + num >= y + height;
		}
		return false;
	}

	protected List<Tile> GetTilesAt(TileMap map, Vector2 coordinates)
	{
		List<Tile> list = new List<Tile>();
		int num = (int)Math.Floor(coordinates.X + padding);
		int num2 = (int)Math.Ceiling(coordinates.X + width - padding);
		int num3 = (int)Math.Floor(coordinates.Y + padding);
		int num4 = (int)Math.Ceiling(coordinates.Y + height - padding);
		for (int i = num; i < num2; i++)
		{
			for (int j = num3; j < num4; j++)
			{
				Tile tile = map[i, j];
				if (tile != null)
				{
					list.Add(tile);
				}
			}
		}
		return list;
	}

	protected void UpdateTiles()
	{
		List<Tile> tilesAt = GetTilesAt(CurrentMap, Coordinates);
		foreach (Tile occupiedTile in OccupiedTiles)
		{
			occupiedTile.RemoveEntity(this);
			if (!tilesAt.Contains(occupiedTile))
			{
				LeaveTile(occupiedTile);
			}
		}
		foreach (Tile item in tilesAt)
		{
			item.AddEntity(this);
			if (!OccupiedTiles.Contains(item))
			{
				EnterTile(item);
			}
		}
		OccupiedTiles.Clear();
		OccupiedTiles.AddRange(tilesAt);
	}

	public bool UpdateWorldTilesFromPlatform()
	{
		bool result = true;
		if (CurrentPlatform == null)
		{
			return result;
		}
		List<Tile> tilesAt = GetTilesAt(levelMap, WorldCoordinates);
		foreach (Tile occupiedWorldTile in OccupiedWorldTiles)
		{
			occupiedWorldTile.RemoveEntity(this);
			if (!tilesAt.Contains(occupiedWorldTile))
			{
				LeaveTile(occupiedWorldTile);
			}
		}
		foreach (Tile item in tilesAt)
		{
			if (!item.ContainsHangingObstaclesFor(this))
			{
				item.AddEntity(this);
				if (!OccupiedWorldTiles.Contains(item))
				{
					EnterTile(item);
				}
			}
			else
			{
				result = false;
			}
		}
		OccupiedWorldTiles.Clear();
		OccupiedWorldTiles.AddRange(tilesAt);
		OnPlatformMoved();
		return result;
	}

	public virtual void OnPlatformMoved()
	{
	}

	public bool TryMoveToCoordinates(TileMap map, Vector2 newCoordinates, int depth = 0, bool actuallyMove = true)
	{
		if (depth >= 20)
		{
			return false;
		}
		if (depth == 0 && map.Platform == CurrentPlatform)
		{
			Vector2 v = (newCoordinates - Coordinates).Direction();
			if (!v.IsEqualTo(Vector2.Zero))
			{
				LastMovementDir = v.Clone();
			}
		}
		bool result = false;
		List<Tile> tilesAt = GetTilesAt(map, newCoordinates);
		Tile tile = tilesAt.Find((Tile t) => newCoordinates.X + width - t.Coordinates.X >= 0.5f * width && t.Coordinates.X + 1f - newCoordinates.X >= 0.5f * width && newCoordinates.Y + height - t.Coordinates.Y >= 0.5f * height && t.Coordinates.Y + 1f - newCoordinates.Y >= 0.5f * height);
		if (!Flying && (tile == null || tile.Type == TileType.Pit))
		{
			if (map.Platform != null)
			{
				if (actuallyMove)
				{
					foreach (Tile occupiedWorldTile in OccupiedWorldTiles)
					{
						occupiedWorldTile.RemoveEntity(this);
					}
				}
				result = TryMoveToCoordinates(levelMap, newCoordinates + map.Platform.Coordinates, depth + 1, actuallyMove);
				if (actuallyMove)
				{
					OccupiedWorldTiles.Clear();
				}
				return result;
			}
			Tile tile2 = tilesAt.Find((Tile t) => t.Type == TileType.Pit && t.Entities.Find((Entity e) => e.IsPlatform && e != CurrentPlatform) != null && t.IsPassableFor(this));
			if (tile2 != null && tile2.Entities.Find((Entity e) => e.IsPlatform && e != CurrentPlatform) is PlatformEntity platformEntity && GetTilesAt(platformEntity.Map, newCoordinates - platformEntity.Coordinates).Count > 0)
			{
				return TryMoveToCoordinates(platformEntity.Map, newCoordinates - platformEntity.Coordinates, depth + 1, actuallyMove);
			}
		}
		foreach (Tile item in tilesAt)
		{
			if (item == null || item.IsPassableFor(this))
			{
				continue;
			}
			foreach (Entity entity in item.Entities)
			{
				entity.InteractWith(this);
				InteractWith(entity);
			}
			return result;
		}
		if (tile != null || CanEnterNullTiles())
		{
			if (!actuallyMove)
			{
				return true;
			}
			float num = newCoordinates.X;
			float num2 = newCoordinates.Y;
			if (tile != null && CurrentPlatform != map.Platform && gridAligned)
			{
				num = tile.X;
				num2 = tile.Y;
			}
			dxTween.Start(WorldCoordinates.X - (num + map.X), 0f, dAnimDuration);
			dyTween.Start(WorldCoordinates.Y - (num2 + map.Y), 0f, dAnimDuration);
			CurrentPlatform = map.Platform;
			x = num;
			y = num2;
			result = true;
		}
		UpdateTiles();
		return result;
	}

	public void FlushTiles()
	{
		foreach (Tile occupiedWorldTile in OccupiedWorldTiles)
		{
			occupiedWorldTile.Entities.Remove(this);
		}
		foreach (Tile occupiedTile in OccupiedTiles)
		{
			occupiedTile.Entities.Remove(this);
		}
		UpdateTiles();
	}

	public virtual bool CanTeleport()
	{
		return false;
	}

	public virtual int TeleportDelay()
	{
		return 1;
	}

	public void TeleportTo(TileMap map, Vector2 coordinates, TeleportEntity teleportFrom, TeleportEntity teleportTo, int timeout = 1)
	{
		if (CanTeleport())
		{
			teleportTimeout = timeout;
			teleportMap = map;
			teleportX = coordinates.X;
			teleportY = coordinates.Y;
			SourceTeleport = teleportFrom;
			DestTeleport = teleportTo;
		}
	}

	private void DoTeleport()
	{
		if (!CanTeleport())
		{
			return;
		}
		justTeleported = true;
		CurrentPlatform = teleportMap.Platform;
		if (OnDoTeleport())
		{
			x = teleportX;
			y = teleportY;
			FlushTiles();
			if (!TryMoveToCoordinates(CurrentMap, Coordinates))
			{
				(this as IPushableEntity)?.Crash(null);
			}
		}
	}

	protected virtual bool OnDoTeleport()
	{
		return true;
	}
}
