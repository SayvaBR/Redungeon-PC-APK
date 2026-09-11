using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public class TextProfile
{
	public static Color OrangeLight = default(Color).FromRgb(16430139);

	public static Color OrangeMiddle = default(Color).FromRgb(11688223);

	public static Color GravestoneScript = default(Color).FromRgb(1908778);

	public static TextProfile OrangeBoldText = new TextProfile
	{
		Color = OrangeLight,
		SecondColor = OrangeMiddle,
		Decoration = TextDecoration.Extrude1,
		Font = Font.Bold,
		BoxAlignment = Alignment2D.Middle,
		TextAlignment = Alignment2D.Middle
	};

	public static TextProfile GravestoneText = new TextProfile
	{
		Color = GravestoneScript,
		SecondColor = null,
		Decoration = TextDecoration.None,
		Font = Font.Thin,
		BoxAlignment = Alignment2D.Center,
		TextAlignment = Alignment2D.Center
	};

	public Color Color;

	public Color? SecondColor;

	public TextDecoration Decoration;

	public int Width;

	public int Height;

	public Alignment2D BoxAlignment;

	public Alignment2D TextAlignment;

	public Font Font;

	public float Scale = 1f;

	public TextProfile Alter(Color? color = null, Color? secondColor = null, TextDecoration? decoration = null, int? width = null, int? height = null, Alignment2D boxAlignment = null, Alignment2D textAlignment = null, Font? font = null, float? scale = null)
	{
		return new TextProfile
		{
			Color = (color ?? Color),
			SecondColor = (secondColor ?? SecondColor),
			Decoration = (decoration ?? Decoration),
			Width = (width ?? Width),
			Height = (height ?? Height),
			BoxAlignment = (boxAlignment ?? BoxAlignment),
			TextAlignment = (textAlignment ?? TextAlignment),
			Font = (font ?? Font),
			Scale = (scale ?? Scale)
		};
	}
}
