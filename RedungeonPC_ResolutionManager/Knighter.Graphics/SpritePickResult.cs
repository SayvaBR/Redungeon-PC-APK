using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

/// <summary>
/// Resultado de um Renderer.Pick() — informação sobre o sprite sob um ponto
/// da tela, para o Sprite Inspector (DevPanel). Não afeta a renderização.
/// </summary>
public struct SpritePickResult
{
	public bool Hit;

	public Texture2D Texture;

	public Rectangle SourceRect;

	/// <summary>Posição lógica (mesmo espaço de ScreenWidth/Height) onde o sprite foi desenhado.</summary>
	public Vector2 Position;

	public Vector2 Scale;

	public float Rotation;

	public Color Tint;

	public SpriteEffects Flip;

	public int Depth;

	/// <summary>Quem desenhou (ex.: a entidade), se foi desenhado dentro de um BeginOwnerScope. Null caso contrário.</summary>
	public object Owner;

	/// <summary>Pixel dentro do recorte do atlas (SourceRect) onde o clique caiu — útil pra destacar no preview.</summary>
	public Vector2 LocalPixel;
}
