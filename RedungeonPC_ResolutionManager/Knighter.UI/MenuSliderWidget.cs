using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Linha de opção com valor contínuo (ex.: Intensidade da Vibração,
/// Sensibilidade do Mouse). Trabalha em 0f..1f internamente; quem constrói
/// o widget decide o texto de exibição via formatValue.
/// </summary>
public class MenuSliderWidget : MenuWidgetBase
{
	private static readonly Color TrackShadow = default(Color).FromRgb(1908778);
	private static readonly Color FillColor = TextProfile.OrangeMiddle;

	private readonly Func<float> getValue;
	private readonly Action<float> setValue;
	private readonly float step;
	private readonly Func<float, string> formatValue;

	public MenuSliderWidget(SId titleId, SId descriptionId, SpriteName iconSprite, Func<float> getValue, Action<float> setValue, Func<float, string> formatValue, float step = 0.1f)
		: base(titleId, descriptionId, iconSprite)
	{
		this.getValue = getValue;
		this.setValue = setValue;
		this.formatValue = formatValue;
		this.step = step;
	}

	public override bool ConsumesHorizontalInput => true;

	protected override float ControlAreaWidth => 72f;

	protected override void DrawControl(RectangleF controlArea, bool isFocused, int depth)
	{
		float value = Math.Clamp(getValue(), 0f, 1f);

		TextProfile valueProfile = TextProfile.OrangeBoldText.Alter(
			boxAlignment: Alignment2D.RightMiddle,
			textAlignment: Alignment2D.RightMiddle,
			width: 20,
			height: (int)controlArea.Height,
			scale: 0.42f);
		R["fg", depth, false].DrawTextS(formatValue(value), new Vector2(controlArea.Right - 20f, controlArea.Center.Y), valueProfile);

		RectangleF track = new RectangleF(controlArea.Left, controlArea.Center.Y - 3f, controlArea.Width - 24f, 6f);
		Renderer trackRenderer = R["fg", depth + 1, false];
		trackRenderer.DrawRectangleS(track, TrackShadow);
		if (value > 0.02f)
		{
			trackRenderer.DrawRectangleS(new RectangleF(track.X, track.Y, track.Width * value, track.Height), FillColor);
		}
		trackRenderer.DrawRectangleS(new RectangleF(track.X + track.Width * value - 1f, track.Y - 1f, 2f, track.Height + 2f), Color.White);
	}

	public override void Adjust(int direction)
	{
		float value = Math.Clamp(getValue() + direction * step, 0f, 1f);
		setValue(value);
		PlayValueChangeSound();
	}
}
