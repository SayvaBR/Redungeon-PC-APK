using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class ItemEntity : Entity
{
	public static List<string> itemNames = new List<string> { "coin_gold", "coin_green", "coin_blue", "coin_red", "skull_key", "candy_cane", "gingerbread", "chunjie_tangerine" };

	public static List<string> itemAnimations = new List<string> { "123456", "123456", "123456", "123456", "12345678", "123456789abc", "123456789abc", "12345678" };

	public ItemType Type;

	private Animation animation;

	private Entity target;

	private int targetDelay;

	private int vanishA = -1;

	private int vanishD = 25;

	public Vector2 InitialPos;

	public bool Dancing;

	public bool HasTarget { get; private set; }

	public static ItemType ValueToType(int value)
	{
		return value switch
		{
			3 => ItemType.BlueCoin, 
			2 => ItemType.GreenCoin, 
			1 => ItemType.GoldCoin, 
			_ => ItemType.RedCoin, 
		};
	}

	public ItemEntity(float x, float y, ItemType type)
		: base(x + 0.45f, y + 0.45f, 0.1f, 0.1f)
	{
		Type = type;
		float speed = 0.25f;
		if (type == ItemType.Ginger || type == ItemType.CandyCane || type == ItemType.Tangerine)
		{
			speed = 0.17f;
		}
		animation = new Animation(speed);
		List<Sprite> list = new List<Sprite>();
		string text = itemAnimations[(int)Type];
		string text2 = itemNames[(int)Type];
		for (int i = 0; i < text.Length; i++)
		{
			list.Add(base.core.SpriteManager.GetSprite(text2 + "_" + text[i]));
		}
		animation.AddAndPlay("spin", list);
		animation.SkipToRandomFrame();
		Sprite spark = _(SpriteName.spark);
		base.core.ParticleManager.AddEmitter(inWorld: true, base.WorldCenter, 3f).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Position -= base.WorldCenter;
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age > 20;
			})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer[base.Z + 1].DrawSpriteW(spark, base.WorldCenter + p.Position.Shift(0f, -7f), null, new Vector2((float)(20 - p.Age) / 20f), (float)p.Age * 0.05f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Start(50 + Component._rnd(0, 20));
	}

	public override void Load()
	{
		base.Load();
	}

	public override void Update()
	{
		if (Dancing && base.core.CurrentPlayState.Player != null && base.core.CurrentPlayState.Player.Dead)
		{
			Dancing = false;
		}
		animation.Update();
		if (targetDelay > 0)
		{
			targetDelay--;
		}
		if (target != null && targetDelay == 0)
		{
			if (CurrentPlatform != null)
			{
				x += CurrentPlatform.WorldCoordinates.X;
				y += CurrentPlatform.WorldCoordinates.Y;
				CurrentPlatform = null;
			}
			x += (target.WorldCenterCoordinates.X - x) * 0.075f;
			y += (target.WorldCenterCoordinates.Y - y) * 0.075f;
			UpdateTiles();
			if (target.Unloaded && vanishA == -1)
			{
				vanishA = vanishD;
			}
			if (vanishA > 0)
			{
				vanishA--;
				if (vanishA == 0)
				{
					IsBroken = true;
					SendMessage(new RemoveEntityMessage(this));
				}
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = (float)Math.Sin((float)((double)base.worldTicks + Math.Sin(x + y) * 20.0) / 20f) * 2.5f;
		base.core.Renderer[base.Z + (Dancing ? 1000 : 0)].DrawSpriteW(animation.GetCurrentFrame(), base.WorldCenter.Shift(0f, -7f - num), null, Vector2.One * ((base.Age < 25) ? ((float)base.Age / 25f) : 1f) * ((vanishA >= 0) ? ((float)vanishA / (float)vanishD) : 1f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(animation.GetCurrentFrame(), base.WorldPosition.Shift(0.5f, 5f + num * 0.3f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical, SpriteOrigin.Center);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (Dancing)
		{
			return;
		}
		if (other is PlayerEntity { Dead: false, Falling: false } playerEntity)
		{
			base.core.ParticleManager.MakeItemToHudEmitter(base.WorldCenter, Type, many: false, Type != ItemType.SkullKey).Emit(1);
			switch (Type)
			{
			case ItemType.GoldCoin:
				playerEntity.CollectCoins(1, this, default(Color).FromRgb(14853902));
				break;
			case ItemType.GreenCoin:
				playerEntity.CollectCoins(2, this, default(Color).FromRgb(4760625));
				break;
			case ItemType.BlueCoin:
				playerEntity.CollectCoins(3, this, default(Color).FromRgb(2522871));
				break;
			case ItemType.RedCoin:
				playerEntity.CollectCoins(4, this, default(Color).FromRgb(14040624));
				break;
			case ItemType.SkullKey:
				(playerEntity as BraggChar)?.CollectKey();
				break;
			case ItemType.Ginger:
				playerEntity.CollectCoins(5, this, default(Color).FromRgb(14853902));
				break;
			case ItemType.CandyCane:
				playerEntity.CollectCoins(5, this, default(Color).FromRgb(14853902));
				break;
			case ItemType.Tangerine:
				playerEntity.CollectCoins(5, this, default(Color).FromRgb(16755743));
				break;
			}
			int value = Type switch
			{
				ItemType.GreenCoin => 5891368, 
				ItemType.BlueCoin => 2656997, 
				ItemType.RedCoin => 15018024, 
				ItemType.SkullKey => 16777215, 
				_ => 16495673, 
			};
			Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(value), 0.8f, 0.4f, this);
			light.FollowRate = 1f;
			light.ChangeRate = 0.1f;
			light.Radius = 4f;
			light.Intencity = 0.8f;
			IsBroken = true;
			SendMessage(new RemoveEntityMessage(this));
		}
		base.CollideWith(other);
	}

	public void SetTarget(Entity newTarget, int delay = 0)
	{
		HasTarget = true;
		targetDelay = delay;
		target = newTarget;
	}
}
