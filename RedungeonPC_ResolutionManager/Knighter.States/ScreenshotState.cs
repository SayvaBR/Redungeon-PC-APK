using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class ScreenshotState : State
{
	private enum Button
	{
		Share,
		Back
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private readonly Vector2 corner;

	private Sprite screenshotSprite;

	private float previewWidth;

	private string shareMessage;

	public ScreenshotState(Vector2 corner, float previewWidth, string shareMessage)
	{
		this.corner = corner;
		this.previewWidth = previewWidth;
		this.shareMessage = shareMessage;
		base.TransDuration = 30;
		IsOverlay = true;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 5000);
		int num = base.core.Renderer.ScreenHeight - 40;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.Back, new RectangleF(10 + 3 * num2 / 2 + 1, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.MISC_btn_back));
		touchMenu.SetupButton(Button.Share, new RectangleF(10 + num2 / 2, num, num2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(Settings.ShareIcon), icon: true, iconIsPicture: false, blink: true);
		touchMenu[Button.Share].Hidden = true;
		touchMenu[Button.Back].Rectangle = new RectangleF(core.Renderer.ScreenCenter.X - 70f, num, 140f, 30f);
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, Button.Back);
		screenshotSprite = base.core.SpriteManager.MakeFullSpriteFromScreenshot(base.core.GameplayScreenshot);
	}

	public override void Load()
	{
		Screen("screenshot");
		base.Load();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.Share].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, 80.0, -80.0, base.TransDuration));
		touchMenu[Button.Back].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, 80.0, -80.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void Update()
	{
		IsOpaque = Transition == TransType.None;
		base.Update();
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None || touchMenu.HandleInput()
			|| menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds))
		{
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (item.State == TouchLocationState.Pressed)
			{
				OnBackButtonPressed();
				break;
			}
		}
		base.HandleInput();
	}

	public override void Draw()
	{
		float num = (float)base.Trans / (float)base.TransDuration;
		bool flag = num < 0.1f;
		base.core.Renderer["fg", 3999, false].FillScreen(Color.Black * num);
		float num2 = Component._sin(num * (float)Math.PI);
		Vector2 vector = corner.Shift(0f, 10f - num2 * previewWidth * 2f);
		Vector2 vector2 = base.core.Renderer.ScreenCenter.Shift(0f, -20f);
		vector += (vector2 - vector) * num;
		float x = previewWidth + ((float)base.core.Renderer.ScreenWidth * 0.8f - previewWidth) * num;
		float num3 = previewWidth + ((float)base.core.Renderer.ScreenHeight * 0.8f - previewWidth) * num;
		num3 = Component._M(1f, num3 * (1f - Math.Abs(num2)));
		float rotation = Component._sin((float)base.ticks * 0.05f) * 0.1f * (1f - num * 0.8f);
		Vector2 vector3 = new Vector2(x, num3);
		base.core.Renderer["fg", flag ? 901 : 4001, false].DrawSpriteS(base.core.SpriteManager.Pixel, vector, Color.Lerp(Color.LightGray, Color.DarkGray, Math.Abs(num2)), vector3.Shift(0f, -0.02f * vector3.Y), rotation, SpriteFlip.None, SpriteOrigin.Center);
		int num4 = (int)MathHelper.Lerp(screenshotSprite.Width / 5, 0f, num);
		int num5 = (int)MathHelper.Lerp((screenshotSprite.Height - num4 * 3) / 2, 0f, num);
		Sprite sprite = screenshotSprite.Reduce(num4, num5, num4, num5);
		base.core.Renderer["fg", flag ? 901 : 4001, false].DrawSpriteS(sprite, vector, Color.Lerp(Color.White, Color.White * 0.4f, Component._sin(num * (float)Math.PI)), vector3 / new Vector2(sprite.Width, sprite.Height) * 0.95f, rotation, SpriteFlip.None, SpriteOrigin.Center);
		touchMenu.Draw();
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.paper_reverse));
		IsOpaque = false;
		TransitionOut(CoreEvent.PopState);
		base.OnBackButtonPressed();
	}
}
