using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter;

public class GameOverAction : Component
{
	public readonly GameOverActionType Type;

	public readonly Character Character;

	public float Top = -70f;

	public float Left = -40f;

	public Sprite Pic;

	public bool SmallPic;

	public Color GlowColor;

	public string Text;

	public string DoneText;

	public string DoneLabel;

	public bool Done;

	public string Label;

	public bool YellowButton;

	public bool HasTextButton;

	public string TextButtonText;

	private List<Sprite> comingSoonSprites;

	private BagOf<string> hints;

	private Dictionary<InjuryType, string> deathHints;

	private Animation doubler;

	public GameOverAction(GameOverActionType type, Character character)
	{
		hints = new BagOf<string>();
		hints.Put(__(SId.ACTION_hint_control_schemes));
		hints.Put(__(SId.ACTION_hint_stats));
		hints.Put(__(SId.ACTION_hint_control_options));
		hints.Put(__(SId.ACTION_hint_swipe_and_hold));
		hints.Put(__(SId.ACTION_hint_control_see_through));
		hints.Put(__(SId.ACTION_hint_hold_to_run));
		hints.Put(__(SId.ACTION_hint_getting_gold));
		hints.Put(__(SId.ACTION_hint_upgrades));
		hints.Put(__(SId.ACTION_hint_screenshots));
		hints.Put(__(SId.ACTION_hint_premium));
		deathHints = new Dictionary<InjuryType, string>
		{
			{
				InjuryType.Bat,
				__(SId.ACTION_hint_death_bat)
			},
			{
				InjuryType.Bolt,
				__(SId.ACTION_hint_death_crossbow)
			},
			{
				InjuryType.Crushed,
				__(SId.ACTION_hint_death_piston)
			},
			{
				InjuryType.Fall,
				__(SId.ACTION_hint_death_fall)
			},
			{
				InjuryType.Saw,
				__(SId.ACTION_hint_death_saw)
			},
			{
				InjuryType.Slime,
				__(SId.ACTION_hint_death_slime)
			},
			{
				InjuryType.Spikes,
				__(SId.ACTION_hint_death_spikes)
			},
			{
				InjuryType.Sword,
				__(SId.ACTION_hint_death_sword)
			},
			{
				InjuryType.Timeout,
				__(SId.ACTION_hint_death_darkness)
			},
			{
				InjuryType.Zap,
				__(SId.ACTION_hint_death_zap)
			},
			{
				InjuryType.Axe,
				__(SId.ACTION_hint_death_statue)
			},
			{
				InjuryType.Flame,
				__(SId.ACTION_hint_death_fire)
			},
			{
				InjuryType.Follower,
				__(SId.ACTION_hint_death_follower)
			},
			{
				InjuryType.DeadBattery,
				__(SId.ACTION_hint_death_battery)
			}
		};
		Type = type;
		Character = character;
		Done = false;
		SmallPic = false;
		HasTextButton = false;
		int num = -1;
		comingSoonSprites = new List<Sprite>
		{
			_(SpriteName.coming_soon_1),
			_(SpriteName.coming_soon_2),
			_(SpriteName.coming_soon_3),
			_(SpriteName.coming_soon_4),
			_(SpriteName.coming_soon_5)
		};
		switch (Type)
		{
		case GameOverActionType.ComingSoon:
			Label = "Stay tuned";
			break;
		case GameOverActionType.Like:
			Label = __(SId.ACTION_like_us) + " +^" + 200;
			GlowColor = default(Color).FromRgb(23973) * 0.7f;
			Pic = _(SpriteName.go_facebook);
			Text = __(SId.ACTION_nitrome_facebook_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = string.Format(__(SId.ACTION_please_accept), 200);
			break;
		case GameOverActionType.LikeEneminds:
			Label = __(SId.ACTION_like_us) + " +^" + 200;
			GlowColor = default(Color).FromRgb(23973) * 0.7f;
			Pic = _(SpriteName.go_facebook);
			Text = __(SId.ACTION_eneminds_facebook_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = string.Format(__(SId.ACTION_please_accept), 200);
			break;
		case GameOverActionType.Follow:
			Label = __(SId.ACTION_follow_us) + " +^" + 200;
			GlowColor = default(Color).FromRgb(26808) * 0.7f;
			Pic = _(SpriteName.go_twitter);
			Text = __(SId.ACTION_nitrome_twitter_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = string.Format(__(SId.ACTION_please_accept), 200);
			break;
		case GameOverActionType.FollowEneminds:
			Label = __(SId.ACTION_follow_us) + " +^" + 200;
			GlowColor = default(Color).FromRgb(26808) * 0.7f;
			Pic = _(SpriteName.go_twitter);
			Text = __(SId.ACTION_eneminds_twitter_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = string.Format(__(SId.ACTION_please_accept), 200);
			break;
		case GameOverActionType.Feedback:
			Label = __(SId.ACTION_write_us);
			GlowColor = default(Color).FromRgb(12224879) * 0.7f;
			Pic = _(SpriteName.go_feedback);
			Text = __(SId.ACTION_write_us_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = __(SId.ACTION_write_us_done);
			break;
		case GameOverActionType.Rate:
			Label = __(SId.ACTION_rate_us);
			GlowColor = default(Color).FromRgb(16430139) * 0.7f;
			Pic = _(SpriteName.go_star);
			Text = __(SId.ACTION_rate_us_desc);
			DoneLabel = __(SId.ACTION_thanks);
			DoneText = __(SId.ACTION_rate_us_done);
			break;
		case GameOverActionType.Upgrade:
		case GameOverActionType.Unlock:
		case GameOverActionType.UpgradeSoon:
		case GameOverActionType.UnlockSoon:
		{
			CharDescription charDescription = CharDescription.Get[character];
			GlowColor = charDescription.Color1 * 2f;
			Pic = _(charDescription.Portrait);
			SmallPic = true;
			switch (Type)
			{
			case GameOverActionType.Unlock:
				Label = __(SId.SHOP_unlock) + " ^" + CharDescription.Get[character].UnlockPrice;
				Text = string.Format(__(SId.ACTION_ready_to_unlock_char), __(charDescription.Name));
				DoneLabel = __(SId.ACTION_unlocked);
				DoneText = string.Format(__(SId.ACTION_try_playing_as_char), __(charDescription.Name));
				break;
			case GameOverActionType.UnlockSoon:
				Label = __(SId.SHOP_unlock) + " ^" + CharDescription.Get[character].UnlockPrice;
				Text = string.Format(__(SId.ACTION_coins_to_unlock), CharDescription.Get[character].UnlockPrice - base.core.ProfileData.Coins);
				DoneLabel = "-";
				DoneText = "-";
				YellowButton = true;
				break;
			case GameOverActionType.Upgrade:
				num = base.core.ProfileData.Characters[character].Level;
				if (num >= charDescription.Levels.Count)
				{
					Label = "-";
				}
				else
				{
					Label = __(SId.SHOP_uprgade) + " ^" + charDescription.Levels[num].Price;
				}
				Text = string.Format(__(SId.ACTION_upgrade_char_to_level), __(charDescription.Name), num + 1);
				DoneLabel = __(SId.ACTION_upgraded);
				DoneText = string.Format(__(SId.ACTION_new_powers), __(charDescription.Name));
				break;
			case GameOverActionType.UpgradeSoon:
				num = base.core.ProfileData.Characters[character].Level;
				Label = __(SId.SHOP_uprgade) + " ^" + charDescription.Levels[num].Price;
				Text = string.Format(__(SId.ACTION_coins_to_next_level), charDescription.Levels[num].Price - base.core.ProfileData.Coins);
				DoneLabel = "-";
				DoneText = "-";
				YellowButton = true;
				break;
			}
			break;
		}
		case GameOverActionType.WatchAd:
		{
			int optimalWatchAdReward = base.core.AdsManager.GetOptimalWatchAdReward();
			Label = string.Format(__(SId.ACTION_watch_ad) + " +^{0}", optimalWatchAdReward);
			GlowColor = default(Color).FromRgb(14523446) * 0.7f;
			Pic = _(SpriteName.go_ad);
			Text = __(SId.ACTION_watch_ad_desc);
			DoneLabel = __(SId.ACTION_done);
			DoneText = string.Format(__(SId.ACTION_youve_earned), optimalWatchAdReward);
			break;
		}
		case GameOverActionType.Offer1:
			Label = $"+^{Store.CoinsForOffer[Iap.Offer1]}" + " " + base.core.Store.GetPrice(Iap.Offer1);
			GlowColor = default(Color).FromRgb(5392517) * 0.7f;
			Pic = _(SpriteName.go_offer_1);
			Text = __(SId.ACTION_chest_small_desc);
			DoneLabel = __(SId.ACTION_done);
			DoneText = string.Format(__(SId.ACTION_youve_purchased), Store.CoinsForOffer[Iap.Offer1]);
			break;
		case GameOverActionType.Offer2:
			Label = $"+^{Store.CoinsForOffer[Iap.Offer2]}" + " " + base.core.Store.GetPrice(Iap.Offer2);
			GlowColor = default(Color).FromRgb(5392517) * 0.7f;
			Pic = _(SpriteName.go_offer_2);
			Text = __(SId.ACTION_chest_madium_desc);
			DoneLabel = __(SId.ACTION_done);
			DoneText = string.Format(__(SId.ACTION_youve_purchased), Store.CoinsForOffer[Iap.Offer2]);
			break;
		case GameOverActionType.Offer3:
			Label = $"+^{Store.CoinsForOffer[Iap.Offer3]}" + " " + base.core.Store.GetPrice(Iap.Offer3);
			GlowColor = default(Color).FromRgb(14523446) * 0.7f;
			Pic = _(SpriteName.go_offer_3);
			Text = __(SId.ACTION_chest_big_desc);
			DoneLabel = __(SId.ACTION_done);
			DoneText = string.Format(__(SId.ACTION_youve_purchased), Store.CoinsForOffer[Iap.Offer3]);
			break;
		case GameOverActionType.Doubler:
			Label = "2×^ " + base.core.Store.GetPrice(Iap.CoinDoubler);
			GlowColor = default(Color).FromRgb(6022911) * 0.2f;
			Pic = _(SpriteName.doubler_shadow);
			Text = __(SId.ACTION_coin_doubler_desc);
			DoneLabel = __(SId.ACTION_done);
			DoneText = __(SId.ACTION_coin_doubler_done);
			doubler = new Animation();
			doubler.Add("double", "doubler_", "1111123425678999999abcdefgggggghij");
			doubler.Play("double");
			break;
		case GameOverActionType.Hint:
		{
			BagOf<string> bagOf = hints.Clone();
			bagOf.Put(string.Format(__(SId.ACTION_HINTTITLE_random_fact) + "|" + __(SId.ACTION_FACT_run_count) + "[facts]", base.core.LocaleManager.GetOrdinal(_stat(Stat.Attempts))));
			bagOf.Put(string.Format(__(SId.ACTION_HINTTITLE_random_fact) + "|" + __(SId.ACTION_FACT_walked_total) + "[facts]", _stat(Stat.MetersWalked)));
			int num2 = _stat(Stat.TicksInGame) / 216000;
			int num3 = _stat(Stat.TicksInGame) % 216000 / 3600;
			int num4 = _stat(Stat.TicksInGame) % 3600 / 60;
			string text = "";
			if (num2 > 0)
			{
				text = text + num2 + __(SId.MISC_hours) + " ";
			}
			if (num3 > 0)
			{
				text = text + num3 + __(SId.MISC_minutes) + " ";
			}
			if (num4 > 0)
			{
				text = text + num4 + __(SId.MISC_seconds);
			}
			bagOf.Put(string.Format(__(SId.ACTION_HINTTITLE_random_fact) + "|" + __(SId.ACTION_FACT_total_playtime) + "[facts]", text));
			InjuryType causeOfDeath = base.core.CurrentPlayState.Session.CauseOfDeath;
			if (Achievements.CauseOfDeathStat.ContainsKey(causeOfDeath))
			{
				int n = _stat(Achievements.CauseOfDeathStat[causeOfDeath]);
				if (deathHints.ContainsKey(causeOfDeath))
				{
					bagOf.Put(__(SId.ACTION_HINTTITLE_random_fact) + "|" + string.Format(deathHints[causeOfDeath], base.core.LocaleManager.GetOrdinal(n)) + "[deaths]");
				}
			}
			string[] array = bagOf.Draw().Split('|');
			Label = array[0];
			Text = array[1];
			if (Text.EndsWith("[facts]", StringComparison.InvariantCulture))
			{
				Text = Text.Replace("[facts]", "");
				TextButtonText = __(SId.ACTION_more_facts);
				HasTextButton = true;
			}
			if (Text.EndsWith("[deaths]", StringComparison.InvariantCulture))
			{
				Text = Text.Replace("[deaths]", "");
				TextButtonText = __(SId.ACTION_death_stats);
				HasTextButton = true;
			}
			break;
		}
		}
	}

	public override void Update()
	{
		if (doubler != null)
		{
			doubler.Update();
		}
		base.Update();
	}

	public void Draw(Vector2 pos)
	{
		if (Type == GameOverActionType.Hint)
		{
			base.core.Renderer["fg", 1, false].DrawTextS(Label.ToUpper(), new Vector2(base.core.Renderer.ScreenCenter.X, pos.Y - 25f), TextProfile.OrangeBoldText.Alter(null, default(Color).FromRgb(1514280), TextDecoration.Extrude1, textAlignment: Alignment2D.Center, boxAlignment: Alignment2D.Center, width: 130, height: 25, font: Font.Bold, scale: 0.75f));
			base.core.Renderer["fg", 1, false].DrawTextS(Text, new Vector2(base.core.Renderer.ScreenCenter.X, pos.Y - 21f), TextProfile.OrangeBoldText.Alter(default(Color).FromRgb(7109247), default(Color).FromRgb(1514280), TextDecoration.Extrude1, textAlignment: Alignment2D.Middle, boxAlignment: Alignment2D.Center, width: 130, height: 50 - (HasTextButton ? 10 : 0), font: Font.Thin, scale: 0.75f));
		}
		else if (Type == GameOverActionType.ComingSoon)
		{
			int num = 0;
			foreach (Sprite comingSoonSprite in comingSoonSprites)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(comingSoonSprite, pos.Shift(67.5f, 5f + ((num > 0) ? (3f * Component._sin((float)(base.ticks + num * 40) * 0.02f)) : 0f)), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				num++;
			}
			int num2 = 70;
			int num3 = 15;
			base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.coming_soon_lock), pos.Shift((float)(num2 - 4) + 2.5f, num3 - 25 + 2), null, null, (float)Math.Sin((float)base.ticks * 0.05f) * 0.2f, SpriteFlip.None, SpriteOrigin.TopCenter);
			base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.coming_soon_lock), pos.Shift((float)(num2 - 28) + 2.5f, num3 - 30 + 2), Color.White * 0.5f, null, (float)Math.Sin((float)(base.ticks + 20) * 0.05f) * 0.2f, SpriteFlip.None, SpriteOrigin.TopCenter);
			base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.coming_soon_lock), pos.Shift((float)(num2 + 18) + 2.5f, num3 - 30 + 2), Color.White * 0.5f, null, (float)Math.Sin((float)(base.ticks + 40) * 0.05f) * 0.2f, SpriteFlip.None, SpriteOrigin.TopCenter);
			base.core.Renderer["fg", 1, false].DrawTextS((base.ticks % 180 < 90) ? "STAY TUNED" : "NEW STUFF", new Vector2(base.core.Renderer.ScreenCenter.X, pos.Y + 21f), TextProfile.OrangeBoldText.Alter(null, default(Color).FromRgb(1514280), TextDecoration.Extrude2, textAlignment: Alignment2D.Center, boxAlignment: Alignment2D.Center, width: 130, height: 25, font: Font.Bold, scale: 0.75f));
		}
		else
		{
			int num4 = 30;
			base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.glow_huge), pos.Shift(num4, -14f), GlowColor, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			if ((Type == GameOverActionType.Upgrade || Type == GameOverActionType.UpgradeSoon) && !Done)
			{
				float num5 = (float)(base.ticks % 100) / 100f;
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.go_upgrade_arrow), pos.Shift(num4, 17f - 15f * num5), GlowColor * Component._sin(num5 * (float)Math.PI) * 0.5f, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
				if (Type == GameOverActionType.Upgrade)
				{
					num5 = (float)((base.ticks + 33) % 66) / 66f;
					base.core.Renderer["fg", 2, false].DrawSpriteS(_(SpriteName.go_upgrade_arrow), pos.Shift(num4 - 15, 17f - 10f * num5), GlowColor * Component._sin(num5 * (float)Math.PI) * 0.5f, Vector2.One * 0.5f, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
					num5 = (float)((base.ticks + 66) % 66) / 66f;
					base.core.Renderer["fg", 2, false].DrawSpriteS(_(SpriteName.go_upgrade_arrow), pos.Shift(num4 + 15, 17f - 10f * num5), GlowColor * Component._sin(num5 * (float)Math.PI) * 0.5f, Vector2.One * 0.5f, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
				}
			}
			float num6 = (SmallPic ? 0.75f : 1f);
			bool flag = Type == GameOverActionType.Unlock || Type == GameOverActionType.UnlockSoon;
			base.core.Renderer["fg", 1, false].DrawSpriteS(Pic, pos.Shift(num4, 7f) - Pic.Link * num6, (flag && !Done) ? Color.Black : Color.White, Vector2.One * num6);
			if (Type == GameOverActionType.Doubler)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(doubler.GetCurrentFrame(), pos.Shift(0f, -22f + Component._sin((float)base.ticks * 0.04f) * Component._sin((float)base.ticks * 0.04f)));
			}
			if (flag && !Done)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.icon_lock), pos.Shift(num4, 7f - (float)Pic.Height * 0.75f * 0.5f), null, rotation: Component._sin((float)base.ticks * 0.07f) * 0.1f, scale: Vector2.One * 0.75f, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
			}
			base.core.Renderer["fg", 1, false].DrawTextS(Text, new Vector2(base.core.Renderer.ScreenCenter.X - 3f, pos.Y + 8f), TextProfile.OrangeBoldText.Alter(default(Color).FromRgb(7109247), null, TextDecoration.None, textAlignment: Alignment2D.Middle, boxAlignment: Alignment2D.Center, width: 130, height: 25, font: Font.Thin, scale: 0.75f));
		}
		base.Draw();
	}
}
