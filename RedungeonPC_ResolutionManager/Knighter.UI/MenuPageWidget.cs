using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

public sealed class MenuPageWidget : Component, IMenuWidget
{
	private readonly Func<int> page;
	private readonly Func<int> count;
	private readonly Action<int> change;
	public MenuPageWidget(Func<int> page, Func<int> count, Action<int> change)
	{ this.page = page; this.count = count; this.change = change; }
	public RectangleF Bounds { get; set; }
	public bool IsFocusable => true;
	public bool ConsumesHorizontalInput => true;
	public void Update(float dt) { }
	public void Adjust(int direction) => change(direction);
	public void Activate() => Adjust(1);
	public void HandleClick(Vector2 point) => Adjust(point.X < Bounds.Center.X ? -1 : 1);
	public void Draw(bool focused, int depth)
	{
		R["fg", depth, false].DrawTextS($"<   {__(SId.PC_page)} {page() + 1} / {count()}   >", Bounds.Center,
			TextProfile.OrangeBoldText.Alter(width: (int)Bounds.Width, height: (int)Bounds.Height,
				scale: 0.5f, boxAlignment: Alignment2D.Middle, textAlignment: Alignment2D.Middle));
	}
}
