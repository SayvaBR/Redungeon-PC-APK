using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Desenha um botão de largura arbitrária a partir de um sprite pequeno de
/// "ponta" (ex.: <c>button</c>, 5×32), esticando uma fatia de 1px do meio —
/// exatamente a técnica que <see cref="TouchMenu{T}"/> já usa para os
/// botões do jogo. Criado para não duplicar essa lógica em cada lugar novo
/// (abas, botões de rodapé) que precisa de um botão "de verdade" em vez de
/// um retângulo de cor lisa.
/// </summary>
public static class PixelButtonPainter
{
	public static void DrawStretched(Renderer renderer, Sprite endCap, RectangleF rect, Color tint)
	{
		float y = rect.Y + (rect.Height - endCap.Height) / 2f;
		float stretchWidth = rect.Width - endCap.Width * 2f + 1f;

		renderer.DrawSpriteS(endCap, new Vector2(rect.Left, y), tint);

		if (stretchWidth > 0f)
		{
			Sprite middleSlice = endCap.Reduce(endCap.Width - 1, 0, 0, 0);
			if (middleSlice.Width > 0)
			{
				renderer.DrawSpriteS(middleSlice, new Vector2(rect.Left + endCap.Width, y), tint, new Vector2(stretchWidth, 1f));
			}
		}

		renderer.DrawSpriteS(endCap, new Vector2(rect.Right - endCap.Width, y), tint, null, 0f, SpriteFlip.Horizontal);
	}
}
