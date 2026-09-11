using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class SubMenuState : State
{
	protected override bool ShowContextPromptLegend => false;

	private enum Button
	{
		Options,
		Scores,
		Achievements,
		NoAds
	}

	private TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private int buttonsBottom;

	public SubMenuState()
	{
		base.TransDuration = 10;
		ShowCoins = false;
		IsOverlay = true;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 2000);
		touchMenu.SetupButton(Button.Options, new RectangleF(base.core.Renderer.ScreenWidth - 28, 0f, 30f, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Blue, "", _(SpriteName.icon_options), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.piston_retract);
		touchMenu.SetupButton(Button.Scores, new RectangleF(base.core.Renderer.ScreenWidth - 28, 39f, 30f, 26f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Blue, "", _(SpriteName.icon_scores), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 1f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.piston_retract);
		touchMenu.SetupButton(Button.Achievements, new RectangleF(base.core.Renderer.ScreenWidth - 28, 65f, 30f, 26f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Blue, "", _(SpriteName.icon_achievements), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 1f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.piston_retract);
		buttonsBottom = (int)touchMenu[Button.Achievements].Rectangle.Bottom;
		if (!base.core.ProfileData.AdsRemoved)
		{
			touchMenu.SetupButton(Button.NoAds, new RectangleF(base.core.Renderer.ScreenWidth - 28, 104f, 30f, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Blue, "", _(SpriteName.icon_no_ads), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.piston_retract);
			buttonsBottom = (int)touchMenu[Button.NoAds].Rectangle.Bottom;
		}
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			Button.Options, Button.Achievements, Button.Scores, Button.NoAds);
	}

	public override void Update()
	{
		touchMenu[Button.Scores].SeeThrough = true;
		base.Update();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.Options].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 40.0, -40.0, base.TransDuration));
		touchMenu[Button.Scores].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 40.0, -40.0, base.TransDuration));
		touchMenu[Button.Achievements].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 40.0, -40.0, base.TransDuration));
		if (!base.core.ProfileData.AdsRemoved)
		{
			touchMenu[Button.NoAds].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 40.0, -40.0, base.TransDuration));
		}
		base.UpdateTransition();
	}

	public override void Draw()
	{
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * (0.8f * num));
		base.core.Renderer["fg", 2000, false].DrawSpriteS(_(SpriteName.submenu_button_bg), touchMenu[Button.Options].Rectangle.BottomLeft.Shift(-3f, 0f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
		base.core.Renderer["fg", 2000, false].DrawSpriteS(_(SpriteName.submenu_button_bg), touchMenu[Button.Achievements].Rectangle.BottomLeft.Shift(-3f, 5f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
		if (!base.core.ProfileData.AdsRemoved)
		{
			base.core.Renderer["fg", 2000, false].DrawSpriteS(_(SpriteName.submenu_button_bg_narrow), touchMenu[Button.NoAds].Rectangle.BottomLeft.Shift(-3f, 3f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
		}
		touchMenu.Draw();
		ContextPromptLegend.Draw(
			base.core.Renderer["fg", 2010, false],
			new Vector2(10f, base.core.Renderer.ScreenHeight - 20f),
			new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_confirm)),
			new ContextPrompt(PromptAction.Cancel, __(SId.PROMPT_cancel)));
		base.Draw();
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		bool handled = touchMenu.HandleInput();
		if (!handled)
		{
			handled = menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
		}
		if (!handled)
		{
			foreach (TouchLocation item in base.core.TouchState)
			{
				if (item.State == TouchLocationState.Pressed && (item.Position.X < (float)(base.core.Renderer.ScreenWidth - 34) || item.Position.Y > (float)buttonsBottom))
				{
					TransitionOut(CoreEvent.PopState);
					SendMessage(new PlaySoundMessage(SoundName.piston_retract));
				}
			}
		}
		base.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		TransitionOut(CoreEvent.PopState);
		switch (button)
		{
		case Button.Options:
			SendMessage(new CoreEventMessage(CoreEvent.ShowOptions), 15);
			break;
		case Button.Scores:
			base.core.SystemCalls.ShowLeaderboards();
			break;
		case Button.Achievements:
			SendMessage(new PushStateMessage(new FactsState()), 15);
			break;
		case Button.NoAds:
			base.core.TimerManager.CreateTimer(15, 1, 1, delegate
			{
				base.core.Store.PurchaseProduct(Iap.RemoveAds, delegate
				{
				});
			});
			break;
		}
	}

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.PopState);
		SendMessage(new PlaySoundMessage(SoundName.piston_retract));
		base.OnBackButtonPressed();
	}
}
