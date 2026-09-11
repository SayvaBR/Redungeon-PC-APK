using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class PromoState : State
{
	private const float WindowScale = 0.85f;

	private float scale;

	private Sprite popupSprite;

	private Sprite xSprite;

	private RectangleF closeButtonRect;

	private RectangleF imageButtonRect;

	private Vector2 topLeft;

	public PromoState()
	{
		base.TransDuration = 20;
		ShowCoins = false;
		IsOverlay = true;
		popupSprite = base.core.SpriteManager.GetSprite(SpriteName.nitrome_popup);
		xSprite = base.core.SpriteManager.GetSprite(SpriteName.nitrome_popup_x);
		scale = (float)base.core.Renderer.ScreenWidth / (float)popupSprite.Width * 0.85f;
		topLeft = base.core.Renderer.ScreenCenter - new Vector2(popupSprite.Width, popupSprite.Height) * scale * 0.5f;
		closeButtonRect = new RectangleF((float)base.core.Renderer.ScreenWidth - topLeft.X - (float)xSprite.Width * scale, topLeft.Y, (float)xSprite.Width * scale, (float)xSprite.Height * scale);
		imageButtonRect = new RectangleF(topLeft.X, topLeft.Y, (float)popupSprite.Width * scale, (float)popupSprite.Height * scale);
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None && base.core.Input.WasPressed(GameAction.Confirm))
		{
			Event(AnalyticsCategory.Ux, "follow-promo-link", base.core.CrossPromotion.ActiveSlot.PackageName);
			base.core.SystemCalls.OpenUrl(base.core.CrossPromotion.ActiveSlot.Url);
			TransitionOut(CoreEvent.PopState);
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (item.State == TouchLocationState.Pressed)
			{
				if (closeButtonRect.Contains(item.Position))
				{
					TransitionOut(CoreEvent.PopState);
					break;
				}
				if (imageButtonRect.Contains(item.Position))
				{
					Event(AnalyticsCategory.Ux, "follow-promo-link", base.core.CrossPromotion.ActiveSlot.PackageName);
					base.core.SystemCalls.OpenUrl(base.core.CrossPromotion.ActiveSlot.Url);
					TransitionOut(CoreEvent.PopState);
					break;
				}
			}
		}
		base.HandleInput();
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Draw()
	{
		Texture2D texture = base.core.SpriteManager.GetTexture("slot-image");
		Sprite sprite = new Sprite
		{
			X = 0,
			Y = 0,
			Width = texture.Width,
			Height = texture.Height,
			SrcWidth = texture.Width,
			SrcHeight = texture.Height,
			TextureName = "slot-image"
		};
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 100500, false].FillScreen(Color.Black * num * 0.75f);
		base.core.Renderer["fg", 100500, false].DrawSpriteS(sprite, topLeft + new Vector2(9f, 9f) * scale, scale: new Vector2(scale), tint: Color.White * num);
		base.core.Renderer["fg", 100500, false].DrawSpriteS(popupSprite, topLeft, scale: new Vector2(scale), tint: Color.White * num);
		base.Draw();
	}

	public override void Load()
	{
		Screen("promo");
		base.core.UpdateOnlyTopState = true;
		base.core.CrossPromotion.Showing = true;
		base.Load();
	}

	public override void Unload()
	{
		base.core.UpdateOnlyTopState = false;
		base.core.CrossPromotion.Showing = false;
		base.core.CrossPromotion.Shown = true;
		base.Unload();
	}

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.PopState);
		base.OnBackButtonPressed();
	}
}
