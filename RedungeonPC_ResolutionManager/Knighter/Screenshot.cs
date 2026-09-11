using Microsoft.Xna.Framework.Graphics;

namespace Knighter;

public class Screenshot : Component
{
	public readonly Texture2D Texture;

	public int Width => Texture.Width;

	public int Height => Texture.Height;

	public Screenshot(Texture2D texture)
	{
		Texture = texture;
	}
}
