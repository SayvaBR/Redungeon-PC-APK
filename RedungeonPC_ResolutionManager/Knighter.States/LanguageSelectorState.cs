using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class LanguageSelectorState : State
{
	private enum Button
	{
		en_US,
		ru_RU,
		uk_UA,
		es_ES,
		pl_PL,
		de_DE,
		pt_PT,
		fr_FR,
		ja_JP,
		Back
	}

	private TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private RectangleF menuRect;

	private Sprite block;

	private Sprite chain;

	private bool quick;

	private Animation bat;

	private Vector2 batPos;

	private Vector2 batTarget;

	public LanguageSelectorState(bool quickMode = false)
	{
		quick = quickMode;
		base.TransDuration = 30;
		ShowCoins = false;
		IsOverlay = !quick;
		block = _(SpriteName.language_options_block);
		chain = _(SpriteName.gui_chain);
		menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - block.Width) * 0.5f, (float)(base.core.Renderer.ScreenHeight - block.Height) * 0.5f + (float)(quick ? 15 : 0), block.Width, block.Height);
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
		int num = 38;
		int num2 = 26;
		float num3 = menuRect.Width / 2f;
		int num4 = 0;
		int num5 = 0;
		foreach (Language value in Enum.GetValues(typeof(Language)))
		{
			if (value != Language.ja_JP)
			{
				touchMenu.SetupButton((Button)Enum.Parse(typeof(Button), value.ToString()), new RectangleF(menuRect.Left + num3 * (float)num5, menuRect.Top + (float)num + (float)((num2 + 2) * num4), num3, num2), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Stone, (value == Language.ja_JP) ? "" : base.core.LocaleManager.Locales[value].LanguageName, (value == Language.ja_JP) ? _(SpriteName.btn_japanese) : null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 0.8f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.button_up, seeThrough: false, 0.8f);
				if (num5 == 0)
				{
					num5++;
					continue;
				}
				num5 = 0;
				num4++;
			}
		}
		UpdateLanguageButtons();
		touchMenu.SetupButton(Button.Back, new RectangleF(menuRect.Center.X - 30f, menuRect.Bottom - 40f, 60f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_ok));
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		bat = new Animation(0.15f);
		bat.AddAndPlay("fly", new List<SpriteName>
		{
			SpriteName.bat_1,
			SpriteName.bat_2,
			SpriteName.bat_3,
			SpriteName.bat_4
		});
		batPos = new Vector2(-60f, (float)base.core.Renderer.ScreenHeight * 0.5f);
		Button currentLanguage = (Button)Enum.Parse(typeof(Button), base.core.LocaleManager.CurrentLocale.ToString());
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, currentLanguage, Button.Back);
	}

	public override void Load()
	{
		base.core.UpdateOnlyTopState = true;
		base.Load();
	}

	public override void Unload()
	{
		base.core.UpdateOnlyTopState = false;
		base.Unload();
	}

	public override void Update()
	{
		touchMenu.Update();
		IsOpaque = Transition == TransType.None;
		base.core.AudioManager.MusicVolumeBox.Set("language-selector", 0.3f, inWorld: false);
		bat.Update();
		batPos += (batTarget - batPos) * 0.04f;
		base.Update();
	}

	public void UpdateLanguageButtons()
	{
		foreach (Language value in Enum.GetValues(typeof(Language)))
		{
			Button button = (Button)Enum.Parse(typeof(Button), value.ToString());
			if (touchMenu.HasButton(button))
			{
				TouchMenu<Button>.ButtonDesc buttonDesc = touchMenu[button];
				if (value == base.core.LocaleManager.CurrentLocale)
				{
					buttonDesc.Color = ButtonColor.Orange;
					batTarget = buttonDesc.Rectangle.Center;
				}
				else
				{
					buttonDesc.Color = ButtonColor.Stone;
				}
			}
		}
	}

	public override void Draw()
	{
		float num = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num) * (quick ? 0.8f : 1f));
		float num2 = (float)Tween.BackEaseOut(base.Trans, -base.core.Renderer.ScreenHeight, base.core.Renderer.ScreenHeight, base.TransDuration);
		for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
		{
			base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 13f, menuRect.Top + 21f + num2 - (float)i));
			base.core.Renderer["fg", 9000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 12f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
		}
		base.core.Renderer["fg", 9000, false].DrawSpriteS(block, menuRect.CenterTop.Shift(0f, num2), null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		base.core.Renderer["fg", 9000, false].DrawTextS(__(SId.MISC_language), menuRect.CenterTop.Shift(0f, 10f + num2), new TextProfile
		{
			Width = (int)menuRect.Width,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Center,
			Color = default(Color).FromRgb(9212825),
			SecondColor = default(Color).FromRgb(1645605),
			Decoration = TextDecoration.Extrude2,
			Font = Font.Bold,
			Scale = 1f
		});
		touchMenu.Draw();
		base.core.Renderer["fg", 11000, false].DrawSpriteS(bat.GetCurrentFrame(), batPos.Shift(Component._sin((float)base.ticks * 0.08f) * 20f, Component._cos((float)base.ticks * 0.08f + (float)Math.PI / 8f) * 12f), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
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
		foreach (Button value in Enum.GetValues(typeof(Button)))
		{
			if (touchMenu.HasButton(value))
			{
				touchMenu[value].Rectangle.Shift(0f, y);
			}
		}
		base.UpdateTransition();
	}

	private void OnButtonRelease(Button button)
	{
		if (button == Button.Back)
		{
			OnBackButtonPressed();
			return;
		}
		Language currentLocale = (Language)Enum.Parse(typeof(Language), button.ToString());
		base.core.LocaleManager.SetCurrentLocale(currentLocale);
		UpdateLanguageButtons();
	}

	public override void OnBackButtonPressed()
	{
		if (base.core.ProfileData.LanguageSelectorPending)
		{
			base.core.ProfileData.LanguageSelectorPending = false;
		}
		base.core.SaveOptions();
		base.core.ProfileData.SaveIntoStorage();
		TransitionOut(CoreEvent.PopState);
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		batTarget = new Vector2(base.core.Renderer.ScreenWidth + 60, (float)base.core.Renderer.ScreenHeight * 0.5f);
		base.OnBackButtonPressed();
	}
}
