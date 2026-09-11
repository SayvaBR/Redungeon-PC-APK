using Knighter;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Linha não-interativa usada para dividir grupos dentro de uma categoria,
/// ou (sem label) como um placeholder de "Em breve" centralizado — usado
/// pelas categorias que ainda não têm opções reais. Não participa da
/// navegação por teclado/gamepad/foco.
/// </summary>
public class MenuSeparatorWidget : Component, IMenuWidget
{
	private static readonly Color LineColor = default(Color).FromRgb(5000268);

	private readonly SId? labelId;
	private readonly bool big;

	public MenuSeparatorWidget(SId? labelId = null, bool big = false)
	{
		this.labelId = labelId;
		this.big = big;
	}

	public RectangleF Bounds { get; set; }

	public bool IsFocusable => false;

	public bool ConsumesHorizontalInput => false;

	public void Update(float dt)
	{
	}

	public void Draw(bool isFocused, int baseDepth)
	{
		Renderer renderer = R["fg", baseDepth, false];

		if (labelId.HasValue)
		{
			TextProfile labelProfile = (big ? TextProfile.OrangeBoldText : TextProfile.GravestoneText).Alter(
				boxAlignment: Alignment2D.Center,
				textAlignment: Alignment2D.Middle,
				width: (int)Bounds.Width,
				height: (int)Bounds.Height,
				scale: big ? 0.55f : 0.4f);
			renderer.DrawTextS(__(labelId.Value), new Vector2(Bounds.Center.X, Bounds.Y + Bounds.Height * 0.4f), labelProfile);
			return;
		}

		float lineY = Bounds.Center.Y;
		renderer.DrawRectangleS(new RectangleF(Bounds.X, lineY, Bounds.Width, 1f), LineColor);
	}

	public void Activate()
	{
	}

	public void Adjust(int direction)
	{
	}

	public void HandleClick(Vector2 point)
	{
	}
}
