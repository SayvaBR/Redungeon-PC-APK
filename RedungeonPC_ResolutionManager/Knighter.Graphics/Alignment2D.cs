namespace Knighter.Graphics;

public class Alignment2D
{
	public static Alignment2D Left = new Alignment2D
	{
		Horizontal = Alignment.Min,
		Vertical = Alignment.Min
	};

	public static Alignment2D Right = new Alignment2D
	{
		Horizontal = Alignment.Max,
		Vertical = Alignment.Min
	};

	public static Alignment2D Center = new Alignment2D
	{
		Horizontal = Alignment.Center,
		Vertical = Alignment.Min
	};

	public static Alignment2D Middle = new Alignment2D
	{
		Horizontal = Alignment.Center,
		Vertical = Alignment.Center
	};

	public static Alignment2D LeftMiddle = new Alignment2D
	{
		Horizontal = Alignment.Min,
		Vertical = Alignment.Center
	};

	public static Alignment2D RightMiddle = new Alignment2D
	{
		Horizontal = Alignment.Max,
		Vertical = Alignment.Center
	};

	public static Alignment2D BottomCenter = new Alignment2D
	{
		Horizontal = Alignment.Center,
		Vertical = Alignment.Max
	};

	public static Alignment2D RightBottom = new Alignment2D
	{
		Horizontal = Alignment.Max,
		Vertical = Alignment.Max
	};

	public Alignment Horizontal;

	public Alignment Vertical;
}
