using System;
using System.Diagnostics;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

[DebuggerDisplay("Looted: {Looted}, contains: ???")]
public class ChestEntity : Entity
{
	public enum ChestTier
	{
		Wood,
		Gold,
		Treasure,
		Present
	}

	public ChestTier Tier;

	public bool Looted;

	public ChestContents Contents;

	private readonly Animation animation;

	private int squish;

	private int squishDuration = 17;

	private int lockStrength;

	private int lockAnimOffset;

	private bool lidOpened;

	public ChestEntity(int x, int y, ChestContents contents, ChestTier tier = ChestTier.Gold)
		: base(x, y, 1f, 1f)
	{
		Tier = tier;
		Contents = contents;
		if (tier == ChestTier.Present)
		{
			Contents.Count = (int)((float)Contents.Count * 1.5f);
		}
		animation = new Animation(0.2f, loop: false);
		string text = "chest_";
		string framesChain = "111234";
		switch (tier)
		{
		case ChestTier.Wood:
			text += "wood_";
			break;
		case ChestTier.Treasure:
			text += "treasure_";
			break;
		case ChestTier.Present:
			text = "present_";
			framesChain = "112345";
			break;
		}
		animation.Add("open", text, framesChain);
		animation.Play("open");
		animation.Pause();
		Looted = false;
		squish = -1;
	}

	public ChestEntity Lock(int strength = 1)
	{
		lockStrength = strength;
		lockAnimOffset = Component._rnd(0, 120);
		return this;
	}

	public override void Update()
	{
		animation.Update();
		if (Tier == ChestTier.Present && animation.GetCurrentFrameNumber() == 3 && !lidOpened)
		{
			lidOpened = true;
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_top, 60, new Vector4(Component._rnd(-0.1f, 0.1f), Component._rnd(-0.1f, 0.1f), 2.5f, 0.02f), "", 19f), null));
		}
		if (squish >= 0)
		{
			squish++;
			if (squish > squishDuration)
			{
				squish = -1;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = ((squish < 0) ? 0f : (0.2f * Component._sin((float)Math.PI * 2f * (float)squish / (float)squishDuration)));
		base.core.Renderer[IsBroken ? "bg" : "default", IsBroken ? ((int)base.WorldPosition.Y + 16) : base.Z, false].DrawSpriteW(IsBroken ? _((Tier == ChestTier.Gold) ? SpriteName.chest_follower_base : ((Tier == ChestTier.Wood) ? SpriteName.chest_follower_base_wood : SpriteName.present_base)) : animation.GetCurrentFrame(), base.WorldPosition.Shift(7f + ((Tier == ChestTier.Present) ? 0.5f : 0f), 13f), null, new Vector2(1f + num, 1f - num), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
		if (lockStrength > 0 && !Looted)
		{
			base.core.Renderer[base.Z].DrawSpriteW(_(SpriteName.chest_lock), base.WorldCenter.Shift(0f, -3f + 2f * Component._sin((float)Math.PI * (float)squish / 17f)), null, null, Component._sin((float)(lockAnimOffset + base.worldTicks) * 0.05f) * 0.5f, SpriteFlip.None, SpriteOrigin.TopCenter);
			int num2 = (base.core.CurrentPlayState.Player as BraggChar)?.Keys ?? 0;
			base.core.Renderer[base.Z].DrawTextW(lockStrength.ToString(), base.WorldCenter.Shift(-1f, -7f + Component._sin((float)base.worldTicks * 0.1f)), new TextProfile
			{
				Font = Font.Bold,
				Color = ((num2 >= lockStrength) ? Color.White : Color.Red),
				SecondColor = Color.Black,
				Width = 200,
				Height = 20,
				TextAlignment = Alignment2D.BottomCenter,
				BoxAlignment = Alignment2D.BottomCenter,
				Scale = 0.8f,
				Decoration = TextDecoration.Contour
			});
		}
		if (!IsBroken)
		{
			base.core.Renderer["bg", base.Z + 32, false].DrawSpriteW(animation.GetCurrentFrame(), base.WorldPosition.Shift(-5f + ((Tier == ChestTier.Present) ? 1f : 0f), 2f), Color.Black * 0.2f, new Vector2(1f, 0.8f), 0f, SpriteFlip.Vertical);
		}
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		if (!IsBroken)
		{
			return other is FragmentEntity;
		}
		return true;
	}

	public override void InteractWith(Entity other)
	{
		if (!(other is PlayerEntity { Dead: false } playerEntity) || Looted)
		{
			return;
		}
		squish = 0;
		if (lockStrength > 0 && playerEntity is BraggChar)
		{
			if (!(playerEntity as BraggChar).SpendKeys(lockStrength))
			{
				SendMessage(new PlayWorldSoundMessage(SoundName.knight_step_2, base.WorldCenter));
				base.core.CurrentPlayState.Hud.ShowAlert("chest-locked", __(SId.SKILL_TREASUREHUNT_alert_keys_needed) + " " + lockStrength, default(Color).FromRgb(5452592), 110, SpriteName.chest_lock);
				return;
			}
			base.core.CurrentPlayState.Hud.ShowAlert("chest-unlocked", __(SId.SKILL_TREASUREHUNT_alert_keys_used) + " " + lockStrength, default(Color).FromRgb(5452592), 110, SpriteName.chest_lock_opened);
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_lock_opened, -1, new Vector4(SciHelper.ChanceRoll() ? (-0.2f) : 0.2f, Component._rnd(-0.1f, 0.1f), 2f, 0.5f)), null));
			SendMessage(new PlayWorldSoundMessage(SoundName.unlock_lock, base.WorldCenter));
		}
		animation.Play();
		SendMessage(new PlayWorldSoundMessage((Tier == ChestTier.Present) ? SoundName.present_unwrap : SoundName.chest_open, base.WorldCenter));
		Loot();
	}

	private void Loot()
	{
		if (Looted)
		{
			return;
		}
		PlayerEntity player = base.core.CurrentPlayState.Player;
		if (player != null && !player.Dead)
		{
			if (Contents != null && Contents.Item == ItemType.GoldCoin)
			{
				player.CollectCoins(Contents.Count, this, Color.White);
				base.core.ParticleManager.MakeItemToHudEmitter(base.WorldCenter, ItemType.GoldCoin, many: true).AttachTo(this).Delay(20)
					.Emit((int)Component._m(Contents.Count, 5f));
			}
			Looted = true;
			_inc(Stat.ChestsLooted);
		}
	}

	public void SpawnCoinsFromValue(int reward)
	{
		int num = reward;
		int num2 = reward;
		int num3 = 0;
		num3 += reward / 4;
		reward %= 4;
		num3 += reward / 3;
		reward %= 3;
		num3 += reward / 2;
		reward %= 2;
		num3 += reward;
		int num4 = 20;
		if (num < num4)
		{
			num3 = num;
		}
		for (int i = 0; i < num3; i++)
		{
			int num5 = ((num2 >= 4) ? 4 : ((num2 >= 3) ? 3 : ((num2 < 2) ? 1 : 2)));
			if (num < num4)
			{
				num5 = 1;
			}
			ItemEntity itemEntity = new ItemEntity(base.WorldCenterCoordinates.X, base.WorldCenterCoordinates.Y, ItemEntity.ValueToType(num5));
			num2 -= num5;
			SendMessage(new SpawnEntityMessage(itemEntity, null));
			if (base.core.CurrentPlayState.Player is IchitakaChar ichitakaChar)
			{
				ichitakaChar.AddDancingCoin(itemEntity);
			}
		}
	}

	public override void Break(Entity offender)
	{
		if ((offender is IchitakaChar && Contents == null) || lockStrength > 0)
		{
			return;
		}
		SendMessage(new PlayWorldSoundMessage((Tier == ChestTier.Present) ? SoundName.present_tear : SoundName.chest_break, base.WorldCenter));
		if (Tier == ChestTier.Present)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_shred_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_shred_2), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_shred_3), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_ribbon_1), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_ribbon_2), null));
			if (!IsBroken)
			{
				SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.present_bow), null));
			}
		}
		else
		{
			if (Tier == ChestTier.Gold)
			{
				for (int i = 1; i <= 4; i++)
				{
					SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_1), null));
				}
			}
			for (int j = 1; j <= 3; j++)
			{
				SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_2), null));
				SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, SpriteName.chest_board_2), null));
			}
		}
		SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCenterCoordinates, (Tier == ChestTier.Gold) ? SpriteName.chest_lid : ((Tier == ChestTier.Wood) ? SpriteName.chest_lid_wood : SpriteName.present_top), 60, new Vector4(Component._rnd(-0.1f, 0.1f), Component._rnd(-0.1f, 0.1f), 2.5f, 0.02f), "", 19f), null));
		IsBroken = true;
		if (offender is IchitakaChar && Contents != null)
		{
			SpawnCoinsFromValue(Contents.Count);
			Looted = true;
		}
		else
		{
			Loot();
		}
		base.Break(offender);
	}
}
