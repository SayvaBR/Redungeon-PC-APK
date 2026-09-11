#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Knighter.Graphics;

namespace Knighter;

public class InputPanel : IDevPanel
{
	public string Title => "Input";

	public Color TitleColor => new Color(200, 140, 40);

	public void Update(GameTime gameTime)
	{
	}

	public void CollectLines(Renderer renderer, List<string> lines)
	{
		Core core = Core.Instance;
		MouseState mouse = Mouse.GetState();
		lines.Add($"Mouse: {mouse.X},{mouse.Y}  L:{mouse.LeftButton}");

		TouchCollection touches = core.TouchState;
		lines.Add($"Touches: {touches.Count}");
		for (int i = 0; i < touches.Count && i < 3; i++)
		{
			TouchLocation t = touches[i];
			lines.Add($"  [{i}] {t.State} @ {t.Position.X:0},{t.Position.Y:0}");
		}
	}
}
#endif
