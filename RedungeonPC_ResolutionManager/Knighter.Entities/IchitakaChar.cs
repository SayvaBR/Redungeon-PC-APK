using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class IchitakaChar : PlayerEntity
{
	private int r;

	private Sprite area;

	private ParticleEmitter magnetEffect;

	private int danceT;

	private int danceD = 100;

	private List<ItemEntity> dancingCoins;

	[Preserve]
	public IchitakaChar(int x, int y)
		: base(x, y)
	{
		normalAnimSpeed = 0.1f;
		animation = new Animation(normalAnimSpeed);
		animation.Add("n", "ichitaka_n_", "1213");
		animation.Add("s", "ichitaka_s_", "1213");
		animation.Add("w", "ichitaka_w_", "1213");
		animation.Add("e", "ichitaka_e_", "1213");
		animation.Add("spin", "ichitaka_fall_", "11112222");
		PosShift = new Vector2(-5f, -7f);
		dancingCoins = new List<ItemEntity>();
	}

	public override void InitAbilities(Abilities abilities)
	{
		base.InitAbilities(abilities);
		r = Abilities.SkillLevel[Skill.CoinMagnetRadius];
		area = _(SpriteName.ichitaka_area);
		magnetEffect = base.core.ParticleManager.AddEmitter(inWorld: true, base.Center).AttachTo(this).OnSpawn(delegate(Particle p)
		{
			p.Position -= base.WorldCenter;
		})
			.OnUpdate(delegate(Particle p)
			{
				p.Dead = p.Age > 50 * r;
			})
			.OnDraw(delegate(Particle p)
			{
				float num = (float)p.Age / (float)(50 * r);
				base.core.Renderer["bg", base.Z + 300, false].DrawSpriteW(area, base.WorldCenter + base.dAnim, Color.Lerp(Color.Red, Color.Gold, num) * (0.1f * num), new Vector2((1f - (float)(p.Age + 10) / (float)(r * 50 + 10)) * (((float)r + ((r == 1) ? 1f : 0.5f)) / 3f)), 0f, SpriteFlip.None, SpriteOrigin.Center);
			})
			.Start(20);
	}

	public override bool SpawnFragments(bool bolt = false)
	{
		SendMessage(new PlaySoundMessage(SoundName.ribb_death));
		if (!bolt)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.ichitaka_frag_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.ichitaka_frag_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.ichitaka_frag_3), null));
		}
		for (int i = 0; i < 5; i++)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates.Shift(0f, -0.4f), SpriteName.ichitaka_frag_4), null));
		}
		return true;
	}

	public override bool SpawnLeftovers(Vector2 pos, bool bolt = false)
	{
		SendMessage(new SpawnEntityMessage(new FragmentEntity(pos, SpriteName.ichitaka_mask), null));
		return true;
	}

	public override void TryTriggerAbility()
	{
		if (Dead)
		{
			return;
		}
		if (Abilities.SkillCharge[Skill.Telekinesis] < 1f)
		{
			base.playState.Hud.AbilitiesHud.skillPanels[Skill.Telekinesis].Shake();
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_still_charging)), CurrentPlatform));
			return;
		}
		List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, 8f).FindAll((Entity c) => !c.IsBroken && ((c is ChestEntity && !(c as ChestEntity).Looted) || (c is FollowerEntity && (c as FollowerEntity).Kind == FollowerKind.Red)));
		if (list.Count > 0)
		{
			Abilities.SkillCharge[Skill.Telekinesis] = 0f;
			foreach (Entity item in list)
			{
				if (item is ChestEntity || item is FollowerEntity)
				{
					item.Break(this);
				}
				Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(14853902), 0.8f, 0.4f, item);
				light.FollowRate = 1f;
				light.ChangeRate = 0.1f;
				light.Radius = 4f;
				light.Intencity = 0.8f;
				light.Die();
			}
			Light light2 = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(14853902), 0.8f, 0.4f, this);
			light2.FollowRate = 1f;
			light2.ChangeRate = 0.005f;
			light2.Radius = 5f;
			light2.Intencity = 1.2f;
			light2.Die();
			danceT = danceD;
			base.playState.Camera.Shake("telekinesis", 2f, 20);
			SendMessage(new PlaySoundMessage(SoundName.ichi_drums_1));
		}
		else
		{
			SendMessage(new SpawnEntityMessage(new FloatingTextEntity(base.CenterCoordinates, __(SId.SKILL_TELEKINESIS_alert_no_chests)), CurrentPlatform));
		}
		base.TryTriggerAbility();
	}

	public void AddDancingCoin(ItemEntity coin)
	{
		dancingCoins.Add(coin);
		coin.InitialPos = coin.WorldCenterCoordinates.Clone();
		coin.Dancing = true;
	}

	public override void Update()
	{
		int num = 0;
		if (danceT > 0)
		{
			danceT--;
			dancingCoins.RemoveAll((ItemEntity coin) => coin.Unloaded);
			int num2 = 0;
			int count = dancingCoins.Count;
			foreach (ItemEntity dancingCoin in dancingCoins)
			{
				if (danceT > 0)
				{
					int num3 = ((num2 % 2 == 0) ? 1 : (-1));
					float num4 = 2.5f + 0.3f * (float)num3;
					float num5 = (float)danceT * 0.1f + (float)num2 * 2f * (float)Math.PI / (float)count;
					int num6 = 30;
					if (danceT < num6)
					{
						num4 *= (float)danceT / (float)num6;
					}
					Vector2 vector = base.WorldCenterCoordinates.Shift(Component._cos(num5) * num4, Component._sin(num5) * num4) + base.dAnim / 16f;
					int num7 = 30;
					if (danceT > danceD - num7)
					{
						float num8 = (float)(danceT - (danceD - num7)) / (float)num7;
						vector += (dancingCoin.InitialPos - vector) * num8;
					}
					dancingCoin.CurrentPlatform = null;
					dancingCoin.UpdatePosition(vector.X, vector.Y);
					num2++;
				}
				else
				{
					SendMessage(new RemoveEntityMessage(dancingCoin));
					switch (dancingCoin.Type)
					{
					case ItemType.GoldCoin:
						num++;
						break;
					case ItemType.GreenCoin:
						num += 2;
						break;
					case ItemType.BlueCoin:
						num += 3;
						break;
					case ItemType.RedCoin:
						num += 4;
						break;
					}
				}
			}
			if (danceT == 0)
			{
				CollectCoins(num, null, default(Color).FromRgb(14853902));
				Light light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(14853902), 0.8f, 0.4f, this);
				light.FollowRate = 1f;
				light.ChangeRate = 0.05f;
				light.Radius = 5f;
				light.Intencity = 1.4f;
				light.Die();
				base.core.PlayCoinSound();
				base.core.PlayCoinSound(5);
				base.core.PlayCoinSound(10);
				base.core.PlayCoinSound(15);
			}
		}
		base.Update();
	}

	protected override void UpdateAbilities()
	{
		if (base.playState.Started && Abilities.SkillLevel[Skill.CoinMagnetRadius] > 0)
		{
			int num = Abilities.SkillLevel[Skill.CoinMagnetRadius];
			List<Entity> list = base.playState.EntityManager.GetEntitiesInRadius(base.WorldCenterCoordinates, num).FindAll((Entity e) => e is ItemEntity && !((ItemEntity)e).HasTarget);
			foreach (Entity item in list)
			{
				(item as ItemEntity).SetTarget(this);
			}
			if (list.Count > 0)
			{
				_inc(Stat.IchitakaCoinsCollectedWithMagnet, list.Count);
			}
		}
		base.UpdateAbilities();
	}

	public override SpriteName ShotSprite(int dir)
	{
		return SpriteName.ichitaka_shot;
	}
}
