using Knighter.Gameplay;
using Knighter.Input;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class PauseState : State
{
	protected override bool ShowContextPromptLegend => false;

	private enum Button
	{
		BackToGame,
		Exit,
		Options,
		Share
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private bool stoppedGame;

	private Sprite screenshotSprite;

	private bool hideScreenshot;

	public PauseState()
	{
		base.TransDuration = 25;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 20);
		int num = base.core.Renderer.ScreenHeight - 64;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.BackToGame, new RectangleF(10 + num2, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_play), icon: true, iconIsPicture: false, blink: false);
		touchMenu.SetupButton(Button.Exit, new RectangleF(10f, num, num2 - 3, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_exit));
		float num3 = System.Math.Min(base.core.Renderer.ScreenWidth * 0.3f, System.Math.Max(60f, base.core.Renderer.ScreenHeight - 150f));
		Vector2 screenCenter = new Vector2(R.ScreenCenter.X, 65f + num3 * 0.5f);
		RectangleF rectangle = new RectangleF(screenCenter.X - num3 * 0.5f - 3f, screenCenter.Y - num3 * 0.5f - 3f, num3 + 6f, num3 + 20f);
		touchMenu.SetupButton(Button.Share, rectangle, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.paper_touch, SoundName.paper);
		touchMenu.SetupButton(Button.Options, new RectangleF(10 + 3 * num2, num, num2 - 3, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_options));
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			Button.BackToGame, Button.Exit, Button.Options);
		touchMenu[Button.Share].Hidden = true;
		screenshotSprite = base.core.SpriteManager.MakeFullSpriteFromScreenshot(base.core.GameplayScreenshot);
		int num4 = screenshotSprite.Width / 5;
		int num5 = (screenshotSprite.Height - num4 * 3) / 2;
		screenshotSprite = screenshotSprite.Reduce(num4, num5, num4, num5);
	}

	public override void Load()
	{
		Screen("pause");
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		base.Load();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.BackToGame].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(2, 4), 150.0, -150.0, base.TransDuration - 4));
		touchMenu[Button.Exit].Rectangle.Shift(0f, (float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		touchMenu[Button.Options].Rectangle.Shift(0f, (float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void Update()
	{
		touchMenu.Update();
		base.core.AudioManager.MusicVolumeBox.Set("pause", 0.3f, inWorld: false);
		base.core.CurrentPlayState.Camera.ZoomBox.Set("pause", 1f - (float)Tween.BackEaseOut(base.Trans, 0.0, 0.30000001192092896, base.TransDuration), inWorld: false);
		base.Update();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
						if (core.Input.WasPressed(GameAction.Screenshot)) { OnButtonRelease(Button.Options); return; }
			touchMenu.HandleInput();
			menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
		}
		base.HandleInput();
	}

	public override void Draw()
	{
		float num = 0.6f * (float)base.Trans / (float)base.TransDuration;
		if (Transition == TransType.Out)
		{
			num = ((!stoppedGame) ? 0.6f : (0.6f + (1f - num) * 0.4f));
		}
		float num2 = (float)base.Trans / (float)base.TransDuration;
		num2 *= num2;
		if (touchMenu[Button.Share].IsDown)
		{
			num2 *= 0.9f;
		}
		float photoSize = System.Math.Min(R.ScreenWidth * 0.3f, System.Math.Max(60f, R.ScreenHeight - 150f));
		float num3 = photoSize * num2;
		Vector2 vector = new Vector2(R.ScreenCenter.X, 65f + photoSize * 0.5f);
		RectangleF rectangleF = new RectangleF(vector.X - num3 * 0.5f, vector.Y - num3 * 0.5f, num3, num3);
		base.core.Renderer["fg", 10, false].FillScreen(Color.Black * num);
		Color value = (touchMenu[Button.Share].IsDown ? Color.Gray : (Color.LightGray * num2));
		if (screenshotSprite != null && !hideScreenshot)
		{
			float rotation = 0f;
			base.core.Renderer["fg", 10, false].DrawSpriteS(base.core.SpriteManager.Pixel, rectangleF.Center, value, new Vector2(rectangleF.Width, rectangleF.Height), rotation, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 10, false].DrawSpriteS(screenshotSprite, rectangleF.Center, Color.White * num2, new Vector2(rectangleF.Width - 4f, rectangleF.Height - 4f) / new Vector2(screenshotSprite.Width, screenshotSprite.Height), rotation, SpriteFlip.None, SpriteOrigin.Center);
		}
		float num4 = (float)Tween.BackEaseOut(base.Trans, -80.0, 80.0, base.TransDuration);
		Vector2 vector2 = new Vector2(base.core.Renderer.ScreenCenter.X, 15f + num4 + (float)base.topSafeArea);
		Sprite sprite = _(SpriteName.gui_chain);
		for (float num5 = vector2.Y - (float)sprite.Height; num5 > (float)(-sprite.Height); num5 -= (float)sprite.Height)
		{
			base.core.Renderer["fg", 15, false].DrawSpriteS(sprite, new Vector2(vector2.X - 32f - (float)(sprite.Width / 2), num5));
			base.core.Renderer["fg", 15, false].DrawSpriteS(sprite, new Vector2(vector2.X + 32f - (float)(sprite.Width / 2), num5));
		}
		Sprite sprite2 = _(SpriteName.pause_block);
		base.core.Renderer["fg", 10, false].DrawSpriteS(sprite2, vector2, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		base.core.Renderer["fg", 10, false].DrawTextS(__(SId.PAUSE_paused), vector2.Shift(0f, 21f), new TextProfile
		{
			Width = base.core.Renderer.ScreenWidth - 20,
			BoxAlignment = Alignment2D.Middle,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(9212825),
			SecondColor = default(Color).FromRgb(1645605),
			Decoration = TextDecoration.Extrude2,
			Font = Font.Bold,
			Scale = 1.2f
		});
		touchMenu.Draw();
		if (IsTopState && Transition == TransType.None)
		{
			DrawHint(Button.Exit, PromptAction.Cancel, SId.PC_pause_exit);
			DrawHint(Button.BackToGame, PromptAction.Pause, SId.PC_pause_resume);
			DrawHint(Button.Options, PromptAction.Screenshot, SId.OPTIONS_settings_title);
		}
		base.Draw();
	}

	private void DrawHint(Button button, PromptAction action, SId label)
	{
		var rect = touchMenu[button].Rectangle;
		ContextPromptLegend.Draw(R, new Vector2(rect.Left + 4, rect.Bottom + 5), new ContextPrompt(action, __(label)));
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.BackToGame:
			Resume();
			break;
		case Button.Exit:
			SendMessage(new PlaySoundMessage(SoundName.trans_1));
			TransitionOut(CoreEvent.ResetGame);
			stoppedGame = true;
			break;
		case Button.Options:
			SendMessage(new CoreEventMessage(CoreEvent.ShowOptions));
			break;
		}
	}

	public override void OnReturn()
	{
		hideScreenshot = false;
		base.OnReturn();
	}

	public override void OnBackButtonPressed() { if (Transition == TransType.None) OnButtonRelease(Button.Exit); }

	private void Resume()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1));
		TransitionOut(CoreEvent.HidePause);
		base.OnBackButtonPressed();
	}

	public override void OnStartButtonPressed()
	{
		Resume();
		base.OnStartButtonPressed();
	}
}
