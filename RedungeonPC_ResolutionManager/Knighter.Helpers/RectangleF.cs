using Microsoft.Xna.Framework;

namespace Knighter.Helpers;

public class RectangleF
{
	private float x;

	private float y;

	public float Width;

	public float Height;

	public Vector2 Offset { get; private set; }

	public float X
	{
		get
		{
			return x + Offset.X;
		}
		set
		{
			x = value;
		}
	}

	public float Y
	{
		get
		{
			return y + Offset.Y;
		}
		set
		{
			y = value;
		}
	}

	public float Left => X;

	public float Right => X + Width;

	public float Top => Y;

	public float Bottom => Y + Height;

	public float TrueLeft => x;

	public float TrueRight => x + Width;

	public float TrueTop => y;

	public float TrueBottom => y + Height;

	public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

	public Vector2 CenterTop => new Vector2(X + Width / 2f, Y);

	public Vector2 CenterBottom => new Vector2(X + Width / 2f, Y + Height);

	public Vector2 TopLeft => new Vector2(X, Y);

	public Vector2 TopRight => new Vector2(X + Width, Y);

	public Vector2 BottomLeft => new Vector2(X, Y + Height);

	public Vector2 BottomRight => new Vector2(X + Width, Y + Height);

	public RectangleF(float x, float y, float width, float height)
	{
		this.x = x;
		this.y = y;
		Width = width;
		Height = height;
		Offset = Vector2.Zero;
	}

	public void Shift(float x = 0f, float y = 0f)
	{
		Offset = new Vector2(x, y);
	}

	public static explicit operator RectangleF(Rectangle r)
	{
		return new RectangleF(r.X, r.Y, r.Width, r.Height);
	}

	public bool Contains(Vector2 point)
	{
		if (point.X > Left && point.X < Right && point.Y > Top)
		{
			return point.Y < Bottom;
		}
		return false;
	}

	public bool Overlaps(RectangleF other)
	{
		if (Left <= other.Right && other.Left <= Right && Top <= other.Bottom)
		{
			return other.Top <= Bottom;
		}
		return false;
	}

	public RectangleF Clone()
	{
		RectangleF rectangleF = new RectangleF(x, y, Width, Height);
		rectangleF.Shift(Offset.X, Offset.Y);
		return rectangleF;
	}

	public RectangleF Grow(float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
	{
		RectangleF rectangleF = Clone();
		rectangleF.X += left;
		rectangleF.Y += top;
		rectangleF.Width += right - left;
		rectangleF.Height += bottom - top;
		return rectangleF;
	}
}
