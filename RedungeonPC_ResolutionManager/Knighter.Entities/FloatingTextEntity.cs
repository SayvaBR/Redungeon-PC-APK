using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class FloatingTextEntity : Entity
{
	private readonly string text;

	private readonly Color color;

	private readonly float scale;

	private int age;

	private int ttl;

	public FloatingTextEntity(Vector2 coordinates, string text, Color? color = null, float scale = 1f, int ttl = 60)
		: base(coordinates.X, coordinates.Y, 0f, 0f)
	{
		this.text = text;
		this.color = color ?? Color.White;
		this.scale = scale;
		this.ttl = ttl;
	}

	public override void Update()
	{
		if (++age > ttl)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		base.Update();
	}

	public override void Draw()
	{
		if (!base.core.TakingScreenshot)
		{
			float num = (float)age / (float)ttl;
			base.core.Renderer["fg", -100, false].DrawTextW(text, base.WorldCenter.Shift(0f, -15f - (float)age * 0.35f), new TextProfile
			{
				BoxAlignment = Alignment2D.Middle,
				TextAlignment = Alignment2D.Middle,
				Color = color * (1f - num * num * num),
				Font = Font.Bold,
				Decoration = TextDecoration.Contour,
				SecondColor = Color.Black,
				Width = (int)(100f * scale * (1f - num * num * num)),
				Scale = scale * (1f - num * num * num)
			});
			base.Draw();
		}
	}
}
