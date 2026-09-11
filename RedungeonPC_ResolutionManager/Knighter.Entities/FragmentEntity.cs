using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FragmentEntity : Entity
{
	private struct TrailPoint
	{
		public bool used;

		public bool falling;

		public float x;

		public float y;

		public float elevation;

		public float rotation;

		public int z;
	}

	private readonly Sprite sprite;

	private Animation animation;

	private bool useAnim;

	private float dx;

	private float dy;

	private float de;

	private float dr;

	private float elevation;

	private float rotation;

	private TrailPoint[] trail;

	private int trailLength = 10;

	private int trailHead = -1;

	private bool falling;

	private int fallAnim;

	private int ttl;

	private int age;

	private string layer;

	private float bounce;

	private float fric;

	private Color tint = Color.White;

	private Color tintFrom;

	private Color tintTo;

	private int tintDuration = -1;

	private SoundName bounceSound;

	private Action<FragmentEntity> onFall;

	public FragmentEntity(Vector2 coordinates, SpriteName spriteName, int ttl = -1, Vector4 direction = default(Vector4), string layer = "", float elevation = 0.2f, float bounce = 0.6f, float fric = 0.95f, SoundName bounceSound = SoundName.none, string spriteStr = "", Color? tint = null)
		: base(coordinates.X, coordinates.Y, 0.4f, 0.4f)
	{
		trail = new TrailPoint[trailLength];
		if (ttl == -1)
		{
			ttl = 200;
		}
		if (tint.HasValue)
		{
			this.tint = tint.Value;
		}
		sprite = ((spriteStr == "") ? base.core.SpriteManager.GetSprite(spriteName) : _(spriteStr));
		if (direction == default(Vector4))
		{
			dx = SciHelper.GetRandom(-0.15f, 0.15f);
			dy = SciHelper.GetRandom(-0.15f, 0.15f);
			de = SciHelper.GetRandom(1f, 2.5f);
			dr = SciHelper.GetRandom(-1f, 1f);
		}
		else
		{
			Vector4 vector = direction;
			dx = vector.X;
			dy = vector.Y;
			de = vector.Z;
			dr = vector.W;
		}
		rotation = 0f;
		this.elevation = elevation;
		falling = false;
		padding = -0.2f;
		this.bounce = bounce;
		this.fric = fric;
		this.layer = layer;
		this.ttl = ttl;
		age = 0;
		this.bounceSound = bounceSound;
	}

	public FragmentEntity SetTintFlash(Color tintFrom, Color tintTo, int duration)
	{
		this.tintFrom = tintFrom;
		this.tintTo = tintTo;
		tintDuration = duration;
		return this;
	}

	public FragmentEntity SetAnim(Animation anim)
	{
		animation = anim;
		useAnim = true;
		return this;
	}

	public FragmentEntity OnFall(Action<FragmentEntity> onFall)
	{
		this.onFall = onFall;
		return this;
	}

	public override bool CanTeleport()
	{
		return false;
	}

	private void UpdatePosition()
	{
		if (!falling && !TryMoveToCoordinates(base.CurrentMap, base.Coordinates + new Vector2(dx, dy)))
		{
			if (dx > dy)
			{
				dx *= -1f;
			}
			else
			{
				dy *= -1f;
			}
		}
		elevation += de;
		de -= 0.1f;
		if (elevation < 0.2f && (OccupiedTiles.Find((Tile t) => t.Type == TileType.Pit && t.MostlyContains(this)) != null || OccupiedTiles.Count == 0))
		{
			falling = true;
		}
		rotation += dr;
		if (!falling && elevation <= 0f)
		{
			de *= 0f - bounce;
			dx *= 0.5f;
			dy *= 0.5f;
			dr = 0f;
			if (bounceSound != SoundName.none)
			{
				SendMessage(new PlayWorldSoundMessage(bounceSound, base.WorldPosition, 0.6f));
			}
			if (onFall != null)
			{
				onFall(this);
			}
		}
		dx *= fric;
		dy *= fric;
		dr *= fric;
		if (falling)
		{
			if (fallAnim < 20)
			{
				fallAnim++;
			}
			else
			{
				SendMessage(new RemoveEntityMessage(this));
			}
		}
	}

	public override void Update()
	{
		if (useAnim && animation != null && (elevation > 0.2f || falling))
		{
			animation.Update();
		}
		if (base.worldTicks % 2 == 0)
		{
			trailHead++;
			if (trailHead >= trailLength)
			{
				trailHead = 0;
			}
			trail[trailHead].x = base.WorldPosition.X;
			trail[trailHead].y = base.WorldPosition.Y;
			trail[trailHead].elevation = elevation;
			trail[trailHead].rotation = rotation;
			trail[trailHead].z = base.Z;
			trail[trailHead].used = true;
			trail[trailHead].falling = falling;
		}
		UpdatePosition();
		UpdateTiles();
		if (ttl >= 0)
		{
			if (age == ttl)
			{
				SendMessage(new RemoveEntityMessage(this));
			}
			age++;
		}
		if (tintDuration > 0 && age <= tintDuration)
		{
			tint = Color.Lerp(tintFrom, tintTo, (float)age / (float)tintDuration);
		}
		base.Update();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}

	public override void OnEnterTile(Tile tile)
	{
		base.OnEnterTile(tile);
	}

	public void Crash()
	{
	}

	public override void Draw()
	{
		float num = 1f;
		if (falling)
		{
			num *= 1f - (float)fallAnim / 20f;
		}
		if (ttl > 0)
		{
			num *= (float)Math.Min(ttl - age, 20) / 20f;
		}
		if (base.core.TakingScreenshot && trailHead >= 0 && trailHead < trail.Length)
		{
			int num2 = trailHead;
			float num3 = 0.15f;
			for (int i = 0; i < trailLength; i++)
			{
				TrailPoint trailPoint = trail[num2];
				if (trailPoint.used)
				{
					(trailPoint.falling ? base.core.Renderer["bg", trailPoint.z - 1 - 16, false] : ((layer == "") ? base.core.Renderer[trailPoint.z - 1 - 16] : base.core.Renderer[layer, -17, false])).DrawSpriteW(useAnim ? animation.GetCurrentFrame() : sprite, new Vector2(trailPoint.x, trailPoint.y) + new Vector2(3f, 0f - trailPoint.elevation), Color.White * num * num3, null, trailPoint.rotation, SpriteFlip.None, SpriteOrigin.Center);
				}
				num3 -= 0.15f / (float)trailLength;
				num2--;
				if (num2 < 0)
				{
					num2 = trailLength - 1;
				}
			}
		}
		(falling ? base.core.Renderer["bg", base.Z - 1, false] : ((layer == "") ? base.core.Renderer[base.Z - 1] : base.core.Renderer[layer, -1, false])).DrawSpriteW(useAnim ? animation.GetCurrentFrame() : sprite, base.WorldPosition + new Vector2(3f, 0f - elevation), tint * num, null, rotation, SpriteFlip.None, SpriteOrigin.Center);
		if (!falling)
		{
			DrawShadow(base.WorldCenter + new Vector2(0f, -sprite.Height / 2), 0.6f, 0.6f, num);
		}
		base.Draw();
	}
}
