using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ObstacleEntity : Entity
{
	private static BagOf<SpriteName> bagOfObstacles;

	private bool invisible;
	public bool CastsLightShadow => !invisible && !IsBroken;

	private bool flipped;

	private bool lightUp;

	private bool lit;

	private Animation fire;

	private Sprite fireGlow;

	private Vector2 firePos = Vector2.Zero;

	private float fireScale;

	private const int fireAnimDuration = 35;

	private int fireAnim = 35;

	public Entity Host;

	public Light light;

	private Sprite sprite;

	static ObstacleEntity()
	{
		bagOfObstacles = new BagOf<SpriteName>().Put(SpriteName.column_1).Put(SpriteName.column_2).Put(SpriteName.column_3)
			.Put(SpriteName.column_4)
			.Put(SpriteName.column_5)
			.Put(SpriteName.column_6)
			.Put(SpriteName.rock_1)
			.Put(SpriteName.rock_2)
			.Put(SpriteName.rock_3)
			.Put(SpriteName.rock_4)
			.Put(SpriteName.rock_5)
			.Put(SpriteName.rock_6)
			.Put(SpriteName.rock_7)
			.Put(SpriteName.rock_8)
			.Put(SpriteName.rock_9)
			.Put(SpriteName.sculpture_1)
			.Put(SpriteName.sculpture_2)
			.Put(SpriteName.sculpture_3)
			.Put(SpriteName.pillar_1);
	}

	public ObstacleEntity(int x, int y, bool invisible = false, int kind = 0, int index = 0, int flip = 0)
		: base(x, y, 0.1f, 0.1f)
	{
		this.invisible = invisible;
		if (!invisible)
		{
			Init(kind, index, flip);
		}
	}

	public ObstacleEntity(int x, int y, TileDesc desc)
		: base(x, y, 0.1f, 0.1f)
	{
		Init(desc["kind"], desc["index"], desc["flip"], desc.Flipped);
	}

	private void Init(int kind, int index, int flip, bool flippedTile = false)
	{
		switch (flip)
		{
		case 0:
			flipped = SciHelper.ChanceRoll();
			break;
		case 1:
			flipped = false;
			break;
		case 2:
			flipped = true;
			break;
		}
		if (flippedTile)
		{
			flipped = !flipped;
		}
		if (kind == 0)
		{
			sprite = _(bagOfObstacles.Draw((SpriteName o) => o.ToString().Contains("rock") || o.ToString().Contains("column")));
			return;
		}
		string kindStr = "";
		switch (kind)
		{
		case 1:
			kindStr = "rock";
			break;
		case 2:
			kindStr = "column";
			break;
		case 3:
			kindStr = "sculpture";
			if (index == 3)
			{
				lightUp = true;
				firePos = new Vector2(flipped ? (-8.5f) : 21.5f, 0f);
			}
			break;
		case 4:
			kindStr = "pillar";
			lightUp = true;
			firePos = new Vector2(flipped ? 9f : 4f, -8f);
			break;
		}
		if (index == 0)
		{
			sprite = _(bagOfObstacles.Draw((SpriteName o) => o.ToString().Contains(kindStr)));
		}
		else
		{
			sprite = base.core.SpriteManager.GetSprite(kindStr + "_" + index);
		}
		if (lightUp)
		{
			fire = new Animation(0.25f);
			fire.AddAndPlay("burn", new List<SpriteName>
			{
				SpriteName.torch_fire_blue_1,
				SpriteName.torch_fire_blue_2,
				SpriteName.torch_fire_blue_3,
				SpriteName.torch_fire_blue_4,
				SpriteName.torch_fire_blue_5,
				SpriteName.torch_fire_blue_6
			});
			fire.SkipToRandomFrame();
			fireGlow = _(SpriteName.glow);
		}
	}

	public override void Update()
	{
		if (lightUp && !lit && base.core.CurrentPlayState.Player.WorldCoordinates.Y <= base.WorldCoordinates.Y)
		{
			lit = true;
			light.Active = true;
		}
		if (lit)
		{
			fire.Update();
			light.TargetIntencity = 0.7f + 0.1f * Component._sin((float)base.worldTicks * 0.15f);
			light.TargetRadius = 1.5f + 0.2f * Component._sin((float)base.worldTicks * 0.2f);
			if (fireAnim > 0)
			{
				fireAnim--;
				fireScale = (float)Tween.BackEaseOut(35 - fireAnim, 0.0, 1.0, 35.0);
			}
		}
		base.Update();
	}

	public override void Load()
	{
		if (lightUp)
		{
			light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(6996223), 1.5f, 0.7f, this);
			light.ChangeRate = 1f;
			light.Active = false;
			light.Offset = firePos.Shift(0f, 5f);
		}
		base.Load();
	}

	public override void Draw()
	{
		if (!invisible)
		{
			SpriteFlip flip = (flipped ? SpriteFlip.Horizontal : SpriteFlip.None);
			Vector2 position = (16f * (base.WorldCoordinates + new Vector2(0.5f))).Shift(flipped ? ((float)(-sprite.Width + sprite.LinkX) - 0.5f) : (-0.5f - (float)sprite.LinkX), -sprite.LinkY);
			base.core.Renderer[base.Z, true].DrawSpriteW(sprite, position, null, null, 0f, flip);
			base.core.Renderer["bg", base.Z + 64, false].DrawSpriteW(sprite, (16f * (base.WorldCoordinates + new Vector2(0.5f))).Shift(flipped ? ((float)(-sprite.Width + sprite.LinkX) - 0.5f) : (-0.5f - (float)sprite.LinkX), -sprite.Height + sprite.LinkY + 7), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, flipped ? (SpriteFlip.Horizontal | SpriteFlip.Vertical) : SpriteFlip.Vertical);
			if (lit)
			{
				base.core.Renderer[base.Z].DrawSpriteW(fireGlow, base.WorldCenter + firePos + new Vector2((float)Math.Sin((float)base.worldTicks / 4f) * 0.5f, 3f + (float)Math.Sin((float)base.worldTicks / 8f) * 0.3f), Color.SkyBlue * (0.75f + 0.25f * (float)Math.Sin((float)base.worldTicks / 5f)), Vector2.One * fireScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer[base.Z].DrawSpriteW(fire.GetCurrentFrame(), base.WorldCenter + firePos, null, Vector2.One * fireScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
				var _ = fireScale;
			}
		}
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return other is FragmentEntity;
	}

	public override void Break(Entity offender)
	{
		if (Host != null && !Host.IsBroken)
		{
			Host.Break(offender);
			IsBroken = true;
			Host = null;
		}
		base.Break(offender);
	}
}
