using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class MenuState : State
{
	protected override bool ShowContextPromptLegend => false;

	private enum Button
	{
		Start,
		Shop,
		Options,
		Scores,
		Achievements,
		Share,
		Action,
		TextAction,
		SubMenu,
		NoAds,
		MoreNitrome,
		GPlay
	}

	private enum TitleTrans
	{
		None,
		In,
		Out,
		ToGame
	}

	private TitleTrans titleTrans;

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private readonly SessionData lastSession;

	private string shareDeathMessage;

	private bool topScore;

	private bool firstScore;

	private bool startedCurrentGame;

	private bool hideScreenshot;

	private readonly Sprite screenshotSprite;

	private GameOverAction gameOverAction;

	private bool afterAd;

	private Animation titleFlameL;

	private Animation titleFlameR;

	private Animation titleBat1;

	private Animation titleBat2;

	private int batSoundDelay = 30;

	private int newItemsCount;

	private RectangleF ComingSoonBounds => new RectangleF(R.ScreenCenter.X - 85, R.ScreenHeight - 140, 170, 27);

	public bool GameOver => lastSession != null;

	public MenuState(SessionData endedSession)
	{
		lastSession = endedSession;
		ShowCoins = GameOver;
		if (!GameOver)
		{
			titleTrans = TitleTrans.In;
			base.TransDuration = 120;
		}
		else
		{
			base.TransDuration = 20;
		}
		if (base.core.JustWatchedAd)
		{
			afterAd = true;
			base.core.JustWatchedAd = false;
		}
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 1000);
		int num = base.core.Renderer.ScreenHeight - 40;
		float num2 = (float)(base.core.Renderer.ScreenWidth - 22) / 4f;
		if (GameOver)
		{
			touchMenu.SetupButton(Button.Shop, new RectangleF(10f, num, num2, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", null, icon: false, iconIsPicture: false, blink: false, null, null, labelAnim: base.core.CurrentCharDesc.AnimSequence, labelAnimSpeed: base.core.CurrentCharDesc.AnimSpeed * base.core.CurrentCharDesc.ButtonAnimSpeedFactor, yShift: (base.core.ProfileData.Character == Character.Creep) ? (-9.5f) : (-6f), xShift: 0f, fontSize: 1f, drawShadow: true);
			touchMenu.SetupButton(Button.Options, new RectangleF(12f + num2 * 3f, num, num2, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_options));
			touchMenu.SetupButton(Button.Start, new RectangleF(10f + num2 + 1f, num, num2 * 2f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_restart), icon: true, iconIsPicture: false, blink: true);
		}
		else
		{
			touchMenu.SetupButton(Button.Shop, new RectangleF(base.core.Renderer.ScreenCenter.X - 25f, num, 50f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", null, icon: false, iconIsPicture: false, blink: false, null, null, labelAnim: base.core.CurrentCharDesc.AnimSequence, labelAnimSpeed: base.core.CurrentCharDesc.AnimSpeed * base.core.CurrentCharDesc.ButtonAnimSpeedFactor, yShift: (base.core.ProfileData.Character == Character.Creep) ? (-9.5f) : (-6f), xShift: 0f, fontSize: 1f, drawShadow: true);
			float menuMargin = Settings.IsTouchDevice && base.core.Renderer.ScreenHeight > base.core.Renderer.ScreenWidth ? 6f : 0f;
			touchMenu.SetupButton(Button.SubMenu, new RectangleF(base.core.Renderer.ScreenWidth - 29f, menuMargin + base.topSafeArea, 23f, 23f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Purple, "", _(SpriteName.icon_submenu), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.piston_extend);
			touchMenu.SetupButton(Button.GPlay, new RectangleF(0f, 0f, 29f, 26f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Purple, "", _(SpriteName.icon_gplay));
			touchMenu[Button.GPlay].Hidden = true;
		}
		if (GameOver)
		{
			touchMenu.SetupButton(Button.Share, new RectangleF(12f + 3f * num2, num - 32 - 20, num2, 50f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.None, "", _(Settings.ShareIcon), icon: true, iconIsPicture: false, blink: false, null, null, 7f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.paper_touch, SoundName.paper);
			touchMenu.SetupButton(Button.Achievements, new RectangleF(10f + 1f * num2, num - 32, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_achievements));
			touchMenu.SetupButton(Button.Scores, new RectangleF(10f, num - 32, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_scores), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.button_up, true);
			touchMenu.SetupButton(Button.GPlay, new RectangleF(10f + 2f * num2, num - 32, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_gplay));
		}
		if (GameOver)
		{
			topScore = lastSession.Distance > base.core.ProfileData.BestDistance;
			firstScore = _stat(Stat.Attempts) == 1;
			shareDeathMessage = lastSession.Distance.ToString();
			screenshotSprite = base.core.SpriteManager.MakeFullSpriteFromScreenshot(base.core.GameplayScreenshot);
			int num3 = screenshotSprite.Width / 5;
			int num4 = (screenshotSprite.Height - num3 * 3) / 2;
			screenshotSprite = screenshotSprite.Reduce(num3, num4, num3, num4);
			SelectGameOverAction();
			if (gameOverAction.Type != GameOverActionType.Hint && gameOverAction.Type != GameOverActionType.ComingSoon)
			{
				SetupActionButton();
			}
			if (gameOverAction.HasTextButton)
			{
				SetupTextActionButton();
			}
			InitPromoButtons();
		}
		else
		{
			titleFlameL = new Animation(0.15f).Add("burn", "title_flame_", "1234").Play("burn");
			titleFlameR = new Animation(0.15f).Add("burn", "title_flame_", "3412").Play("burn");
			titleBat1 = new Animation(0.15f).Add("live", "title_bat_", "1234").Play("live");
			titleBat2 = new Animation(0.15f).Add("live", "title_bat_", "3412").Play("live");
		}
		CountNewItems();
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			Button.Start, Button.Shop, Button.Options, Button.Scores,
			Button.Achievements, Button.GPlay, Button.Share, Button.Action, Button.TextAction);
	}

	private void HideExternalButtons()
	{
		touchMenu[Button.GPlay].Hidden = true;
		if (!GameOver) return;
		foreach (Button b in new[] { Button.NoAds, Button.MoreNitrome, Button.Scores, Button.Achievements, Button.Share })
			touchMenu[b].Hidden = true;
	}

	private void InitPromoButtons()
	{
		touchMenu.SetupButton(Button.NoAds, new RectangleF(-100f, -100f, 74f, 27f), _(SpriteName.button_no_ads), _(SpriteName.button_no_ads_down));
		touchMenu[Button.NoAds].Hidden = base.core.ProfileData.AdsRemoved;
		if (base.core.ProfileData.AdsRemoved)
		{
			touchMenu.SetupButton(Button.MoreNitrome, new RectangleF(-200f, -200f, 147f, 27f), _(SpriteName.button_nitrome_big), _(SpriteName.button_nitrome_big_down));
		}
		else
		{
			touchMenu.SetupButton(Button.MoreNitrome, new RectangleF(-100f, -100f, 74f, 27f), _(SpriteName.button_nitrome), _(SpriteName.button_nitrome_down));
		}
	}

	private void CountNewItems()
	{
		List<int> list = new List<int>();
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			CharDescription charDescription = CharDescription.Get[value];
			int num = (base.core.ProfileData.Characters[value].Unlocked ? base.core.ProfileData.Characters[value].Level : 0);
			if (num < charDescription.Levels.Count)
			{
				int price = charDescription.Levels[num].Price;
				list.Add(price);
			}
		}
		list.Sort();
		newItemsCount = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] <= base.core.ProfileData.Coins)
			{
				newItemsCount++;
			}
		}
	}

	public override void Load()
	{
		Screen(GameOver ? ("game-over " + SciHelper.GetVerboseRange(lastSession.Distance, 20)) : "menu");
		if (GameOver)
		{
			SendMessage(new PlaySoundMessage(SoundName.trans_2));
			base.core.CurrentPlayState.Camera.ZoomBox.SetFixed("game over", 1.6f, inWorld: false, 0.02f);
		}
		base.Load();
	}

	public override void Update()
	{
		if (base.IsTopState && !GameOver)
		{
			if (!base.core.CrossPromotion.Shown && base.core.CrossPromotion.CanShow && !base.core.ProfileData.AdsRemoved && !base.core.CrossPromotion.Disabled)
			{
				SendMessage(new CoreEventMessage(CoreEvent.Promo));
				return;
			}
			if (base.core.ProfileData.LanguageSelectorPending)
			{
				SendMessage(new PushStateMessage(new LanguageSelectorState(quickMode: true)));
				return;
			}
		}
		if (GameOver)
		{
			touchMenu[Button.Scores].SeeThrough = true;
			base.core.AudioManager.MusicVolumeBox.Set("game over", 0.3f, inWorld: false);
			IsOpaque = Transition == TransType.None;
			if (gameOverAction != null)
			{
				gameOverAction.Update();
			}
		}
		else
		{
			titleFlameL.Update();
			titleFlameR.Update();
			titleBat1.Update();
			titleBat2.Update();
			if (titleTrans != TitleTrans.ToGame)
			{
				base.core.CurrentPlayState.Camera.ZoomBox.Set("menu", 0.7f, inWorld: false, 1f);
				base.core.CurrentPlayState.Camera.YOffsetBox.Set("menu", 20f, inWorld: false, 1f);
			}
			if (titleTrans == TitleTrans.In && !base.core.AudioManager.IsPlayingMusic && base.Trans >= base.TransDuration - 35)
			{
				string name = "intro";
				if (base.core.Holiday == Holiday.ChunJie)
				{
					name = "intro_chunjie";
				}
				base.core.AudioManager.PlayMusic(name);
			}
			if (Transition == TransType.None && base.IsTopState)
			{
				batSoundDelay--;
				if (batSoundDelay == 0)
				{
					SendMessage(new PlaySoundMessage(BatEntity.Squeaks.DrawDifferent(), 1f, 0f, Component._rnd(-0.5f, 0.5f)));
					batSoundDelay = Component._rnd(50, 120);
				}
			}
		}
		touchMenu.Update();
		if (!GameOver && base.TicksInState == 20)
		{
			SoundName name2 = SoundName.title_reveal;
			switch (base.core.Holiday)
			{
			case Holiday.Xmas:
				name2 = SoundName.title_hohoho;
				break;
			case Holiday.ChunJie:
				name2 = SoundName.title_gong;
				break;
			}
			SendMessage(new PlaySoundMessage(name2));
			base.core.AudioManager.StopMusic();
		}
		base.Update();
	}

	public override void OnReturn()
	{
		titleTrans = TitleTrans.In;
		hideScreenshot = false;
		int num = base.core.Renderer.ScreenHeight - 40;
		float width = (float)(base.core.Renderer.ScreenWidth - 22) / 4f;
		touchMenu.SetupButton(Button.Shop, GameOver ? new RectangleF(10f, num, width, 30f) : new RectangleF(base.core.Renderer.ScreenCenter.X - 25f, num, 50f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", null, icon: false, iconIsPicture: false, blink: false, null, null, labelAnim: base.core.CurrentCharDesc.AnimSequence, labelAnimSpeed: base.core.CurrentCharDesc.AnimSpeed * base.core.CurrentCharDesc.ButtonAnimSpeedFactor, yShift: (base.core.ProfileData.Character == Character.Creep) ? (-9.5f) : (-6f), xShift: 0f, fontSize: 1f, drawShadow: true);
		UpdateTransition();
		UpdateGameOverActionAfterReturn();
		if (GameOver)
		{
			if (Transition == TransType.In)
			{
				SendMessage(new PlaySoundMessage(SoundName.trans_2));
			}
			InitPromoButtons();
		}
		CountNewItems();
		base.OnReturn();
	}

	public override void UpdateTransition()
	{
		int screenHeight = base.core.Renderer.ScreenHeight;
		if (GameOver)
		{
			touchMenu[Button.Scores].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.Achievements].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.GPlay].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.Share].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.Shop].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.Options].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
			touchMenu[Button.Start].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, screenHeight, -screenHeight, base.TransDuration));
		}
		else if (titleTrans != TitleTrans.In)
		{
			float num = Component._M(base.Trans - (base.TransDuration - 15), 0f);
			touchMenu[Button.Shop].Rectangle.Shift(0f, (float)Tween.CircEaseOut(num, screenHeight, -screenHeight, 15.0));
			touchMenu[Button.SubMenu].Rectangle.Shift(0f, (float)Tween.CircEaseOut(num, -screenHeight, screenHeight, 15.0));
			touchMenu[Button.GPlay].Rectangle.Shift(0f, (float)Tween.CircEaseOut(num, -screenHeight, screenHeight, 15.0));
		}
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		HideExternalButtons();
		// A tela de título não possui um botão visual de "Start" no TouchMenu:
		// qualquer toque inicia a partida. Mantemos a mesma regra para A/Enter,
		// sem deixar o foco cair acidentalmente em Shop/SubMenu.
		if (!GameOver && Transition == TransType.None && base.core.Input.WasPressed(GameAction.Confirm))
		{
			OnButtonRelease(Button.Start);
			return;
		}
		if (!GameOver && Transition == TransType.None && base.core.Input.WasPressed(GameAction.Secondary))
		{
			OnButtonRelease(Button.Shop);
			return;
		}
		bool flag = false;
		if (base.Trans >= base.TransDuration)
		{
			flag = touchMenu.HandleInput();
			if (GameOver && !flag)
			{
				flag = menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
			}
		}
		if (Transition != TransType.None || flag)
		{
			return;
		}
		if (!GameOver)
		{
			foreach (TouchLocation item in base.core.TouchState)
			{
				if (item.State == TouchLocationState.Pressed && !ComingSoonBounds.Contains(item.Position))
				{
					OnButtonRelease(Button.Start);
					break;
				}
			}
		}
		base.HandleInput();
	}

	public override void Draw()
	{
		HideExternalButtons();
		if (!GameOver && Transition == TransType.None)
		{
			MenuTheme.Button(R, ComingSoonBounds, 1200, opacity: 0.45f);
			R["fg", 1204, false].DrawTextS(__(SId.PC_mode_soon), ComingSoonBounds.Center,
				TextProfile.OrangeBoldText.Alter(width: 160, height: 22, scale: 0.55f, color: Color.Wheat * 0.75f,
				boxAlignment: Alignment2D.Middle, textAlignment: Alignment2D.Middle));
		}
		float num = ((titleTrans == TitleTrans.Out) ? 0f : (1f - (float)base.Trans / (float)base.TransDuration));
		if (Transition == TransType.Out && startedCurrentGame)
		{
			num = 0f - num;
		}
		if (Transition == TransType.In && GameOver)
		{
			num = 0f;
		}
		base.core.Renderer["fg", -1000, false].FillScreen(Color.Black * (GameOver ? 1f : (0.6f + num)));
		float num2 = base.core.Renderer.ScreenHeight;
		if (GameOver)
		{
			Vector2 v = new Vector2(0f, 0.25f * (float)(base.core.Renderer.ScreenHeight - 75 - 130));
			num2 = ((Transition == TransType.None) ? 0f : ((float)Tween.CircEaseOut(base.Trans, num2, 0f - num2, base.TransDuration)));
			num2 += (float)base.topSafeArea;
			float num3 = 1f;
			string text = lastSession.Distance + __(SId.MISC_meters);
			float num4 = 0f;
			string text2 = text;
			foreach (char c in text2)
			{
				Sprite sprite = base.core.SpriteManager.GetSprite("score_digit_" + c);
				num4 += (float)(sprite.Width - 3) * num3;
			}
			float x = ((float)base.core.Renderer.ScreenWidth - num4) * 0.5f;
			v = v.Shift(x, 4f);
			if (topScore)
			{
				for (int j = 0; j < 8; j++)
				{
					base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.ray_huge), new Vector2(base.core.Renderer.ScreenCenter.X, v.Y + 15f + num2), default(Color).FromRgb(16298824) * 0.4f * (0.3f + Component._sin((float)base.ticks * 0.01f + (float)(j - 2) * (float)Math.PI * 2f / 8f) * 0.7f), rotation: (float)j * (float)Math.PI * 2f / 8f + (float)base.ticks * 0.01f, scale: Vector2.One * 1f * (0.9f + 0.1f * Component._sin((float)base.ticks * 0.05f + (float)(j * 2))) * 0.7f, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
				}
			}
			int num5 = 0;
			text2 = text;
			foreach (char c2 in text2)
			{
				float num6 = (topScore ? (Component._cos((float)(base.ticks + num5 * 10) * 0.13f) * 2f * 1.5f) : 0f);
				Sprite sprite2 = base.core.SpriteManager.GetSprite("score_digit_" + c2);
				base.core.Renderer["fg", 1, false].DrawSpriteS(sprite2, v.Shift(0f, num6 + num2), null, new Vector2(num3));
				v = v.Shift((float)(sprite2.Width - 3) * num3, 0f);
				num5++;
			}
			v = v.Shift(0f, 34.5f + num2);
			v.X = (base.core.Renderer.ScreenWidth - 145) / 2 + 5;
			string text3 = __(SId.GAMEOVER_best) + " " + base.core.ProfileData.BestDistance + __(SId.MISC_meters);
			if (topScore)
			{
				text3 = ((!firstScore) ? __(SId.GAMEOVER_new_best) : __(SId.GAMEOVER_not_bad));
			}
			base.core.Renderer["fg", 1, false].DrawTextS(text3, new Vector2(10f, v.Y + 3f), TextProfile.OrangeBoldText.Alter(default(Color).FromRgb(10659498), null, TextDecoration.None, textAlignment: Alignment2D.Center, boxAlignment: Alignment2D.Left, width: base.core.Renderer.ScreenWidth - 20, height: null, font: Font.Bold, scale: 0.75f));
			v = v.Shift(0f, 10f);
			v = v.Shift(0f, 49f);
			gameOverAction.Top = v.Y;
			gameOverAction.Left = v.X - 7f;
			touchMenu[Button.MoreNitrome].Rectangle.X = gameOverAction.Left + 1f;
			touchMenu[Button.NoAds].Rectangle.X = gameOverAction.Left + 74f;
			touchMenu[Button.MoreNitrome].Rectangle.Y = gameOverAction.Top + 31f;
			touchMenu[Button.NoAds].Rectangle.Y = touchMenu[Button.MoreNitrome].Rectangle.Y;
			if (gameOverAction.Type != GameOverActionType.Hint && gameOverAction.Type != GameOverActionType.ComingSoon)
			{
				touchMenu[Button.Action].Rectangle.X = gameOverAction.Left + 65f;
				touchMenu[Button.Action].Rectangle.Y = gameOverAction.Top - 23f;
			}
			if (gameOverAction.HasTextButton)
			{
				touchMenu[Button.TextAction].Rectangle.X = gameOverAction.Left + 14f;
				touchMenu[Button.TextAction].Rectangle.Y = gameOverAction.Top + 12f;
			}
			Sprite sprite3 = _(SpriteName.go_shelf);
			base.core.Renderer["fg", 1, false].DrawSpriteS(sprite3, new Vector2(base.core.Renderer.ScreenCenter.X, v.Y), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			gameOverAction.Draw(v);

		}
		else
		{
			if (num < 0.3f || titleTrans != TitleTrans.In)
			{
				int num7 = 80;
				Vector2 v2 = new Vector2(base.core.Renderer.ScreenCenter.X, (float)(base.core.Renderer.ScreenHeight - num7) * 0.1f + (float)base.topSafeArea);
				if (titleTrans != TitleTrans.In)
				{
					v2 = v2.Shift(0f, num * (float)base.core.Renderer.ScreenHeight);
				}
				base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_re), v2.Shift(0f, 25f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_dungeon), v2.Shift(0f, 64f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_dungeon_glow), v2.Shift(0f, 64f), Color.White * (0.7f + 0.3f * Component._sin((float)base.TicksInState * 0.1f)), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
				if (base.core.Holiday == Holiday.Xmas)
				{
					base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_lights), v2.Shift(1f, 78f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					string text4 = "12481248124812481248005500aa005500aa8421842184218421842136c936c936c936c936c91111222244448888";
					int index = base.ticks / 8 % text4.Length;
					int num8 = Convert.ToInt32(text4[index].ToString() ?? "", 16);
					if ((num8 & 1) != 0)
					{
						base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_lights_1), v2.Shift(1f, 78f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
					if ((num8 & 2) != 0)
					{
						base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_lights_2), v2.Shift(1f, 78f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
					if ((num8 & 4) != 0)
					{
						base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_lights_3), v2.Shift(1f, 78f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
					if ((num8 & 8) != 0)
					{
						base.core.Renderer["fg", 1010, false].DrawSpriteS(_(SpriteName.title_lights_4), v2.Shift(1f, 78f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
				}
				else if (base.core.Holiday == Holiday.ChunJie)
				{
					base.R["fg", 1010, false].DrawSpriteS(_(SpriteName.chunjie_title_glow), v2.Shift(0f, 64f), Color.White * (0.7f + 0.3f * Component._sin((float)base.TicksInState * 0.05f)), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					base.R["fg", 1010, false].DrawSpriteS(_(SpriteName.chunjie_title_decoration), v2.Shift(0f, 95f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
					string text5 = "122133";
					base.R["fg", 1010, false].DrawSpriteS(_("chunjie_title_lantern_" + text5[base.ticks / 10 % 6]), v2.Shift(-71f, 81f));
					base.R["fg", 1010, false].DrawSpriteS(_("chunjie_title_lantern_" + text5[(2 + base.ticks / 10) % 6]), v2.Shift(38f, 81f));
					base.R["fg", 1009, false].DrawSpriteS(_(SpriteName.glow_huge), v2.Shift(-54f, 92f), default(Color).FromRgb(16631062) * (0.9f + Component._sin((float)base.ticks * 0.2f) * 0.05f), Vector2.One * (0.9f + Component._sin((float)base.ticks * 0.05f) * 0.05f), 0f, SpriteFlip.None, SpriteOrigin.Center);
					base.R["fg", 1009, false].DrawSpriteS(_(SpriteName.glow_huge), v2.Shift(55f, 92f), default(Color).FromRgb(16631062) * (0.6f + Component._sin((float)base.ticks * 0.2f) * 0.1f), Vector2.One * (0.9f + Component._sin((float)base.ticks * 0.05f) * 0.05f), 0f, SpriteFlip.None, SpriteOrigin.Center);
				}
				Vector2 vector2 = v2.Shift(57f * Component._sin((float)base.TicksInState * 0.03f), 20f + 15f * Component._sin((float)base.TicksInState * 0.05f));
				float num9 = (float)(((double)((float)base.TicksInState * 0.03f) + Math.PI / 2.0) % (Math.PI * 2.0) / (Math.PI * 2.0));
				int num10 = 1010 + ((num9 < 0.5f) ? 2 : (-2));
				float num11 = Component._sin(num9 * (float)Math.PI * 2f);
				float num12 = 0.7f + 0.3f * num11;
				base.core.Renderer["fg", num10, false].DrawSpriteS(titleBat1.GetCurrentFrame(), vector2, Color.Lerp(Color.White, Color.DimGray, (1f - num11) * (1f - num11)), Vector2.One * num12, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["fg", num10 - 1, false].DrawSpriteS(titleBat1.GetCurrentFrame(), vector2.Shift(0f, -5f * num11), Color.Black * (0.4f * (num11 - 0.2f)), Vector2.One * (num12 * (1f + 0.2f * num11)), 0f, SpriteFlip.None, SpriteOrigin.Center);
				vector2 = v2.Shift(57f * Component._sin((float)base.TicksInState * 0.03f + (float)Math.PI), 20f + 15f * Component._sin((float)base.TicksInState * 0.05f + (float)Math.PI));
				num9 = (float)(((double)((float)base.TicksInState * 0.03f) + 4.71238898038469) % (Math.PI * 2.0) / (Math.PI * 2.0));
				num10 = 1010 + ((num9 < 0.5f) ? 2 : (-2));
				num11 = Component._sin(num9 * (float)Math.PI * 2f);
				num12 = 0.7f + 0.3f * num11;
				base.core.Renderer["fg", num10, false].DrawSpriteS(titleBat1.GetCurrentFrame(), vector2, Color.Lerp(Color.White, Color.DimGray, (1f - num11) * (1f - num11)), Vector2.One * num12, 0f, SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["fg", num10 - 1, false].DrawSpriteS(titleBat1.GetCurrentFrame(), vector2.Shift(0f, -5f * num11), Color.Black * (0.4f * (num11 - 0.2f)), Vector2.One * (num12 * (1f + 0.2f * num11)), 0f, SpriteFlip.None, SpriteOrigin.Center);
				int num13 = 0;
				if (base.core.Holiday == Holiday.ChunJie)
				{
					num13 = 50;
				}
				if (base.TicksInState / 30 % 2 == 0)
				{
					base.core.Renderer["fg", 1010, false].DrawTextS(__(SId.TITLE_tap_to_start), v2.Shift(0f, 90 + num13), TextProfile.OrangeBoldText.Alter(width: base.core.Renderer.ScreenWidth - 10, color: Color.White, secondColor: Color.Black, decoration: TextDecoration.Contour));
				}
			}
			float num14 = ((titleTrans == TitleTrans.ToGame) ? ((float)base.Trans / (float)base.TransDuration) : 1f);
			float num15 = 0.9f + ((titleTrans == TitleTrans.ToGame) ? ((1f - (float)base.Trans / (float)base.TransDuration) * 0.5f) : 0f);
			float num16 = ((titleTrans == TitleTrans.ToGame) ? (1f - (float)base.Trans / (float)base.TransDuration) : 0f);
			for (int k = 1; k <= 5; k++)
			{
				Sprite sprite4 = _("title_grue_l" + k);
				float num17 = 0.35f + 0.1f * (float)(k - 1);
				float num18 = 0.25f;
				float num19 = Component._m(Component._M(num - num17, 0f), num17 + num18) / num18;
				Vector2 position2 = new Vector2((float)(-sprite4.Width) * num19, (float)base.core.Renderer.ScreenHeight - (float)sprite4.Height * num15).Shift(-1.5f + 1.5f * Component._sin((float)(base.TicksInState + 50 * k) * 0.03f), Component._cos((float)(base.TicksInState + 25 * k) * 0.02f)).Shift((0f - num16) * (float)base.core.Renderer.ScreenWidth, num16 * (float)base.core.Renderer.ScreenHeight);
				base.core.Renderer["fg"].DrawSpriteS(sprite4, position2, Color.Lerp(Color.Black, Color.White, 0.7f + 0.1f * Component._sin((float)(base.TicksInState + k * 200) * (0.01f + (float)k * 0.01f))) * num14, Vector2.One * num15);
				base.core.Renderer["fg"].DrawSpriteS(_("title_bodyglow_l" + k), position2, Color.White * (0.3f * Component._sin((float)(base.TicksInState + k * 300) * 0.04f)) * num14, Vector2.One * num15);
				base.core.Renderer["fg"].DrawSpriteS(_("title_eyeglow_l" + k), position2, Color.White * Component._sin((float)(base.TicksInState + k * 300) * 0.04f) * num14, Vector2.One * num15);
			}
			for (int l = 1; l <= 5; l++)
			{
				Sprite sprite5 = _("title_grue_r" + l);
				float num20 = 0.35f + 0.1f * (float)(l - 1);
				float num21 = 0.25f;
				float num22 = Component._m(Component._M(num - num20, 0f), num20 + num21) / num21;
				Vector2 position3 = new Vector2((float)base.core.Renderer.ScreenWidth - (float)sprite5.Width * num15 + (float)sprite5.Width * num22, (float)base.core.Renderer.ScreenHeight - (float)sprite5.Height * num15).Shift(1.5f - 1.5f * Component._cos((float)(base.TicksInState + 60 * l) * 0.03f), Component._sin((float)(base.TicksInState + 30 * l) * 0.02f)).Shift(num16 * (float)base.core.Renderer.ScreenWidth, num16 * (float)base.core.Renderer.ScreenHeight);
				base.core.Renderer["fg"].DrawSpriteS(sprite5, position3, Color.Lerp(Color.Black, Color.White, 0.7f + 0.1f * Component._sin((float)(base.TicksInState + l * 200) * (0.01f + (float)l * 0.01f))) * num14, Vector2.One * num15);
				base.core.Renderer["fg"].DrawSpriteS(_("title_bodyglow_r" + l), position3, Color.White * (0.3f * Component._sin((float)(base.TicksInState + l * 300) * 0.04f)) * num14, Vector2.One * num15);
				if (l < 5)
				{
					base.core.Renderer["fg"].DrawSpriteS(_("title_eyeglow_r" + l), position3, Color.White * Component._sin((float)(base.TicksInState + l * 300) * 0.04f) * num14, Vector2.One * num15);
				}
			}
			if (num < 0.1f && titleTrans != TitleTrans.ToGame)
			{
				for (int m = 1; m <= 3; m++)
				{
					if ((base.TicksInState + m * 20) % (110 + m * 5) >= 15 - m)
					{
						Sprite sprite6 = _("title_eyes_l" + m);
						base.core.Renderer["fg"].DrawSpriteS(sprite6, new Vector2(0f, (float)base.core.Renderer.ScreenHeight - (float)sprite6.Height * num15), Color.White * num14, Vector2.One * num15);
					}
				}
				for (int n = 1; n <= 4; n++)
				{
					if ((base.TicksInState + n * 20) % (140 - n * 10) >= 15 + n * 3)
					{
						Sprite sprite7 = _("title_eyes_r" + n);
						base.core.Renderer["fg"].DrawSpriteS(sprite7, new Vector2((float)base.core.Renderer.ScreenWidth - (float)sprite7.Width * num15, (float)base.core.Renderer.ScreenHeight - (float)sprite7.Height * num15), Color.White * num14, Vector2.One * num15);
					}
				}
			}
			float num23 = 0.3f;
			float num24 = 0.3f;
			float num25 = Component._m(Component._M(num - num23, 0f), num23 + num24) / num24;
			Sprite sprite8 = _(SpriteName.title_knight);
			float num26 = (float)Tween.BackEaseIn(num25, 0.0, (float)sprite8.Height * num15, 1.0);
			base.core.Renderer["fg"].DrawSpriteS(sprite8, new Vector2(base.core.Renderer.ScreenCenter.X, (float)base.core.Renderer.ScreenHeight + num26 + num16 * 180f), Color.White * num14, Vector2.One * (num15 + num16 * 0.2f), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			if (num < 0.3f)
			{
				base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.title_knight_glow), new Vector2(base.core.Renderer.ScreenCenter.X, (float)base.core.Renderer.ScreenHeight + num26 + num16 * 180f), Color.White * num14 * (0.7f + 0.3f * Component._sin((float)base.TicksInState * 0.15f)), Vector2.One * (num15 + num16 * 0.2f), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			}
			if (num < 0.3f)
			{
				base.core.Renderer["fg"].DrawSpriteS(titleFlameL.GetCurrentFrame(), new Vector2(-45f * num16, (float)base.core.Renderer.ScreenHeight + 80f * num16), Color.White * num14, Vector2.One * (num15 - 0.05f), 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
				base.core.Renderer["fg"].DrawSpriteS(titleFlameR.GetCurrentFrame(), new Vector2((float)base.core.Renderer.ScreenWidth + 45f * num16, (float)base.core.Renderer.ScreenHeight + 80f * num16), Color.White * num14, Vector2.One * (num15 - 0.05f), 0f, SpriteFlip.Horizontal, SpriteOrigin.BottomRight);
			}
			if (titleTrans == TitleTrans.In && num > 0f && num < 0.3f)
			{
				base.core.Renderer["fg", 1020, false].FillScreen(default(Color).FromRgb(8972542) * (num - 0f) * 3.333f);
			}
			if (Transition != TransType.None && titleTrans == TitleTrans.Out)
			{
				base.core.Renderer["fg", 1020, false].FillScreen(Color.Black * (1f - (float)base.Trans / (float)base.TransDuration));
			}
		}
		int num27;
		if (!GameOver)
		{
			if (titleTrans == TitleTrans.In)
			{
				num27 = ((!(num > 0.3f)) ? 1 : 0);
				if (num27 == 0)
				{
					goto IL_21bf;
				}
			}
			else
			{
				num27 = 1;
			}
		}
		else
		{
			num27 = 1;
		}
		touchMenu.Draw();
		goto IL_21bf;
		IL_21bf:
		if (num27 != 0 && newItemsCount > 0)
		{
			float num28 = (((double)((float)base.TicksInState * 0.2f) % (Math.PI * 4.0) < Math.PI * 2.0) ? Component._sin((float)base.TicksInState * 0.2f) : 0f);
			float num29 = Math.Abs(num28);
			base.core.Renderer["fg", 1001, false].DrawSpriteS(_(SpriteName.icon_new), touchMenu[Button.Shop].Rectangle.BottomLeft.Shift(0f, -2f - num29 * 3f), null, new Vector2(1f - num28 * 0.1f, 1f + num28 * 0.1f), (0f - num28) * 0.1f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (!GameOver && num27 != 0 && IsTopState && Transition == TransType.None)
		{
			ContextPromptLegend.DrawVertical(
				base.core.Renderer["fg", 1500, false],
				new Vector2(base.core.Renderer.ScreenWidth - 140f, base.core.Renderer.ScreenHeight - 82f),
				new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_start_game)),
				new ContextPrompt(PromptAction.Secondary, __(SId.PROMPT_change_character)));
		}
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.Start:
			SendMessage(new PlaySoundMessage(SoundName.trans_1));
			if (GameOver)
			{
				TransitionOut(CoreEvent.ResetAndStartGame);
				break;
			}
			startedCurrentGame = true;
			titleTrans = TitleTrans.ToGame;
			base.TransDuration = 20;
			TransitionOut(CoreEvent.StartGame);
			break;
		case Button.Shop:
			titleTrans = TitleTrans.Out;
			base.TransDuration = 20;
			SendMessage(new PlaySoundMessage(SoundName.trans_1));
			TransitionOut(CoreEvent.Shop);
			break;
		case Button.Share:
			if (GameOver)
			{
				SendMessage(new PushStateMessage(new ScreenshotState(touchMenu[Button.Share].Rectangle.Center, 50f, string.Format(__(SId.SHARE_i_walked), shareDeathMessage, "Redungeon", "Eneminds", "Nitrome", "Google Play: goo.gl/FUb9zH"))));
				hideScreenshot = true;
			}
			break;
		case Button.SubMenu:
			SendMessage(new CoreEventMessage(CoreEvent.ShowOptions));
			break;
		case Button.GPlay:
			// Google Play sign-in removido no port desktop (sem GooglePlayHelper).
			break;
		case Button.Options:
			SendMessage(new CoreEventMessage(CoreEvent.ShowOptions));
			break;
		case Button.Scores:
			base.core.SystemCalls.ShowLeaderboards();
			break;
		case Button.NoAds:
			base.core.Store.PurchaseProduct(Iap.RemoveAds, delegate(Iap iap, bool succeed)
			{
				touchMenu[Button.NoAds].Hidden = succeed;
			});
			break;
		case Button.Achievements:
			SendMessage(new PushStateMessage(new FactsState()));
			break;
		case Button.Action:
			OnActionButtonRelease();
			break;
		case Button.TextAction:
			SendMessage(new PushStateMessage(new FactsState((!(gameOverAction.TextButtonText == __(SId.ACTION_more_facts))) ? FactsState.FactsPage.Deaths : FactsState.FactsPage.Facts)));
			break;
		case Button.MoreNitrome:
			Event(AnalyticsCategory.Ux, "more-nitrome");
			base.core.Sharing.GoToNitromePage();
			break;
		}
	}

	private Sprite MakeScreenshotSprite()
	{
		Texture2D texture = base.core.SpriteManager.GetTexture("screenshot-preview");
		if (texture == null)
		{
			return null;
		}
		return new Sprite
		{
			X = 0,
			Y = 0,
			Width = texture.Width,
			Height = texture.Height,
			TextureName = "screenshot-preview",
			SrcWidth = texture.Width,
			SrcHeight = texture.Height
		};
	}

	private bool SuggestCharacterToUnlock(bool soon, ref Character character)
	{
		int coins = base.core.ProfileData.Coins;
		for (int i = 0; i < 10; i++)
		{
			Character random = (Character)SciHelper.GetRandom(0, Enum.GetValues(typeof(Character)).Length - 1);
			int unlockPrice = CharDescription.Get[random].UnlockPrice;
			if (base.core.ProfileData.Characters[random].Unlocked)
			{
				continue;
			}
			if (soon)
			{
				if ((float)coins >= (float)unlockPrice * 0.5f && coins < unlockPrice)
				{
					character = random;
					return true;
				}
			}
			else if (coins >= unlockPrice)
			{
				character = random;
				return true;
			}
		}
		return false;
	}

	private bool SuggestCharacterToUpgrade(bool soon, ref Character character)
	{
		int coins = base.core.ProfileData.Coins;
		for (int i = 0; i < 10; i++)
		{
			Character random = (Character)SciHelper.GetRandom(0, Enum.GetValues(typeof(Character)).Length - 1);
			if (!base.core.ProfileData.Characters[random].Unlocked || base.core.ProfileData.Characters[random].Level == CharDescription.Get[random].Levels.Count)
			{
				continue;
			}
			int level = base.core.ProfileData.Characters[random].Level;
			int price = CharDescription.Get[random].Levels[level].Price;
			if (soon)
			{
				if ((float)coins >= (float)price * 0.5f && coins < price)
				{
					character = random;
					return true;
				}
			}
			else if (coins >= price)
			{
				character = random;
				return true;
			}
		}
		return false;
	}

	private void SelectGameOverAction()
	{
		Dictionary<GameOverActionType, Character> dictionary = new Dictionary<GameOverActionType, Character>();
		GameOverActionType gameOverActionType = GameOverActionType.Hint;
		BagOf<GameOverActionType> bagOf = new BagOf<GameOverActionType>();
		int num = _stat(Stat.Attempts);
		Character character = Character.Knight;
		bagOf.Put(GameOverActionType.Hint, 3);
		if (SuggestCharacterToUnlock(soon: false, ref character))
		{
			dictionary[GameOverActionType.Unlock] = character;
			bagOf.Put(GameOverActionType.Unlock);
		}
		if (SuggestCharacterToUnlock(soon: true, ref character))
		{
			dictionary[GameOverActionType.UnlockSoon] = character;
			bagOf.Put(GameOverActionType.UnlockSoon);
		}
		if (SuggestCharacterToUpgrade(soon: false, ref character))
		{
			dictionary[GameOverActionType.Upgrade] = character;
			bagOf.Put(GameOverActionType.Upgrade);
		}
		if (SuggestCharacterToUpgrade(soon: true, ref character))
		{
			dictionary[GameOverActionType.UpgradeSoon] = character;
			bagOf.Put(GameOverActionType.UpgradeSoon);
		}
		if (bagOf.Count > 0)
		{
			gameOverActionType = bagOf.Draw();
		}
		gameOverAction = new GameOverAction(gameOverActionType, dictionary.ContainsKey(gameOverActionType) ? dictionary[gameOverActionType] : Character.Knight);
		base.core.LastShownAction = gameOverActionType;
	}

	private void OnActionButtonRelease()
	{
		Event(AnalyticsCategory.GameOverAction, gameOverAction.Type.ToString());
		gameOverAction.Done = true;
		int reward = 0;
		bool flag = true;
		switch (gameOverAction.Type)
		{
		case GameOverActionType.Like:
			base.core.SystemCalls.OpenUrl("https://m.facebook.com/nitrome");
			reward = 200;
			base.core.ProfileData.FacebookLiked = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.LikeEneminds:
			base.core.SystemCalls.OpenUrl("https://m.facebook.com/eneminds");
			reward = 200;
			base.core.ProfileData.FacebookEnemindsLiked = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.Follow:
			base.core.SystemCalls.OpenUrl("https://mobile.twitter.com/nitrome");
			reward = 200;
			base.core.ProfileData.TwitterFollowed = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.FollowEneminds:
			base.core.SystemCalls.OpenUrl("https://mobile.twitter.com/eneminds");
			reward = 200;
			base.core.ProfileData.TwitterEnemindsFollowed = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.Feedback:
			base.core.Sharing.SendFeedback();
			base.core.ProfileData.FeedbackSent = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.Rate:
			base.core.Sharing.RateUs();
			base.core.ProfileData.AppRated = true;
			base.core.ProfileData.SaveIntoStorage();
			break;
		case GameOverActionType.WatchAd:
			base.core.AdsManager.ShowUnityAds(delegate(WatchAddStatus status)
			{
				if (status == WatchAddStatus.Watched)
				{
					int optimalWatchAdReward = base.core.AdsManager.GetOptimalWatchAdReward();
					Event(AnalyticsCategory.Ads, "show-ads-for-coins", optimalWatchAdReward);
					DisableActionButton();
					base.core.ProfileData.AddCoins(optimalWatchAdReward);
					SendMessage(new PushStateMessage(new PopupState(optimalWatchAdReward)));
				}
			});
			flag = false;
			break;
		case GameOverActionType.Upgrade:
			SendMessage(new PushStateMessage(new ShopState(gameOverAction.Character, immediatePurchase: true)));
			break;
		case GameOverActionType.Unlock:
			SendMessage(new PushStateMessage(new ShopState(gameOverAction.Character, immediatePurchase: true)));
			break;
		case GameOverActionType.UpgradeSoon:
			SendMessage(new CoreEventMessage(CoreEvent.ShowGetCoins));
			flag = false;
			break;
		case GameOverActionType.UnlockSoon:
			SendMessage(new CoreEventMessage(CoreEvent.ShowGetCoins));
			flag = false;
			break;
		case GameOverActionType.Offer1:
		case GameOverActionType.Offer2:
		case GameOverActionType.Offer3:
		{
			flag = false;
			GameOverActionType type = gameOverAction.Type;
			base.core.Store.PurchaseProduct(type switch
			{
				GameOverActionType.Offer2 => Iap.Offer2, 
				GameOverActionType.Offer1 => Iap.Offer1, 
				_ => Iap.Offer3, 
			}, delegate(Iap iap, bool succeed)
			{
				if (succeed)
				{
					DisableActionButton();
					SendMessage(new PushStateMessage(new PopupState(Store.CoinsForOffer[iap])));
				}
			});
			break;
		}
		case GameOverActionType.Doubler:
			flag = false;
			base.core.Store.PurchaseProduct(Iap.CoinDoubler, delegate(Iap iap, bool succeed)
			{
				if (succeed)
				{
					DisableActionButton();
				}
			});
			break;
		}
		if (!flag)
		{
			return;
		}
		base.core.TimerManager.RunOnce(5, delegate
		{
			DisableActionButton();
			if (reward > 0)
			{
				base.core.ProfileData.AddCoins(reward);
				SendMessage(new PushStateMessage(new PopupState(reward)));
			}
		});
	}

	private void DisableActionButton()
	{
		touchMenu[Button.Action].Disabled = true;
		touchMenu[Button.Action].Label = gameOverAction.DoneLabel;
		gameOverAction.Text = gameOverAction.DoneText;
		gameOverAction.Done = true;
	}

	private void SetupActionButton()
	{
		touchMenu.SetupButton(Button.Action, new RectangleF(-70f, -40f, 70f, 33f), gameOverAction.YellowButton ? _(SpriteName.button) : _(SpriteName.button_green), gameOverAction.YellowButton ? _(SpriteName.button_pressed) : _(SpriteName.button_green_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, gameOverAction.YellowButton ? ButtonColor.Orange : ButtonColor.Green, gameOverAction.Label);
	}

	private void SetupTextActionButton()
	{
		touchMenu.SetupButton(Button.TextAction, new RectangleF(-120f, -40f, 120f, 20f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, gameOverAction.TextButtonText, null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 0.65f);
	}

	private void UpdateGameOverActionAfterReturn()
	{
		if (!GameOver)
		{
			return;
		}
		Character character = gameOverAction.Character;
		if (gameOverAction.Type == GameOverActionType.UnlockSoon || gameOverAction.Type == GameOverActionType.Unlock)
		{
			if (base.core.ProfileData.Characters[character].Unlocked)
			{
				gameOverAction = new GameOverAction(GameOverActionType.Unlock, character);
				SetupActionButton();
				DisableActionButton();
			}
			else
			{
				bool flag = base.core.ProfileData.Coins >= CharDescription.Get[character].UnlockPrice;
				gameOverAction = new GameOverAction(flag ? GameOverActionType.Unlock : GameOverActionType.UnlockSoon, character);
				SetupActionButton();
			}
		}
		if (gameOverAction.Type == GameOverActionType.UpgradeSoon || gameOverAction.Type == GameOverActionType.Upgrade)
		{
			if (base.core.ProfileData.Characters[character].Level >= CharDescription.Get[character].Levels.Count)
			{
				gameOverAction = new GameOverAction(GameOverActionType.Upgrade, character);
				SetupActionButton();
				DisableActionButton();
			}
			else
			{
				int level = base.core.ProfileData.Characters[character].Level;
				bool flag2 = base.core.ProfileData.Coins >= CharDescription.Get[character].Levels[level].Price;
				gameOverAction = new GameOverAction(flag2 ? GameOverActionType.Upgrade : GameOverActionType.UpgradeSoon, character);
				SetupActionButton();
			}
		}
		if (gameOverAction.HasTextButton)
		{
			SetupTextActionButton();
		}
	}

	public override void OnBackButtonPressed()
	{
		base.core.SystemCalls.MinimizeGame();
		base.OnBackButtonPressed();
	}

	public override void OnStartButtonPressed()
	{
		if (!GameOver && Transition == TransType.None)
		{
			OnButtonRelease(Button.Start);
		}
		base.OnStartButtonPressed();
	}
}
