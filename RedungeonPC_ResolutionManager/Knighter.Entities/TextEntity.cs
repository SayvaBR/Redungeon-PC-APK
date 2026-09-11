using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class TextEntity : Entity
{
	private string text;

	private Alignment2D align;

	public TextEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		height = desc["height"];
		text = desc.Str("text");
		int num = desc["align"];
		if (desc.Flipped)
		{
			num = -num;
		}
		align = ((num == 0) ? Alignment2D.Middle : ((num < 0) ? Alignment2D.RightMiddle : Alignment2D.LeftMiddle));
	}

	public TextEntity(int x, int y, string text)
		: base(x, y, 1f, 1f)
	{
		height = 1f;
		this.text = text;
		align = Alignment2D.Middle;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void Draw()
	{
		float zoom = base.core.CurrentPlayState.Camera.Zoom;
		base.core.Renderer["bg", 2, false].DrawTextW(text, base.WorldCenter, TextProfile.OrangeBoldText.Alter(default(Color).FromRgb(3685192), null, TextDecoration.None, (int)(150f * zoom), (int)(height * 16f * zoom), scale: 0.75f * zoom, boxAlignment: Alignment2D.Middle, textAlignment: align));
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return true;
	}
}
