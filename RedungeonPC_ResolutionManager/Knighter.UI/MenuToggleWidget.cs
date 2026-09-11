using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Linha de opção do tipo liga/desliga (ex.: Música, Efeitos Sonoros, Tela
/// Cheia, Vibração). Usa os mesmos sprites <c>ui_toggle_0..4</c> e a mesma
/// animação de deslize que o resto do jogo já usa em <see cref="TouchMenu{T}"/>,
/// só que aqui reagindo também a teclado/gamepad. Não guarda o valor
/// internamente — lê e escreve através dos delegates recebidos.
/// </summary>
public class MenuToggleWidget : MenuWidgetBase
{
	private readonly Func<bool> getValue;
	private readonly Action<bool> setValue;
	private readonly SId onLabel;
	private readonly SId offLabel;

	private int toggleT;
	private bool lastDrawnValue;
	private bool hasDrawnOnce;

	public MenuToggleWidget(SId titleId, SId descriptionId, SpriteName iconSprite, Func<bool> getValue, Action<bool> setValue, SId onLabel, SId offLabel)
		: base(titleId, descriptionId, iconSprite)
	{
		this.getValue = getValue;
		this.setValue = setValue;
		this.onLabel = onLabel;
		this.offLabel = offLabel;
	}

	protected override float ControlAreaWidth => 34f;

	protected override void DrawControl(RectangleF controlArea, bool isFocused, int depth)
	{
		bool value = getValue();
		if (hasDrawnOnce && value != lastDrawnValue)
		{
			toggleT = 9;
		}
		lastDrawnValue = value;
		hasDrawnOnce = true;

		if (toggleT > 0)
		{
			toggleT--;
		}

		int frame = value ? 4 : 0;
		if (toggleT > 0)
		{
			frame += (value ? -1 : 1) * toggleT / 3;
		}

		Sprite toggleSprite = _("ui_toggle_" + frame, "ui_toggle_0");
		Vector2 position = new Vector2(controlArea.Right - toggleSprite.Width, controlArea.Center.Y - toggleSprite.Height / 2f);
		R["fg", depth, false].DrawSpriteS(toggleSprite, position);
	}

	public override void Activate()
	{
		setValue(!getValue());
		PlayValueChangeSound();
	}

	public override void Adjust(int direction)
	{
		Activate();
	}
}
