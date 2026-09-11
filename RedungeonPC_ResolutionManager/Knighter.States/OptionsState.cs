using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class OptionsState : State
{
	private enum Button
	{
		PlaySounds,
		PlayMusic,
		Controls,
		Cloud,
		RestoreIap,
		Feedback,
		Credits,
		Languages,
		Back,
		PrivacyPolicy,
		Debug1,
		Debug2,
		Debug3,
		Debug4
	}

	private readonly TouchMenu<Button> touchMenu;

	private RectangleF menuRect;

	private RectangleF view;

	private Sprite block;

	private Sprite chain;

	private Sprite scroll;

	private Sprite roll;

	private float creditsScrollTop;

	private int hintTimer;

	private string hint;

	private Button hintButton;

	private float dScroll;

	private int scrollDir = -1;

	private int scrollWait = 200;

	private float scrollHeight = 210f;

	private string versionString;

	private bool hideScroll;

	public OptionsState()
	{
		base.TransDuration = 25;
		IsOverlay = true;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 3000);
		menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 148) * 0.5f, (float)(base.core.Renderer.ScreenHeight - 262) * 0.5f, 148f, 233f);
		view = new RectangleF(menuRect.Left + 13f, menuRect.Top + 82f, 122f, 67f);
		block = _(SpriteName.options_block);
		roll = _(SpriteName.options_scroll_roll);
		scroll = _(SpriteName.options_scroll);
		chain = _(SpriteName.gui_chain);
		int num = 35;
		int num2 = 30;
		float num3 = menuRect.Left + 17f;
		float num4 = menuRect.Bottom - 40f;
		touchMenu.SetupButton(Button.PlayMusic, new RectangleF(num3, num4, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true);
		touchMenu.SetupButton(Button.PlaySounds, new RectangleF(num3 + 40f, num4, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true);
		touchMenu.SetupButton(Button.Controls, new RectangleF(num3 + 80f, num4, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_control_dpad));
		touchMenu.SetupButton(Button.Credits, new RectangleF(menuRect.Left, num4 - 135f, menuRect.Width, 48f), null, null);
		touchMenu.SetupButton(Button.PrivacyPolicy, new RectangleF(menuRect.Left + 17f, num4 - 135f + 48f, menuRect.Width - 34f, 18f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, __(SId.OPTIONS_privacy_policy));
		touchMenu.SetupButton(Button.Cloud, new RectangleF(num3, num4 - 70f, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true);
		touchMenu.SetupButton(Button.Languages, new RectangleF(num3, num4 - 35f, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_globe));
		touchMenu.SetupButton(Button.RestoreIap, new RectangleF(num3 + 40f, num4 - 35f, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_restore_iap));
		touchMenu.SetupButton(Button.Feedback, new RectangleF(num3 + 80f, num4 - 35f, num, num2), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_feedback));
		touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
		versionString = base.core.SystemCalls.GetVersionString();
	}

	public override void UpdateTransition()
	{
		float y = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
		touchMenu[Button.PrivacyPolicy].Rectangle.Shift(0f, y);
		touchMenu[Button.Cloud].Rectangle.Shift(0f, y);
		touchMenu[Button.Languages].Rectangle.Shift(0f, y);
		touchMenu[Button.RestoreIap].Rectangle.Shift(0f, y);
		touchMenu[Button.Feedback].Rectangle.Shift(0f, y);
		touchMenu[Button.PlayMusic].Rectangle.Shift(0f, y);
		touchMenu[Button.PlaySounds].Rectangle.Shift(0f, y);
		touchMenu[Button.Controls].Rectangle.Shift(0f, y);
		touchMenu[Button.Back].Rectangle.Shift(0f, y);
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
			touchMenu.HandleInput();
			base.HandleInput();
		}
	}

	public override void Update()
	{
		IsOpaque = Transition == TransType.None;
		if (hintTimer > 0)
		{
			hintTimer--;
		}
		if (scrollWait > 0)
		{
			scrollWait--;
		}
		else
		{
			dScroll += 0.15f * (float)scrollDir;
			if (scrollDir > 0)
			{
				if (dScroll >= 0f)
				{
					scrollDir = -1;
					scrollWait = 120;
				}
			}
			else if (dScroll <= 0f - (scrollHeight - view.Height))
			{
				scrollDir = 1;
				scrollWait = 120;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * (1f - num * num * num));
		float num2 = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
		for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
		{
			base.core.Renderer["fg", 2000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
			base.core.Renderer["fg", 2000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
		}
		base.core.Renderer["fg", 2002, false].DrawSpriteS(block, menuRect.TopLeft.Shift(0f, num2));
		int num3 = 17;
		if (!hideScroll)
		{
			creditsScrollTop = menuRect.Top + 53f + (float)num3;
			base.core.Renderer["fg", 2003, false].DrawSpriteS(scroll, menuRect.TopLeft.Shift(13f, 53f + num2 + (float)num3));
			base.core.Renderer["fg", 2005, false].DrawSpriteS(roll, menuRect.TopLeft.Shift(0f, 36f + num2 + (float)num3 + (float)(touchMenu[Button.Credits].IsDown ? 2 : 0)));
			base.core.Renderer["fg", 2002, false].DrawSpriteS(roll, menuRect.TopLeft.Shift(0f, 36f + num2 + (float)num3 + 4f + (float)(touchMenu[Button.Credits].IsDown ? 2 : 0)), Color.Black * 0.2f);
			base.core.Renderer["fg", 2005, false].DrawSpriteS(roll, menuRect.TopLeft.Shift(0f, 72f + num2 + (float)num3 - (float)(touchMenu[Button.Credits].IsDown ? 2 : 0)), null, null, 0f, SpriteFlip.Vertical);
			base.core.Renderer["fg", 2002, false].DrawSpriteS(roll, menuRect.TopLeft.Shift(0f, 72f + num2 + (float)num3 + 4f - (float)(touchMenu[Button.Credits].IsDown ? 2 : 0)), Color.Black * 0.2f, null, 0f, SpriteFlip.Vertical);
		}
		RectangleF rectangleF = view.Clone();
		TextProfile obj = new TextProfile
		{
			Decoration = TextDecoration.None,
			Color = default(Color).FromRgb(9462096),
			Scale = 0.6f,
			Width = (int)rectangleF.Width,
			BoxAlignment = Alignment2D.Left,
			TextAlignment = Alignment2D.Center
		};
		TextProfile textProfile = obj.Alter(default(Color).FromRgb(5185837));
		TextProfile textProfile2 = obj.Alter(boxAlignment: Alignment2D.Left, textAlignment: Alignment2D.Center, scale: 0.5f, color: default(Color).FromRgb(6844288), secondColor: default(Color).FromRgb(855827), decoration: TextDecoration.None, width: (int)menuRect.Width);
		base.core.Renderer["fg", 2002, false].DrawTextS(__(SId.CREDITS_credits), menuRect.TopLeft.Shift(5f, 40f + num2 + (float)num3), textProfile.Alter(null, null, null, font: Font.Bold, textAlignment: Alignment2D.Middle, width: (int)menuRect.Width - 10, height: 44, boxAlignment: null, scale: touchMenu[Button.Credits].IsDown ? 0.9f : 1f));
		base.core.Renderer["fg", 2002, false].DrawTextS(versionString, menuRect.TopLeft.Shift(0f, 45f + num2), textProfile2);
		base.core.Renderer["fg", 2002, false].DrawTextS(__(SId.OPTIONS_last_saved), touchMenu[Button.Cloud].Rectangle.TopRight.Shift(8f, 0f), textProfile2.Alter(null, null, null, font: Font.Bold, textAlignment: Alignment2D.Left, width: (int)menuRect.Width - 55, height: null, boxAlignment: null, scale: 0.8f));
		string text = "—";
		if (base.core.ProfileData.LastSyncTime != string.Empty && DateTime.TryParse(base.core.ProfileData.LastSyncTime, out var result))
		{
			text = result.ToString("yyyy-MM-dd\nHH:mm");
		}
		base.core.Renderer["fg", 2002, false].DrawTextS(text, touchMenu[Button.Cloud].Rectangle.TopRight.Shift(8f, 10f), textProfile2.Alter(null, null, null, font: Font.Thin, textAlignment: Alignment2D.Left, width: (int)menuRect.Width - 55, height: null, boxAlignment: null, scale: 0.8f));
		if (hintTimer > 0)
		{
			float num4 = 1f - (float)hintTimer / 70f;
			RectangleF rectangleF2 = touchMenu[hintButton].Rectangle.Clone();
			rectangleF2.X -= 30f;
			rectangleF2.Width += 60f;
			rectangleF2.Y -= 20f + 20f * num4;
			base.core.Renderer["fg", 3010, false].DrawTextS(hint, rectangleF2.CenterTop, TextProfile.OrangeBoldText.Alter(TextProfile.OrangeLight * (1f - num4 * num4 * num4), Color.Black * (1f - num4 * num4 * num4), TextDecoration.Contour));
		}
		touchMenu.Draw();
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.PrivacyPolicy:
			base.core.SystemCalls.OpenUrl("http://www.nitrome.com/privacy/");
			break;
		case Button.Credits:
			hideScroll = true;
			SendMessage(new PushStateMessage(new CreditsState(creditsScrollTop)));
			break;
		case Button.PlaySounds:
			base.core.OptionsData.PlaySounds = !base.core.OptionsData.PlaySounds;
			UpdateLabels();
			base.core.ApplyOptions();
			base.core.SaveOptions();
			if (base.core.OptionsData.PlaySounds)
			{
				SendMessage(new PlaySoundMessage(SoundName.coin));
			}
			break;
		case Button.PlayMusic:
			base.core.OptionsData.PlayMusic = !base.core.OptionsData.PlayMusic;
			UpdateLabels();
			base.core.ApplyOptions();
			base.core.SaveOptions();
			break;
		case Button.Controls:
			SendMessage(new PushStateMessage(new ControlsSelectorState()));
			break;
		case Button.Feedback:
			base.core.Sharing.SendFeedback();
			break;
		case Button.RestoreIap:
			Event(AnalyticsCategory.Ux, "restore-iap");
			base.core.Store.RestorePurchases();
			break;
		case Button.Cloud:
			base.core.ProfileData.UseCloud = !base.core.ProfileData.UseCloud;
			UpdateLabels();
			break;
		case Button.Languages:
			SendMessage(new PushStateMessage(new LanguageSelectorState()));
			break;
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
	}

	public override void Load()
	{
		Screen("options");
		UpdateLabels();
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
	}

	private void UpdateLabels()
	{
		Sprite sprite = _(SpriteName.icon_cloud);
		Sprite sprite2 = _(SpriteName.icon_cloud_off);
		sprite = _(SpriteName.icon_cloud_android);
		sprite2 = _(SpriteName.icon_cloud_off_android);
		touchMenu[Button.Cloud].LabelSprite = (base.core.ProfileData.UseCloud ? sprite : sprite2);
		touchMenu[Button.PlaySounds].LabelSprite = (base.core.OptionsData.PlaySounds ? _(SpriteName.icon_sound_on) : _(SpriteName.icon_sound_off));
		touchMenu[Button.PlayMusic].LabelSprite = (base.core.OptionsData.PlayMusic ? _(SpriteName.icon_music_on) : _(SpriteName.icon_music_off));
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		TransitionOut(CoreEvent.HideOptions);
		base.OnBackButtonPressed();
	}

	public override void OnReturn()
	{
		hideScroll = false;
		touchMenu[Button.PrivacyPolicy].Label = __(SId.OPTIONS_privacy_policy);
		base.OnReturn();
	}
}
