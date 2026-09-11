using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class TorchEntity : Entity
{
	public enum TorchPosition
	{
		N,
		NE,
		E,
		SE,
		S,
		SW,
		W,
		NW
	}

	private TorchPosition pos;

	private Sprite baseSprite;

	private Sprite glowSprite;

	private Animation fire;

	private int animStart;

	private Vector2 db;

	private Vector2 df;

	private Light light;

	public TorchEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		Init((TorchPosition)desc["pos"], desc.Flipped);
	}

	public TorchEntity(int x, int y, TorchPosition position)
		: base(x, y, 1f, 1f)
	{
		Init(position);
	}

	private void Init(TorchPosition pos, bool flipped = false)
	{
		fire = new Animation(0.25f);
		fire.AddAndPlay("burn", new List<SpriteName>
		{
			SpriteName.torch_fire_1,
			SpriteName.torch_fire_2,
			SpriteName.torch_fire_3,
			SpriteName.torch_fire_4,
			SpriteName.torch_fire_5,
			SpriteName.torch_fire_6
		});
		fire.SkipToRandomFrame();
		animStart = fire.GetCurrentFrameNumber();
		baseSprite = base.core.SpriteManager.GetSprite(SpriteName.dungeon_torch);
		glowSprite = base.core.SpriteManager.GetSprite(SpriteName.glow);
		this.pos = pos;
		if (flipped)
		{
			this.pos = (TorchPosition)((int)(8 - pos)).Mod(8);
		}
		int num = (int)this.pos;
		int num2;
		switch (num)
		{
		default:
			num2 = -1;
			break;
		case 1:
		case 2:
		case 3:
			num2 = 1;
			break;
		case 0:
		case 4:
			num2 = 0;
			break;
		}
		int num3 = num2;
		int num4;
		switch (num)
		{
		default:
			num4 = -1;
			break;
		case 3:
		case 4:
		case 5:
			num4 = 1;
			break;
		case 2:
		case 6:
			num4 = 0;
			break;
		}
		int num5 = num4;
		db = new Vector2(8f * (float)num3, 8f * (float)num5);
		df = default(Vector2).Copy(db);
		if (num3 != 0 && num5 == 0)
		{
			db.X += num3;
			df.X += num3 * 2;
		}
		if (num5 == -1)
		{
			df.Y -= 2f;
		}
	}

	public override void Load()
	{
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16759608), 1.5f, 0.7f, this);
		light.FollowRate = 1f;
		light.ChangeRate = 1f;
		light.Offset = df;
		base.Load();
	}

	public override void Update()
	{
		fire.Update();
		int num = base.worldTicks + animStart * 10;
		light.TargetIntencity = 0.7f + 0.01f * Component._sin((float)num * 0.15f);
		light.TargetRadius = 1.5f + 0.05f * Component._sin((float)num * 0.2f);
		base.Update();
	}

	public override void Draw()
	{
		int depth = base.Z + ((!(db.Y > 0.1f)) ? 1 : 3);
		Holiday holiday = base.core.Holiday;
		if (holiday == Holiday.ChunJie)
		{
			base.core.Renderer[depth].DrawSpriteW(_(SpriteName.chunjie_torch), base.WorldPosition + new Vector2(7.5f, -1f) + db, null, null, Component._sin(((float)base.worldTicks + x * 30f + y * 50f) * 0.07f) * 0.1f, SpriteFlip.None, SpriteOrigin.TopCenter);
			base.core.Renderer[depth].DrawSpriteW(glowSprite, base.WorldPosition + new Vector2(-1.5f + (float)Math.Sin((float)base.worldTicks / 4f) * 0.5f, -6f + (float)Math.Sin((float)base.worldTicks / 8f) * 0.3f) + df, Color.Gold * (0.35f + 0.25f * (float)Math.Sin((float)base.worldTicks / 5f)));
		}
		else
		{
			base.core.Renderer[depth, true].DrawSpriteW(baseSprite, base.WorldPosition + new Vector2(4f, 0f) + db);
			base.core.Renderer[depth].DrawSpriteW(glowSprite, base.WorldPosition + new Vector2(-1.5f + (float)Math.Sin((float)base.worldTicks / 4f) * 0.5f, -10f + (float)Math.Sin((float)base.worldTicks / 8f) * 0.3f) + df, Color.Gold * (0.75f + 0.25f * (float)Math.Sin((float)base.worldTicks / 5f)));
			base.core.Renderer[depth].DrawSpriteW(fire.GetCurrentFrame(), base.WorldPosition + new Vector2(5f, -10f) + df);
		}
		base.Draw();
	}

	public override void InteractWith(Entity other)
	{
	}
}
