using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class ControlsSelectorState : State
{
	private enum Button
	{
		SeeThrough,
		LeftHanded,
		TapToStep,
		HoldToRun,
		ChooseSwipes,
		ChooseButtons,
		ChooseDPad,
		Back
	}

	private TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private RectangleF menuRect;

	private bool quick;

	private float selTop;

	private float targetSelTop;

	public ControlsSelectorState(bool quickMode = false)
	{
		quick = quickMode;
		base.TransDuration = 30;
		ShowCoins = false;
		IsOverlay = !quick;
		int num = 250;
		int num2 = 225;
		menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - 141) * 0.5f, (float)(base.core.Renderer.ScreenHeight - num) * 0.5f + (float)(quick ? 15 : 0), 141f, num2);
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
		touchMenu.OnToggle = OnToggle;
		touchMenu.SetupButton(Button.ChooseSwipes, quick ? new RectangleF(menuRect.Left, menuRect.Top + 13f, menuRect.Width, 65f) : new RectangleF(menuRect.Left, menuRect.Top + 11f, menuRect.Width, 21f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, quick ? "" : __(SId.CONTROLS_swipes), null, icon: true, iconIsPicture: false, blink: false, null, null, (!quick) ? 2 : 0);
		selTop = touchMenu[Button.ChooseSwipes].Rectangle.Top;
		touchMenu.SetupButton(Button.ChooseButtons, quick ? new RectangleF(menuRect.Left, menuRect.Top + 82f, menuRect.Width, 65f) : new RectangleF(menuRect.Left, menuRect.Top + 11f + 21f, menuRect.Width, 21f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, quick ? "" : __(SId.CONTROLS_arrows), null, icon: true, iconIsPicture: false, blink: false, null, null, (!quick) ? 2 : 0);
		touchMenu.SetupButton(Button.ChooseDPad, quick ? new RectangleF(menuRect.Left, menuRect.Top + 152f, menuRect.Width, 65f) : new RectangleF(menuRect.Left, menuRect.Top + 11f + 42f, menuRect.Width, 21f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, quick ? "" : __(SId.CONTROLS_dpad), null, icon: true, iconIsPicture: false, blink: false, null, null, (!quick) ? 2 : 0);
		if (!quick)
		{
			touchMenu.SetupToggle(Button.LeftHanded, menuRect.TopLeft.Shift(12f, 85f), base.core.OptionsData.LeftHandedMode, 120);
			touchMenu.SetupToggle(Button.SeeThrough, menuRect.TopLeft.Shift(12f, 106f), base.core.OptionsData.SeeThroughMode, 120);
			touchMenu.SetupToggle(Button.TapToStep, menuRect.TopLeft.Shift(12f, 127f), base.core.OptionsData.TapToStep, 120);
			touchMenu.SetupToggle(Button.HoldToRun, menuRect.TopLeft.Shift(12f, 127f), base.core.OptionsData.HoldToRun, 120);
			touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 35f, menuRect.Bottom + 10f, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
		}
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		UpdateButtons();
		Button selectedScheme = base.core.OptionsData.SwipeControl
			? Button.ChooseSwipes
			: (base.core.OptionsData.CompactDPad ? Button.ChooseDPad : Button.ChooseButtons);
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			selectedScheme, Button.ChooseSwipes, Button.ChooseButtons, Button.ChooseDPad,
			Button.LeftHanded, Button.SeeThrough, Button.TapToStep, Button.HoldToRun, Button.Back);
	}

	private void UpdateButtons()
	{
		touchMenu[Button.ChooseSwipes].Color = ((!quick && base.core.OptionsData.SwipeControl) ? ButtonColor.Orange : ButtonColor.Stone);
		touchMenu[Button.ChooseButtons].Color = ((!quick && !base.core.OptionsData.SwipeControl && !base.core.OptionsData.CompactDPad) ? ButtonColor.Orange : ButtonColor.Stone);
		touchMenu[Button.ChooseDPad].Color = ((!quick && !base.core.OptionsData.SwipeControl && base.core.OptionsData.CompactDPad) ? ButtonColor.Orange : ButtonColor.Stone);
		if (!quick)
		{
			touchMenu[Button.HoldToRun].Hidden = base.core.OptionsData.SwipeControl;
			touchMenu[Button.TapToStep].Hidden = !base.core.OptionsData.SwipeControl;
			if (base.core.OptionsData.SwipeControl)
			{
				targetSelTop = touchMenu[Button.ChooseSwipes].Rectangle.Top;
			}
			if (!base.core.OptionsData.SwipeControl && !base.core.OptionsData.CompactDPad)
			{
				targetSelTop = touchMenu[Button.ChooseButtons].Rectangle.Top;
			}
			if (!base.core.OptionsData.SwipeControl && base.core.OptionsData.CompactDPad)
			{
				targetSelTop = touchMenu[Button.ChooseDPad].Rectangle.Top;
			}
		}
	}

	public override void Update()
	{
		touchMenu.Update();
		IsOpaque = !quick && Transition == TransType.None;
		base.core.AudioManager.MusicVolumeBox.Set("controls-selector", 0.3f, inWorld: false);
		if (!quick)
		{
			selTop += (targetSelTop - selTop) * 0.2f;
		}
		base.Update();
	}

	public override void Draw()
	{
		float num = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num) * (quick ? 0.8f : 1f));
		float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
		if (quick)
		{
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_choose), menuRect.CenterTop.Shift(0f, -16f + num2), new TextProfile
			{
				Width = base.core.Renderer.ScreenWidth - 20,
				BoxAlignment = Alignment2D.Middle,
				TextAlignment = Alignment2D.Middle,
				Color = default(Color).FromRgb(16430139),
				SecondColor = default(Color).FromRgb(9651758),
				Decoration = TextDecoration.Extrude1,
				Font = Font.Bold,
				Scale = 1.2f
			});
			TextProfile profile = new TextProfile
			{
				Width = (int)menuRect.Width - 10,
				BoxAlignment = Alignment2D.Left,
				TextAlignment = Alignment2D.Left,
				Color = default(Color).FromRgb(5462882),
				SecondColor = Color.Black * 0.2f,
				Decoration = TextDecoration.Extrude2,
				Font = Font.Bold,
				Scale = 0.7f
			};
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_swipes), touchMenu[Button.ChooseSwipes].Rectangle.TopLeft.Shift(8f, -2f), profile);
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_arrows), touchMenu[Button.ChooseButtons].Rectangle.TopLeft.Shift(8f, 2f), profile);
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_dpad), touchMenu[Button.ChooseDPad].Rectangle.TopLeft.Shift(8f, 2f), profile);
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_great_for_tablets), menuRect.BottomLeft.Shift(7.5f, -56f + num2 + (float)(touchMenu[Button.ChooseDPad].IsDown ? 3 : 0)), new TextProfile
			{
				Width = 36,
				BoxAlignment = Alignment2D.Left,
				TextAlignment = Alignment2D.Left,
				Color = default(Color).FromRgb(5397345),
				Decoration = TextDecoration.None,
				Font = Font.Thin,
				Scale = 0.7f
			});
		}
		Sprite sprite = _(SpriteName.controls_options_block);
		base.core.Renderer["fg", 9000, false].DrawSpriteS(sprite, menuRect.CenterTop.Shift(0f, num2), null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		touchMenu.Draw();
		if (!quick)
		{
			TextProfile textProfile = new TextProfile
			{
				Width = 87,
				Height = 30,
				BoxAlignment = Alignment2D.Left,
				TextAlignment = Alignment2D.LeftMiddle,
				Decoration = TextDecoration.None,
				Font = Font.Thin,
				Scale = 0.75f
			};
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_left_handed), touchMenu[Button.LeftHanded].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.LeftHanded].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
			base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_see_through), touchMenu[Button.SeeThrough].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.SeeThrough].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
			if (!touchMenu[Button.TapToStep].Hidden)
			{
				base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_tap_to_step), touchMenu[Button.TapToStep].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.TapToStep].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
			}
			if (!touchMenu[Button.HoldToRun].Hidden)
			{
				base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.CONTROLS_hold_to_run), touchMenu[Button.HoldToRun].Rectangle.TopLeft.Shift(32f, -7f), textProfile.Alter(touchMenu[Button.HoldToRun].ToggleValue ? TextProfile.OrangeMiddle : default(Color).FromRgb(6910328)));
			}
			base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.cs_selection), menuRect.CenterTop.Shift(0f, 3f + selTop - menuRect.Top + num2), null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		}
		if (quick || (!base.core.OptionsData.SwipeControl && !base.core.OptionsData.CompactDPad))
		{
			Vector2 position = (quick ? menuRect.CenterTop.Shift(-40f, 77f + num2) : menuRect.CenterBottom.Shift(-40f, -70f + num2));
			base.core.Renderer["fg", 9000, false].DrawSpriteS(_((base.core.OptionsData.SeeThroughMode && !quick) ? SpriteName.cs_arrows_seethrough : SpriteName.cs_arrows), position);
			if (!quick)
			{
				base.core.Renderer["fg", 9000, false].DrawSpriteS(_(base.core.OptionsData.SeeThroughMode ? SpriteName.cs_action_seethrough : SpriteName.cs_action), position, null, null, 0f, base.core.OptionsData.LeftHandedMode ? SpriteFlip.Horizontal : SpriteFlip.None);
			}
		}
		if (quick || (!base.core.OptionsData.SwipeControl && base.core.OptionsData.CompactDPad))
		{
			Vector2 position2 = (quick ? menuRect.CenterTop.Shift(-19f, 152f + num2) : menuRect.CenterBottom.Shift(-40f, -70f + num2));
			base.core.Renderer["fg", 9000, false].DrawSpriteS(_((base.core.OptionsData.SeeThroughMode && !quick) ? SpriteName.cs_dpad_seethrough : SpriteName.cs_dpad), position2, null, null, 0f, (!quick && base.core.OptionsData.LeftHandedMode) ? SpriteFlip.Horizontal : SpriteFlip.None);
			if (!quick)
			{
				base.core.Renderer["fg", 9000, false].DrawSpriteS(_(base.core.OptionsData.SeeThroughMode ? SpriteName.cs_dpad_action_seethrough : SpriteName.cs_dpad_action), position2, null, null, 0f, base.core.OptionsData.LeftHandedMode ? SpriteFlip.Horizontal : SpriteFlip.None);
			}
		}
		if (quick || base.core.OptionsData.SwipeControl)
		{
			if (!quick)
			{
				Vector2 position3 = (quick ? menuRect.CenterTop.Shift(-40f, 13f + num2) : menuRect.CenterBottom.Shift(-40f, -70f + num2));
				base.core.Renderer["fg", 9000, false].DrawSpriteS(_((base.core.OptionsData.SeeThroughMode && !quick) ? SpriteName.cs_action_seethrough : SpriteName.cs_action), position3, null, null, 0f, (base.core.OptionsData.LeftHandedMode && !quick) ? SpriteFlip.Horizontal : SpriteFlip.None);
			}
			int ticksInState = base.TicksInState;
			string text = "nnnneeeesssswwww";
			int num3 = 14;
			int num4 = ticksInState % (text.Length * num3);
			int num5 = num4 / num3;
			int num6 = num4 % num3;
			char c = text[num5];
			string name = "cs_";
			bool flag = false;
			float rotation = 0f;
			char c2 = '_';
			Vector2 v = new Vector2(0f, 0f);
			for (int i = 0; i <= num5; i++)
			{
				char c3 = text[i];
				char c4 = '_';
				if (i + 1 < text.Length)
				{
					c4 = text[i + 1];
				}
				int num7 = 6;
				if (i == num5)
				{
					num7 = ((num6 > 3) ? 6 : (2 * num6));
				}
				switch (c3)
				{
				case 'n':
					v = v.Shift(0f, -num7);
					rotation = 1.57f;
					break;
				case 's':
					v = v.Shift(0f, num7);
					rotation = -1.57f;
					break;
				case 'e':
					v = v.Shift(num7, 0f);
					rotation = 3.14f;
					break;
				case 'w':
					v = v.Shift(-num7, 0f);
					rotation = 0f;
					break;
				}
				if (i == num5 && c != '_')
				{
					if (c == 'n' && base.core.OptionsData.TapToStep)
					{
						int num8 = ((num6 < 4) ? 1 : ((num6 < 6) ? 2 : ((num6 < 8) ? 3 : 0)));
						name = "cs_tap_" + num8;
						rotation = 0f;
						flag = num8 > 0;
					}
					else
					{
						int num9 = num6 / 2 + 1;
						if (c3 == c4)
						{
							num9 = (int)Component._m(num9, 5f);
						}
						else if (num9 > 7)
						{
							num9 = 0;
						}
						if (c3 == c2)
						{
							num9 = ((num9 != 0) ? ((int)Component._M(num9, 5f)) : 0);
						}
						name = "cs_swipe_" + num9;
						flag = num9 > 0;
					}
				}
				c2 = c3;
			}
			if (flag)
			{
				Sprite sprite2 = _(name);
				base.core.Renderer["fg", 9000, false].DrawSpriteS(sprite2, quick ? menuRect.CenterTop.Shift(0f, 50f + num2) : menuRect.CenterBottom.Shift(0f, -38f + num2), null, null, rotation, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		base.Draw();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
			touchMenu.HandleInput();
			menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
			base.HandleInput();
		}
	}

	public override void UpdateTransition()
	{
		float y = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
		touchMenu[Button.ChooseSwipes].Rectangle.Shift(0f, y);
		touchMenu[Button.ChooseButtons].Rectangle.Shift(0f, y);
		touchMenu[Button.ChooseDPad].Rectangle.Shift(0f, y);
		if (!quick)
		{
			touchMenu[Button.SeeThrough].Rectangle.Shift(0f, y);
			touchMenu[Button.LeftHanded].Rectangle.Shift(0f, y);
			touchMenu[Button.TapToStep].Rectangle.Shift(0f, y);
			touchMenu[Button.HoldToRun].Rectangle.Shift(0f, y);
			touchMenu[Button.Back].Rectangle.Shift(0f, y);
		}
		base.UpdateTransition();
	}

	private void OnToggle(Button button, bool newValue)
	{
		switch (button)
		{
		case Button.SeeThrough:
			base.core.OptionsData.SeeThroughMode = newValue;
			break;
		case Button.LeftHanded:
			base.core.OptionsData.LeftHandedMode = newValue;
			break;
		case Button.TapToStep:
			base.core.OptionsData.TapToStep = newValue;
			break;
		case Button.HoldToRun:
			base.core.OptionsData.HoldToRun = newValue;
			break;
		}
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.ChooseSwipes:
			base.core.OptionsData.SwipeControl = true;
			UpdateButtons();
			break;
		case Button.ChooseButtons:
			base.core.OptionsData.SwipeControl = false;
			base.core.OptionsData.CompactDPad = false;
			UpdateButtons();
			break;
		case Button.ChooseDPad:
			base.core.OptionsData.SwipeControl = false;
			base.core.OptionsData.CompactDPad = true;
			UpdateButtons();
			break;
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
		if (quick)
		{
			base.core.ProfileData.ControlsSelectorPending = false;
			base.core.SaveOptions();
			base.core.ProfileData.SaveIntoStorage();
			base.core.CurrentPlayState.Unpause();
			base.core.CurrentPlayState.UnpauseTimer = 0;
			TransitionOut(CoreEvent.PopState);
			SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		}
	}

	public override void OnBackButtonPressed()
	{
		if (!quick)
		{
			base.core.SaveOptions();
			base.core.ProfileData.SaveIntoStorage();
			TransitionOut(CoreEvent.PopState);
			SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		}
		base.OnBackButtonPressed();
	}
}
