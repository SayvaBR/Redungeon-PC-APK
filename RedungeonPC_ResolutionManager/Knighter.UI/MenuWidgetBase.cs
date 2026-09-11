using Knighter;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Layout compartilhado por toda linha de opção do menu: um pequeno painel
/// de madeira (reaproveitando <see cref="Renderer.DrawWoodenPanel"/>, o
/// mesmo método já usado na loja) com ícone à esquerda, título + descrição
/// no meio, e uma área de controle à direita cujo desenho fica a cargo da
/// subclasse (botão, toggle, seletor, slider...).
///
/// Importante: todas as medidas aqui são em "pixels lógicos" do jogo — a
/// mesma escala minúscula (a tela inteira tem só ~266px lógicos de altura)
/// usada pelo painel de Opções original. Não confundir com pixels reais de
/// tela.
///
/// Cada elemento desenhado (fundo, contorno de foco, ícone, título,
/// descrição, controle) usa uma profundidade própria dentro da faixa
/// reservada pra este widget — nunca reaproveitar o mesmo valor pra duas
/// coisas, senão a ordem de desenho entre elas deixa de ser garantida e o
/// fundo pode acabar cobrindo o texto.
/// </summary>
public abstract class MenuWidgetBase : Component, IMenuWidget
{
	private static readonly Color DescriptionColor = Color.White;

	protected readonly SId TitleId;
	protected readonly SId DescriptionId;
	protected readonly SpriteName? IconSprite;

	public RectangleF Bounds { get; set; }

	public virtual bool IsFocusable => true;

	public virtual bool ConsumesHorizontalInput => false;

	/// <summary>Largura reservada para a área de controle (toggle/seletor/slider) na direita da linha.</summary>
	protected virtual float ControlAreaWidth => 70f;

	/// <summary>Área de controle calculada no último Draw — usada por HandleClick para saber onde o mouse clicou.</summary>
	private RectangleF lastControlArea;

	private float focusGlow;
	private float animationTime;
	private bool focused;

	protected MenuWidgetBase(SId titleId, SId descriptionId, SpriteName? iconSprite = null)
	{
		TitleId = titleId;
		DescriptionId = descriptionId;
		IconSprite = iconSprite;
	}

	public virtual void Update(float dt)
	{
		animationTime += dt;
		focusGlow = MathHelper.Lerp(focusGlow, focused ? 1f : 0f, 1f - System.MathF.Exp(-20f * dt));
	}

	public void Draw(bool isFocused, int baseDepth)
	{
		focused = isFocused;

		RectangleF bounds = Bounds;
		MenuTheme.Panel(R, bounds, baseDepth, focusGlow);


		if (isFocused) MenuTheme.Shine(R, bounds, baseDepth + 2, animationTime, 0.10f);
		float textLeft = bounds.X + 10f;
		float controlLeft = bounds.Right - ControlAreaWidth;
		float textWidth = controlLeft - textLeft - 3f;

		TextProfile titleProfile = TextProfile.OrangeBoldText.Alter(
			boxAlignment: Alignment2D.Left,
			textAlignment: Alignment2D.Left,
			width: (int)textWidth,
			height: (int)(bounds.Height * 0.5f),
			scale: 0.75f * MenuTheme.TextScale);
		R["fg", baseDepth + 3, false].DrawTextS(__(TitleId).ToUpper(), new Vector2(textLeft, bounds.Y + 2f), titleProfile);

		TextProfile descProfile = TextProfile.GravestoneText.Alter(
			color: DescriptionColor,
			boxAlignment: Alignment2D.Left,
			textAlignment: Alignment2D.Left,
			width: (int)textWidth,
			height: (int)(bounds.Height * 0.5f),
			scale: 0.55f * MenuTheme.TextScale);
		R["fg", baseDepth + 4, false].DrawTextS(__(DescriptionId), new Vector2(textLeft, bounds.Y + bounds.Height * 0.52f), descProfile);

		RectangleF controlArea = new RectangleF(controlLeft, bounds.Y, ControlAreaWidth - 4f, bounds.Height);
		lastControlArea = controlArea;
		DrawControl(controlArea, isFocused, baseDepth + 5);
	}

	private void DrawFocusOutline(Renderer renderer, RectangleF bounds, float glow, int depth)
	{
		Color outline = TextProfile.OrangeLight * (0.5f * glow);
		renderer["fg", depth, false].DrawRectangleS(new RectangleF(bounds.X, bounds.Y, bounds.Width, 1f), outline);
		renderer["fg", depth, false].DrawRectangleS(new RectangleF(bounds.X, bounds.Bottom - 1f, bounds.Width, 1f), outline);
		renderer["fg", depth, false].DrawRectangleS(new RectangleF(bounds.X, bounds.Y, 1f, bounds.Height), outline);
		renderer["fg", depth, false].DrawRectangleS(new RectangleF(bounds.Right - 1f, bounds.Y, 1f, bounds.Height), outline);
	}

	/// <summary>
	/// Desenha o controle específico do widget (toggle, seletor de valor,
	/// slider, botão) dentro da área reservada à direita. depth já é único
	/// pra esta chamada — mas se o controle desenhar mais de uma peça (ex.:
	/// duas setas), use depth, depth+1, depth+2... para cada uma.
	/// </summary>
	protected abstract void DrawControl(RectangleF controlArea, bool isFocused, int depth);

	public virtual void Activate()
	{
	}

	public virtual void Adjust(int direction)
	{
	}

	public virtual void HandleClick(Vector2 point)
	{
		if (ConsumesHorizontalInput && lastControlArea.Width > 0f)
		{
			Adjust(point.X < lastControlArea.Center.X ? -1 : 1);
			return;
		}
		Activate();
	}

	protected void PlayNavigationSound()
	{
		SendMessage(new PlaySoundMessage(SoundName.button_up, 0.6f));
	}

	protected void PlayConfirmSound()
	{
		SendMessage(new PlaySoundMessage(SoundName.button_down));
	}

	protected void PlayValueChangeSound()
	{
		SendMessage(new PlaySoundMessage(SoundName.swoosh_1, 0.5f));
	}
}
