using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

public readonly struct ContextPrompt
{
	public PromptAction Action { get; }
	public string Label { get; }

	public ContextPrompt(PromptAction action, string label)
	{
		Action = action;
		Label = label ?? string.Empty;
	}
}

/// <summary>
/// Legenda única para as ações disponíveis na tela atual. Em gamepad usa a
/// folha da marca detectada; em teclado/mouse mostra a tecla equivalente.
/// </summary>
public static class ContextPromptLegend
{
	private const float IconScale = 0.25f;
	private const float RowHeight = 19f;
	private const int Depth = 18000;

	public static void DrawButton(Renderer renderer, Vector2 origin, PromptAction action)
	{
#if ANDROID
		if (InputDeviceTracker.CurrentDevice != InputDevice.Gamepad) return;
#endif
		if (action == PromptAction.PreviousCategory && InputDeviceTracker.CurrentDevice != InputDevice.Gamepad)
		{
			var shift = InputPromptIcons.Get(action);
			renderer["fg", Depth, false].DrawSpriteS(shift, origin, Color.White, Vector2.One * (16f / Math.Max(shift.Width, shift.Height)));
			renderer["fg", Depth + 1, false].DrawSimpleTextS("+", origin + new Vector2(15, 6), Color.Wheat, .6f);
			DrawButton(renderer, origin + new Vector2(20, 0), PromptAction.NextCategory);
			return;
		}
		Sprite icon = InputPromptIcons.Get(action);
		if (icon != null)
		{
			renderer["fg", Depth, false].DrawSpriteS(icon, origin + new Vector2(8f, 8f), Color.White, Vector2.One * (16f / Math.Max(icon.Width, icon.Height)), 0f, SpriteFlip.None, SpriteOrigin.Center);
			return;
		}
		string key = KeyboardLabel(action);
		float width = Math.Max(18f, key.Length * 3f + 8f);
		renderer["fg", Depth, false].DrawRectangleS(new RectangleF(origin.X, origin.Y, width, 16f), Color.White);
		renderer["fg", Depth + 1, false].DrawSimpleTextS(key, origin + new Vector2((width - key.Length * 3f) / 2f, 6f), new Color(40, 32, 32), 0.5f);
	}

	public static void Draw(Renderer renderer, Vector2 origin, params ContextPrompt[] prompts)
	{
		DrawCore(renderer, origin, vertical: false, prompts);
	}

	public static void DrawVertical(Renderer renderer, Vector2 origin, params ContextPrompt[] prompts)
	{
		DrawCore(renderer, origin, vertical: true, prompts);
	}

	private static void DrawCore(Renderer renderer, Vector2 origin, bool vertical, params ContextPrompt[] prompts)
	{
#if ANDROID
		if (InputDeviceTracker.CurrentDevice != InputDevice.Gamepad) return;
#endif
		if (renderer == null || prompts == null || prompts.Length == 0)
		{
			return;
		}

		float widest = 0f;
		float total = 0f;
		foreach (ContextPrompt p in prompts)
		{
			float width = Math.Max(18f, KeyboardLabel(p.Action).Length * 3f + 8f) + 8f + p.Label.Length * 3.8f;
			widest = Math.Max(widest, width);
			total += width + 12f;
		}
		if (!vertical && total > renderer.ScreenWidth - 16f) vertical = true;
		float x = Math.Max(8f, Math.Min(origin.X, renderer.ScreenWidth - 8f - (vertical ? widest : total)));
		float y = Math.Max(8f, Math.Min(origin.Y, renderer.ScreenHeight - 8f - (vertical ? prompts.Length * RowHeight : RowHeight)));
		foreach (ContextPrompt prompt in prompts)
		{
			float keyWidth = InputPromptIcons.Get(prompt.Action) != null ? 18f : Math.Max(18f, KeyboardLabel(prompt.Action).Length * 3f + 8f);
			DrawButton(renderer, new Vector2(x, y), prompt.Action);

			float labelX = x + keyWidth + 4f;
			renderer["fg", Depth + 2, false].DrawTextS(prompt.Label.ToUpperInvariant(), new Vector2(labelX, y + 4f), new TextProfile
			{
				Font = Font.Bold, Scale = 0.55f, Color = Color.White,
				Width = Math.Max(1, (int)(renderer.ScreenWidth - labelX - 8f)), Height = 14,
				BoxAlignment = Alignment2D.Left, TextAlignment = Alignment2D.Left,
				Decoration = TextDecoration.None
			});
			if (vertical)
			{
				y += RowHeight;
			}
			else
			{
				x = labelX + Math.Max(42f, prompt.Label.Length * 3.8f) + 12f;
			}
		}
	}

	private static string KeyboardLabel(PromptAction action)
	{
		return action switch
		{
			PromptAction.Confirm => "ENTER",
			PromptAction.Cancel => "ESC",
			PromptAction.Secondary => "Q",
			PromptAction.Pause => "P",
			PromptAction.PreviousCategory => "SHIFT+TAB",
			PromptAction.NextCategory => "TAB",
			PromptAction.Screenshot => "C",
			_ => ""
		};
	}
}
