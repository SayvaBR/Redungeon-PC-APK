using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class ContinueState : State
{
	protected override bool ShowContextPromptLegend => false;

	private enum Button
	{
		Skip,
		Pay,
		No
	}

	private enum ExitStyle
	{
		Normal,
		Revive
	}

	private int revivePrice;

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private bool fadeToBlack;

	private bool exiting;

	private ExitStyle exitStyle;

	private int exitT;

	private int exitDuration = 60;

	private Vector2 blockTop;

	private Vector2 shake;

	private Button topButton;

	private Button bottomButton;

	public ContinueState(int revivePrice, bool adsEnabled)
	{
		base.TransDuration = 30;
		ShowCoins = false;
		this.revivePrice = revivePrice;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 1000);
		shake = Vector2.Zero;
		SetupButtons();
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			topButton, bottomButton, Button.Skip);
	}

	public override void Load()
	{
		Screen("continue");
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		base.Load();
	}

	private void SetupButtons()
	{
		blockTop = base.core.Renderer.ScreenCenter.Shift(0f, -70f);
		int num = base.core.Renderer.ScreenWidth / 2;
		float num2 = blockTop.Y + 42f;
		int num3 = 62;
		topButton = Button.Pay;
		touchMenu.SetupButton(topButton, new RectangleF(num - num3 / 2, num2, num3, 30f), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, label: "^" + revivePrice);
		touchMenu[Button.Pay].Disabled = core.ProfileData.Coins < revivePrice;
		bottomButton = Button.No;
		touchMenu.SetupButton(bottomButton, new RectangleF(num - num3 / 2, num2 + 37f, num3, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, (bottomButton == Button.Pay) ? ("^" + revivePrice) : __(SId.MISC_no).ToUpper());
		touchMenu.SetupButton(Button.Skip, new RectangleF(num + 33, num2 - 45f, 26f, 26f), _(SpriteName.button_x), _(SpriteName.button_x_down));
	}

	public override void UpdateTransition()
	{
		int screenHeight = base.core.Renderer.ScreenHeight;
		float y = (float)Tween.BackEaseOut(TransD(2, 2), -screenHeight, screenHeight, base.TransDuration - 2 - 2);
		touchMenu[Button.Skip].Rectangle.Shift(0f, y);
		touchMenu[topButton].Rectangle.Shift(0f, y);
		touchMenu[bottomButton].Rectangle.Shift(0f, y);
		base.UpdateTransition();
	}

	private void ExitState(ExitStyle style)
	{
		exitT = 0;
		exiting = true;
		exitStyle = style;
		exitDuration = 1;
		switch (style)
		{
		case ExitStyle.Revive:
			exitDuration = 130;
			break;
		case ExitStyle.Normal:
			SendMessage(new PlaySoundMessage(SoundName.trans_1), 10);
			break;
		}
	}

	public override void Update()
	{
		base.core.AudioManager.MusicVolumeBox.Set("game over", 0.3f, inWorld: false);
		if (exiting)
		{
			if (exitStyle == ExitStyle.Revive && exitT == 90)
			{
				base.core.CurrentPlayState.ShowRespawnPoint();
			}
			if (exitStyle == ExitStyle.Revive && exitT == 20)
			{
				SendMessage(new PlaySoundMessage(SoundName.revive));
			}
			if (exitStyle == ExitStyle.Revive && exitT == 100)
			{
				SendMessage(new PlaySoundMessage(SoundName.trans_2));
			}
			if (exitStyle == ExitStyle.Revive && exitT < 50)
			{
				shake = SciHelper.GetRandomVectorInCircle(1f);
				touchMenu[Button.Skip].Rectangle.Shift(shake.X, shake.Y);
				touchMenu[topButton].Rectangle.Shift(shake.X, shake.Y);
				touchMenu[bottomButton].Rectangle.Shift(shake.X, shake.Y);
			}
			else
			{
				shake = Vector2.Zero;
			}
			exitT++;
			if (exitT == exitDuration)
			{
				switch (exitStyle)
				{
				case ExitStyle.Revive:
					TransitionOut(CoreEvent.Continue);
					break;
				case ExitStyle.Normal:
					TransitionOut(CoreEvent.GameOver);
					break;
				}
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		bool num = exiting && exitStyle == ExitStyle.Revive;
		float num2 = 1f - (float)base.Trans / (float)base.TransDuration;
		if (!num || exitT <= exitDuration - 60)
		{
			base.core.Renderer["fg", -1000, false].FillScreen(Color.Black * (0.8f - (fadeToBlack ? (-0.2f * num2) : num2)));
		}
		Vector2 vector = new Vector2(base.core.Renderer.ScreenCenter.X, touchMenu[Button.Skip].Rectangle.Top + 7f);
		vector += shake;
		if (num && exitT > 0)
		{
			if ((exitT > 20 && exitT < 30) || (exitT > 40 && exitT < 50))
			{
				base.core.Renderer["fg", 1055, false].DrawSpriteS(_(SpriteName.revive_bolt_1), vector.Shift(-11f, 1f), Color.White * Component._m(1f, 1f - (float)(exitT - ((exitT < 30) ? 25 : 45)) / 5f), null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			}
			if (exitT > 35 && exitT < 45)
			{
				base.core.Renderer["fg", 1055, false].DrawSpriteS(_(SpriteName.revive_bolt_2), vector.Shift(9f, 11f), Color.White * Component._m(1f, 1f - (float)(exitT - 40) / 5f), null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			}
			if (exitT > 30 && exitT < exitDuration - 60)
			{
				base.core.Renderer["fg", 1050, false].FillScreen(Color.Black * ((float)(exitT - 30) / 10f));
			}
			if (exitT >= exitDuration - 60 && exitT < exitDuration - 30)
			{
				base.core.Renderer["fg", 1050, false].FillScreen(Color.Black * (1f - (float)(exitT - (exitDuration - 60)) / 30f));
			}
			if (exitT > 35 && exitT < 50)
			{
				int num3 = (exitT - 35) / 3;
				num3++;
				Sprite sprite = _(base.core.CurrentCharDesc.ReviveSpriteName + num3);
				base.core.Renderer["fg", 1050, false].DrawSpriteS(sprite, vector.Shift(-26f, -29f).Shift(-25f, -5f));
			}
			if (exitT >= 50 && exitT < 90)
			{
				Sprite sprite2 = _(base.core.CurrentCharDesc.ReviveSpriteName + "5");
				base.core.Renderer["fg", 1050, false].DrawSpriteS(sprite2, vector.Shift(-26f, -29f).Shift(-25f, -5f) - base.core.CurrentCharDesc.ReviveShift * Component._m(1f, (float)(exitT - 50) / 30f), Color.White * (1f - (float)(exitT - 50) / 40f));
			}
			if (exitT > 50)
			{
				float num4 = Component._m(Component._M(0f, exitT - 50 - 40), 15f) / 15f;
				float num5 = 1f - 0.7f * num4;
				bool flag = num5 > 0.3f;
				Sprite sprite3 = (flag ? _(base.core.CurrentCharDesc.Portrait) : _(base.core.CurrentCharDesc.Icon));
				base.core.Renderer["fg", 1050, false].DrawSpriteS(sprite3, base.core.Renderer.ScreenCenter.Shift(0f, -30f + (30f + (float)base.core.Renderer.ScreenHeight * 0.1f) * num4) - (flag ? (sprite3.Link * num5) : Vector2.Zero) + base.core.CurrentCharDesc.ReviveShift * Component._M(0f, 1f - (float)(exitT - 50) / 30f), Color.White * Component._m(1f, (float)(exitT - 50) / 20f), new Vector2(flag ? num5 : (1f / Settings.GuiScale)), 0f, SpriteFlip.None, flag ? SpriteOrigin.TopLeft : SpriteOrigin.BottomCenter);
			}
			if (exitT > 40)
			{
				return;
			}
		}
		Sprite sprite4 = _(SpriteName.gui_chain);
		for (float num6 = vector.Y - (float)sprite4.Height; num6 > (float)(-sprite4.Height); num6 -= (float)sprite4.Height)
		{
			base.core.Renderer["fg", 2, false].DrawSpriteS(sprite4, new Vector2(vector.X - 35f - (float)(sprite4.Width / 2), num6));
			base.core.Renderer["fg", 2, false].DrawSpriteS(sprite4, new Vector2(vector.X + 35f - (float)(sprite4.Width / 2), num6));
		}
		Sprite sprite5 = _(SpriteName.continue_block);
		base.core.Renderer["fg"].DrawSpriteS(sprite5, vector, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		Sprite sprite6 = _(base.core.CurrentCharDesc.SkullSprite);
		base.core.Renderer["fg"].DrawSpriteS(sprite6, vector.Shift(-0.5f, 2.5f) - sprite6.Link);
		Sprite sprite7 = _(SpriteName.glow_huge);
		base.core.Renderer["fg", -1, false].DrawSpriteS(sprite7, vector, base.core.CurrentCharDesc.Color1 * (0.7f + 0.05f * Component._cos((float)base.ticks * 0.07f)), Vector2.One * (1.5f + 0.1f * Component._sin((float)base.ticks * 0.07f)), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg"].DrawTextS(__(SId.CONTINUE_continue), vector.Shift(0f, 25f), new TextProfile
		{
			Width = base.core.Renderer.ScreenWidth - 20,
			BoxAlignment = Alignment2D.Middle,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(9212825),
			SecondColor = default(Color).FromRgb(1645605),
			Decoration = TextDecoration.Extrude2,
			Font = Font.Bold
		});
		base.core.Renderer["fg"].DrawTextS(__(SId.MISC_total) + " ^" + base.core.ProfileData.Coins, vector.Shift(0f, 118f), new TextProfile
		{
			Width = sprite5.Width,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(16298824),
			SecondColor = Color.Black,
			Decoration = TextDecoration.Extrude1,
			Font = Font.Bold,
			Scale = 0.75f
		});
		base.core.Renderer["fg"].DrawTextS(base.core.CurrentPlayState.Session.Distance + "m", vector.Shift(-25f, 150f), new TextProfile
		{
			Width = 5,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Color = Color.White * (1f - num2),
			Decoration = TextDecoration.None,
			Font = Font.Bold,
			Scale = 1f
		});
		base.core.Renderer["fg"].DrawTextS(__(SId.CONTINUE_walked), vector.Shift(-25f, 160f), new TextProfile
		{
			Width = 5,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Color = Color.White * 0.8f * (1f - num2),
			Decoration = TextDecoration.None,
			Font = Font.Bold,
			Scale = 0.6f
		});
		base.core.Renderer["fg"].DrawTextS("^" + base.core.CurrentPlayState.Session.CollectedCoins, vector.Shift(23f, 150f), new TextProfile
		{
			Width = 5,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(16430139) * (1f - num2),
			Decoration = TextDecoration.None,
			Font = Font.Bold,
			Scale = 1f
		});
		base.core.Renderer["fg"].DrawTextS(__(SId.CONTINUE_collected), vector.Shift(25f, 160f), new TextProfile
		{
			Width = 5,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Color = default(Color).FromRgb(16430139) * 0.8f * (1f - num2),
			Decoration = TextDecoration.None,
			Font = Font.Bold,
			Scale = 0.6f
		});
		touchMenu.Draw();
		ContextPromptLegend.Draw(
			base.core.Renderer["fg", 18000, false],
			new Vector2(base.core.Renderer.ScreenWidth - 130f, base.core.Renderer.ScreenHeight - 20f),
			new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_confirm)),
			new ContextPrompt(PromptAction.Cancel, __(SId.PROMPT_skip)));
		base.Draw();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None && !exiting)
		{
			touchMenu.HandleInput();
			menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
			base.HandleInput();
		}
	}

#if DEBUG
	internal void PayForQa() => OnButtonRelease(Button.Pay);
#endif
	private void OnButtonRelease(Button button)
	{
		if (exiting || Transition != TransType.None) return;
		switch (button)
		{
		case Button.Skip:
			Die();
			break;
		case Button.Pay:
			if (base.core.ProfileData.Coins >= revivePrice)
			{
				Event(AnalyticsCategory.Ux, "pay-for-revive", revivePrice);
				base.core.ProfileData.Coins -= revivePrice;
				base.core.ProfileData.SaveIntoStorage();
				ExitState(ExitStyle.Revive);
			}
			break;
		case Button.No:
			Die();
			break;
		}
	}

	private void Die(bool force = false)
	{
		fadeToBlack = true;
		ExitState(ExitStyle.Normal);
	}

	public override void OnBackButtonPressed()
	{
		if (exiting) return;
		Die();
		base.OnBackButtonPressed();
	}
}
