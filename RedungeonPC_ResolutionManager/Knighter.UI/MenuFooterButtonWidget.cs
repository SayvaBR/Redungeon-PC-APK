using System;
using Knighter;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Botão largo de rodapé (Voltar, Restaurar Padrões, Aplicar). Usa o mesmo
/// <see cref="Renderer.DrawWoodenPanel"/> das linhas de opção, com uma
/// lavagem de cor por cima pra diferenciar cada botão (Aplicar em dourado,
/// por ex.) sem perder a textura de madeira entalhada.
/// </summary>
public class MenuFooterButtonWidget : Component, IMenuWidget
{
	private readonly SId labelId;
	private readonly Action onActivate;
	private readonly Color accentTint;

	private float focusGlow;
	private float animationTime;
	private bool focused;
	private float pressTime;
	public bool PointerPressed { get; set; }

	public MenuFooterButtonWidget(SId labelId, Action onActivate, Color accentTint)
	{
		this.labelId = labelId;
		this.onActivate = onActivate;
		this.accentTint = accentTint;
	}

	public RectangleF Bounds { get; set; }

	public bool IsFocusable => true;

	public bool ConsumesHorizontalInput => false;

	public void Update(float dt)
	{
		animationTime += dt;
		pressTime = MathF.Max(0f, pressTime - dt);
		focusGlow = MathHelper.Lerp(focusGlow, focused ? 1f : 0f, 1f - MathF.Exp(-20f * dt));
	}

	public void Draw(bool isFocused, int baseDepth)
	{
		Renderer renderer = R["fg", baseDepth, false];

		focused = isFocused;

		RectangleF bounds = Bounds;
		bool pressed = PointerPressed || pressTime > 0f;
		Sprite sprite = _(pressed ? SpriteName.button_pressed : SpriteName.button);
		Sprite cap = sprite.Reduce(0, 0, 1, 0);
		Sprite middle = sprite.Reduce(sprite.Width - 1, 0, 0, 0);
		float scale = bounds.Height / sprite.Height;
		float capWidth = cap.Width * scale;
		renderer.DrawSpriteS(cap, bounds.TopLeft, Color.White, new Vector2(scale));
		R["fg", baseDepth + 1, false].DrawSpriteS(middle, bounds.TopLeft + new Vector2(capWidth, 0), Color.White,
			new Vector2(MathF.Max(1f, bounds.Width - capWidth * 2f), scale));
		R["fg", baseDepth + 2, false].DrawSpriteS(cap, bounds.TopRight - new Vector2(capWidth, 0), Color.White,
			new Vector2(scale), 0f, SpriteFlip.Horizontal);

		if (isFocused && !pressed) MenuTheme.Shine(R, bounds, baseDepth + 2, animationTime);

		TextProfile labelProfile = TextProfile.OrangeBoldText.Alter(
			boxAlignment: Alignment2D.Middle,
			textAlignment: Alignment2D.Middle,
			width: (int)bounds.Width - 4,
			height: (int)bounds.Height,
			scale: 0.55f * MenuTheme.TextScale);
		R["fg", baseDepth + 3, false].DrawTextS(__(labelId).ToUpper(), bounds.Center + new Vector2(0, pressed ? 1.5f : -1f), labelProfile);
	}

	public void Activate()
	{
		pressTime = 0.12f;
		SendMessage(new PlaySoundMessage(SoundName.button_down));
		onActivate?.Invoke();
	}

	public void Adjust(int direction)
	{
	}

	public void HandleClick(Vector2 point)
	{
		Activate();
	}
}
