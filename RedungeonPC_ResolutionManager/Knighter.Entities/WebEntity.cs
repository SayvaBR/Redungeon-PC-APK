using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class WebEntity : Entity
{
	private bool flawless = true;

	private static BagOf<SpriteName> bagOfWebs;

	private static Dictionary<string, SpriteName> arrows;

	private readonly Sprite sprite;

	private readonly Sprite cover;

	private SpriteFlip flip;

	private Stack<Vector2> pattern;

	private PlayerEntity capturedPlayer;

	private int arrowTimer;

	private const int arrowDuration = 10;

	private int pullTimer = -1;

	private const int pullDuration = 5;

	private int errorTimer = -1;

	private const int errorDuration = 10;

	private Vector2 errorVector;

	private static BagOf<SoundName> bagOfSounds;

	public int Difficulty { get; private set; }

	public Vector2 PullVector
	{
		get
		{
			if (errorTimer < 0)
			{
				return ((pattern.Count > 0) ? pattern.Peek() : Vector2.Zero) * ((float)(pullTimer + 1) / 5f);
			}
			return errorVector * ((float)(errorTimer + 1) / 10f) * 0.5f;
		}
	}

	static WebEntity()
	{
		bagOfWebs = new BagOf<SpriteName>().Put(SpriteName.spider_web_1).Put(SpriteName.spider_web_2).Put(SpriteName.spider_web_3);
		arrows = new Dictionary<string, SpriteName>
		{
			{
				"n",
				SpriteName.web_arrow_n
			},
			{
				"e",
				SpriteName.web_arrow_e
			},
			{
				"w",
				SpriteName.web_arrow_w
			},
			{
				"s",
				SpriteName.web_arrow_s
			}
		};
		bagOfSounds = new BagOf<SoundName>().Put(SoundName.web_1).Put(SoundName.web_2).Put(SoundName.web_3);
	}

	public WebEntity(int x, int y, TileDesc desc, int distance)
		: base(x, y, 1f, 1f)
	{
		sprite = _(bagOfWebs.Draw());
		switch (Component._rnd(0, 3))
		{
		case 0:
			flip = SpriteFlip.None;
			break;
		case 1:
			flip = SpriteFlip.Horizontal;
			break;
		case 2:
			flip = SpriteFlip.Vertical;
			break;
		case 3:
			flip = SpriteFlip.Horizontal | SpriteFlip.Vertical;
			break;
		}
		cover = _(SpriteName.spider_web_cover);
		Difficulty = desc["difficulty"];
		if (Difficulty == 0)
		{
			Difficulty = ((distance < 80) ? 2 : 3);
		}
		pattern = new Stack<Vector2>();
		for (int i = 0; i < Difficulty; i++)
		{
			int num = Component._rnd(0, 3);
			pattern.Push(new Vector2((num > 0) ? (num - 2) : 0, (num < 3) ? (num - 1) : 0));
		}
	}

	public override void Update()
	{
		if (capturedPlayer != null)
		{
			if (capturedPlayer.Dead)
			{
				return;
			}
			if (capturedPlayer.Flying)
			{
				ReleasePlayer();
			}
			if (arrowTimer < 10)
			{
				arrowTimer++;
			}
			if (pullTimer >= 0 && pullTimer < 5)
			{
				pullTimer++;
				if (pullTimer == 5)
				{
					pattern.Pop();
					arrowTimer = 0;
					pullTimer = -1;
					if (pattern.Count == 0)
					{
						ReleasePlayer();
						if (flawless)
						{
							_inc(Stat.FlawlessWebs);
						}
						_inc(Stat.WebsBroken);
					}
				}
			}
			if (errorTimer >= 0 && errorTimer < 10)
			{
				errorTimer++;
				if (errorTimer == 10)
				{
					errorTimer = -1;
				}
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = Component._M((float)pattern.Count / (float)Difficulty, 0.4f);
		base.core.Renderer["bg", 1000, true].DrawSpriteW(this.sprite, base.WorldPosition.Shift(0f, -1f) + ((pattern.Count > 0) ? (PullVector * 5f) : Vector2.Zero), Color.White * num, null, 0f, flip);
		if (capturedPlayer != null && pattern.Count > 0)
		{
			Vector2 vector = pattern.Peek();
			Sprite sprite = _(arrows[vector.DirectionId()]);
			float num2 = (float)(pullTimer + 1) / 5f;
			float num3 = (float)(errorTimer + 1) / 10f;
			float num4 = (float)arrowTimer / 10f;
			float num5 = ((pattern.Count == Difficulty) ? num4 : ((pattern.Count == 1) ? (1f - num2) : 1f));
			Color white = Color.White;
			base.core.Renderer["fg", -4, false].DrawSpriteS(_(SpriteName.glow_huge), base.core.Renderer.ScreenCenter + (new Vector2(-2f, -27f - (1f - num4) * 12f) + vector * (1f + Component._sin((float)base.ticks * 0.2f)) * ((pullTimer < 0) ? 1 : 0) + vector * (num2 * 15f)) / Settings.GuiScale, white * 0.5f * num5 * num4 * (1f - num2), Vector2.One * (0.8f + 0.1f * Component._sin((float)base.ticks * 0.2f)) / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if (arrowTimer >= 10)
			{
				base.core.Renderer["fg", -4, false].DrawSpriteS(sprite, base.core.Renderer.ScreenCenter + (new Vector2(0f, -27f - (1f - num4) * 12f) + vector * (1f + Component._sin((float)base.ticks * 0.2f)) * ((pullTimer < 0) ? 1 : 0) + vector * (num2 * 15f)) / Settings.GuiScale, (errorTimer >= 0) ? Color.Lerp(Color.Red, white, num3) : (white * (1f - num2) * num5), Vector2.One * num4 * (1f + num3 * 0.5f) / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			int num6 = -1;
			foreach (Vector2 item in pattern)
			{
				num6++;
				if (num6 != 0 || arrowTimer != 10)
				{
					sprite = _(arrows[item.DirectionId()]);
					base.core.Renderer["fg", -5, false].DrawSpriteS(sprite, base.core.Renderer.ScreenCenter + new Vector2(((float)(num6 + 1) - num4) * 25f, -27f) / Settings.GuiScale, Color.White * 0.5f * num5, Vector2.One * MathHelper.Lerp(0.5f, num4, (num6 == 0) ? num4 : 0f) / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
			}
			base.core.Renderer["fg", -6, false].DrawRectangleS(new Vector2(-5f, base.core.Renderer.ScreenCenter.Y - 47f / Settings.GuiScale), base.core.Renderer.ScreenWidth + 10, 40f / Settings.GuiScale, Color.Black * 0.65f * num5);
			base.core.Renderer[base.Z + 2, true].DrawSpriteW(cover, base.WorldCenter.Shift(0f, 1f) + PullVector * 5f, Color.White * (num * num5), new Vector2(1f + (1f - num4) * 0.5f, 1f), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity { Flying: false } playerEntity)
		{
			CapturePlayer(playerEntity);
		}
		base.CollideWith(other);
	}

	public override void UnCollideWith(Entity other)
	{
		if (other is PlayerEntity playerEntity && (playerEntity.CurrentPlatform != CurrentPlatform || playerEntity.Tile != base.Tile))
		{
			ReleasePlayer();
		}
		base.UnCollideWith(other);
	}

	private void CapturePlayer(PlayerEntity player)
	{
		if (player.WebCapture(this))
		{
			capturedPlayer = player;
			SendMessage(new PlayWorldSoundMessage(bagOfSounds.DrawDifferent(), base.WorldCenter));
		}
	}

	public void ReleasePlayer()
	{
		if (capturedPlayer != null)
		{
			capturedPlayer.WebRelease();
			capturedPlayer = null;
			SendMessage(new RemoveEntityMessage(this));
			SendMessage(new PlayWorldSoundMessage(SoundName.web_free, base.WorldCenter));
		}
	}

	public void Pull(Vector2 direction)
	{
		if (pullTimer < 0)
		{
			Vector2 v = pattern.Peek();
			SendMessage(new PlayWorldSoundMessage(bagOfSounds.DrawDifferent(), base.WorldCenter));
			if (v.DirectionId() == direction.DirectionId())
			{
				pullTimer = 0;
				return;
			}
			errorTimer = 0;
			errorVector = direction;
			flawless = false;
		}
	}
}
