using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Linha de opção do tipo "◀ valor ▶" (ex.: Resolução, Idioma, Tipo de
/// Controle). getLabel(index) decide o texto mostrado; o widget só sabe
/// navegar entre 0..count-1. Usa o sprite real de seta do jogo
/// (<c>button_arrow_left</c>), reduzido para caber na escala do menu.
/// </summary>
public class MenuValueSelectorWidget : MenuWidgetBase
{
	private const float ArrowScale = 0.55f;

	private readonly Func<int> getIndex;
	private readonly Action<int> setIndex;
	private readonly Func<int, string> getLabel;
	private readonly int count;
	private readonly bool wrap;

	public MenuValueSelectorWidget(SId titleId, SId descriptionId, SpriteName iconSprite, int count, Func<int> getIndex, Action<int> setIndex, Func<int, string> getLabel, bool wrap = true)
		: base(titleId, descriptionId, iconSprite)
	{
		this.count = count;
		this.getIndex = getIndex;
		this.setIndex = setIndex;
		this.getLabel = getLabel;
		this.wrap = wrap;
	}

	public override bool ConsumesHorizontalInput => true;

	protected override float ControlAreaWidth => 105f * MenuTheme.TextScale;

	protected override void DrawControl(RectangleF controlArea, bool isFocused, int depth)
	{
		int index = getIndex();
		Sprite arrow = _(SpriteName.button_arrow_left);
		Vector2 arrowSize = arrow.Size * ArrowScale;

		Vector2 leftPos = new Vector2(controlArea.Left, controlArea.Center.Y - arrowSize.Y / 2f);
		Vector2 rightPos = new Vector2(controlArea.Right - arrowSize.X, controlArea.Center.Y - arrowSize.Y / 2f);

		R["fg", depth, false].DrawSpriteS(arrow, leftPos, CanGoLeft(index) ? Color.White : Color.White * 0.4f, Vector2.One * ArrowScale);
		R["fg", depth + 1, false].DrawSpriteS(arrow, rightPos, CanGoRight(index) ? Color.White : Color.White * 0.4f, Vector2.One * ArrowScale, 0f, SpriteFlip.Horizontal);

		TextProfile valueProfile = TextProfile.OrangeBoldText.Alter(
			boxAlignment: Alignment2D.Middle,
			textAlignment: Alignment2D.Middle,
			width: (int)(controlArea.Width - arrowSize.X * 2f - 4f),
			height: (int)controlArea.Height,
			scale: 0.6f * MenuTheme.TextScale);
		R["fg", depth + 2, false].DrawTextS(getLabel(index), controlArea.Center, valueProfile);
	}

	public override void Adjust(int direction)
	{
		int index = getIndex();
		int next = index + direction;
		if (wrap)
		{
			next = ((next % count) + count) % count;
		}
		else
		{
			next = Math.Clamp(next, 0, count - 1);
		}

		if (next != index)
		{
			setIndex(next);
			PlayValueChangeSound();
		}
	}

	public override void Activate()
	{
		Adjust(1);
	}

	private bool CanGoLeft(int index) => wrap || index > 0;

	private bool CanGoRight(int index) => wrap || index < count - 1;
}
