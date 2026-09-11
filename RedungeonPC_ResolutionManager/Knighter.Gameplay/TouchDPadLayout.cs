using System;
using Knighter.Helpers;

namespace Knighter.Gameplay;

/// <summary>Touch geometry in logical screen units, shared by creation and rotation.</summary>
public sealed class TouchDPadLayout
{
	public RectangleF North { get; private init; }
	public RectangleF East { get; private init; }
	public RectangleF South { get; private init; }
	public RectangleF West { get; private init; }
	public RectangleF Action { get; private init; }
	public RectangleF CompactBounds { get; private init; }
	public float ArrowSize { get; private init; }

	public static TouchDPadLayout Create(float width, float height, bool leftHanded, bool compact)
	{
		const float margin = 14f;
		bool portrait = height > width;
		// Leave the middle of the landscape screen for the dungeon. In portrait,
		// the controls occupy at most the bottom third, below the player.
		float cell = portrait
			? MathF.Min(60f, MathF.Min((width - 2f * margin) / 3f, height * 0.105f))
			: MathF.Min(52f, MathF.Min((width * 0.34f - margin) / 3f, (height - 48f - 2f * margin) / 3f));
		float actionSize = MathF.Min(48f, cell);
		float actionX = leftHanded ? margin + actionSize / 2f : width - margin - actionSize / 2f;

		if (compact)
		{
			float size = portrait
				? MathF.Min(132f, MathF.Min(width * 0.62f - margin, height * 0.315f))
				: cell * 3f;
			float x = leftHanded ? width - margin - size : margin;
			var bounds = new RectangleF(x, height - margin - size, size, size);
			float actionY = portrait ? height - margin - actionSize / 2f : bounds.Center.Y;
			return new TouchDPadLayout
			{
				North = bounds.Clone(), East = bounds.Clone(),
				South = bounds.Clone(), West = bounds.Clone(), CompactBounds = bounds,
				Action = Square(actionX, actionY, actionSize), ArrowSize = size * 0.9f
			};
		}

		float centerX = portrait ? width / 2f : margin + cell * 1.5f;
		if (!portrait && leftHanded) centerX = width - centerX;
		float centerY = height - margin - cell * 1.5f;
		// The action sits beside the SOUTH row in portrait, not on top of EAST/WEST.
		float skillY = portrait ? height - margin - actionSize / 2f : centerY;
		return new TouchDPadLayout
		{
			North = Square(centerX, centerY - cell, cell),
			East = Square(centerX + cell, centerY, cell),
			South = Square(centerX, centerY + cell, cell),
			West = Square(centerX - cell, centerY, cell),
			Action = Square(actionX, skillY, actionSize),
			ArrowSize = cell * 0.8f
		};
	}

	private static RectangleF Square(float centerX, float centerY, float size) =>
		new(centerX - size / 2f, centerY - size / 2f, size, size);
}
