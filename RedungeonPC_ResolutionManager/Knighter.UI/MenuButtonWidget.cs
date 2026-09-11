using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Linha de ação simples, sem valor associado (ex.: item de Acessibilidade
/// futuro do tipo "Abrir tutorial"). Os botões inferiores do menu (Voltar,
/// Restaurar Padrões, Aplicar) terão um widget próprio, mais largo, criado
/// numa fase futura — este aqui é só para itens dentro da lista de opções.
/// </summary>
public class MenuButtonWidget : MenuWidgetBase
{
	private readonly Action onActivate;

	public MenuButtonWidget(SId titleId, SId descriptionId, SpriteName iconSprite, Action onActivate)
		: base(titleId, descriptionId, iconSprite)
	{
		this.onActivate = onActivate;
	}

	protected override float ControlAreaWidth => 20f;

	protected override void DrawControl(RectangleF controlArea, bool isFocused, int depth)
	{
		Sprite arrow = _(SpriteName.button_arrow_left);
		R["fg", depth, false].DrawSpriteS(arrow, controlArea.Center, isFocused ? Color.White : Color.White * 0.6f, Vector2.One * 0.5f, 0f, SpriteFlip.Horizontal, SpriteOrigin.Center);
	}

	public override void Activate()
	{
		PlayConfirmSound();
		onActivate?.Invoke();
	}
}
