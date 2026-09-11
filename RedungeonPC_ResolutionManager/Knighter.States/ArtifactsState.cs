using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class ArtifactsState : State
{
	private const float WindowScale = 0.85f;

	private float scale;

	private Sprite popupSprite;

	private Sprite xSprite;

	private RectangleF closeButtonRect;

	private Vector2 topLeft;

	public ArtifactsState()
	{
		base.TransDuration = 20;
		IsOverlay = true;
		popupSprite = base.core.SpriteManager.GetSprite(SpriteName.nitrome_popup);
		xSprite = base.core.SpriteManager.GetSprite(SpriteName.nitrome_popup_x);
		scale = (float)base.core.Renderer.ScreenWidth / (float)popupSprite.Width * 0.85f;
		topLeft = base.core.Renderer.ScreenCenter - new Vector2(popupSprite.Width, popupSprite.Height) * scale * 0.5f;
		closeButtonRect = new RectangleF((float)base.core.Renderer.ScreenWidth - topLeft.X - (float)xSprite.Width * scale, topLeft.Y, (float)xSprite.Width * scale, (float)xSprite.Height * scale);
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None && base.core.Input.WasPressed(GameAction.Confirm))
		{
			TransitionOut(CoreEvent.PopState);
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (item.State == TouchLocationState.Pressed && closeButtonRect.Contains(item.Position))
			{
				TransitionOut(CoreEvent.PopState);
				break;
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
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 100500, false].FillScreen(Color.Black * num * 0.75f);
		base.core.Renderer["fg", 100500, false].DrawSpriteS(popupSprite, topLeft, scale: new Vector2(scale, scale), tint: Color.White * num);
		base.Draw();
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

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.PopState);
		base.OnBackButtonPressed();
	}
}
