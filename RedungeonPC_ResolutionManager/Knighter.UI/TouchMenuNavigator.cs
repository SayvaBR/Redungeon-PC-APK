using System;
using System.Collections.Generic;
using Knighter.Helpers;
using Knighter.Input;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Adiciona foco de teclado/gamepad a um TouchMenu de tela. A classe é
/// instanciada somente por estados de menu; HUDs e controles touch de
/// gameplay continuam completamente separados desta navegação.
/// </summary>
public sealed class TouchMenuNavigator<T> where T : struct, IConvertible
{
	private readonly TouchMenu<T> menu;
	private readonly MenuInputRouter router;
	private readonly List<T> focusOrder;
	private T? focusedButton;
	private Point? previousMouse;

	public T? FocusedButton => focusedButton;

	public TouchMenuNavigator(TouchMenu<T> menu, InputManager input, params T[] preferredOrder)
	{
		this.menu = menu;
		router = new MenuInputRouter(input);
		focusOrder = new List<T>(preferredOrder ?? Array.Empty<T>());
	}

	public bool HandleInput(float dt)
	{
		router.Update(dt > 0f ? dt : 1f / 60f);
		List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> navigable = GetNavigableButtons();
		if (navigable.Count == 0)
		{
			focusedButton = null;
			menu.SetFocusedButton(null);
			return false;
		}

		if (!focusedButton.HasValue || !Contains(navigable, focusedButton.Value))
		{
			focusedButton = FindPreferred(navigable);
		}

		var mouse = Core.Instance.Input.Current.Mouse;
		if (InputDeviceTracker.UsingMouse && previousMouse != mouse.Position)
		{
			Vector2 p = Core.Instance.ResolutionManager.WindowToLogical(new Vector2(mouse.X, mouse.Y));
			foreach (var button in navigable) if (button.Value.Rectangle.Contains(p)) { focusedButton = button.Key; break; }
		}
		previousMouse = mouse.Position;
		MenuAction action = router.ConsumeDirectionalAction();
		if (action != MenuAction.None)
		{
			MoveFocus(navigable, action);
		}

		menu.SetFocusedButton(focusedButton);
		if (router.ConfirmPressed && focusedButton.HasValue)
		{
			return menu.ActivateButton(focusedButton.Value);
		}
		return action != MenuAction.None;
	}

	public void ResetFocus()
	{
		focusedButton = null;
		menu.SetFocusedButton(null);
	}

	private List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> GetNavigableButtons()
	{
		List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> result = new();
		float screenWidth = Core.Instance.Renderer.ScreenWidth;
		float screenHeight = Core.Instance.Renderer.ScreenHeight;
		foreach (KeyValuePair<T, TouchMenu<T>.ButtonDesc> button in menu.ButtonEntries)
		{
			RectangleF rect = button.Value.Rectangle;
			bool onScreen = rect.Width > 0f && rect.Height > 0f
				&& rect.Right > 0f && rect.Bottom > 0f
				&& rect.Left < screenWidth && rect.Top < screenHeight;
			if (!button.Value.Hidden && !button.Value.Disabled && onScreen)
			{
				result.Add(button);
			}
		}
		return result;
	}

	private T FindPreferred(List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> navigable)
	{
		foreach (T preferred in focusOrder)
		{
			if (Contains(navigable, preferred))
			{
				return preferred;
			}
		}

		navigable.Sort((left, right) =>
		{
			int y = left.Value.Rectangle.Top.CompareTo(right.Value.Rectangle.Top);
			return y != 0 ? y : left.Value.Rectangle.Left.CompareTo(right.Value.Rectangle.Left);
		});
		return navigable[0].Key;
	}

	private void MoveFocus(List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> navigable, MenuAction action)
	{
		if (!focusedButton.HasValue)
		{
			focusedButton = FindPreferred(navigable);
			return;
		}

		Vector2 desired = action switch
		{
			MenuAction.MoveUp => new Vector2(0f, -1f),
			MenuAction.MoveDown => new Vector2(0f, 1f),
			MenuAction.MoveLeft => new Vector2(-1f, 0f),
			MenuAction.MoveRight => new Vector2(1f, 0f),
			_ => Vector2.Zero
		};
		if (desired == Vector2.Zero)
		{
			return;
		}

		Vector2 currentCenter = Find(navigable, focusedButton.Value).Value.Rectangle.Center;
		T? best = null;
		float bestScore = float.MaxValue;
		foreach (KeyValuePair<T, TouchMenu<T>.ButtonDesc> candidate in navigable)
		{
			if (EqualityComparer<T>.Default.Equals(candidate.Key, focusedButton.Value))
			{
				continue;
			}
			Vector2 delta = candidate.Value.Rectangle.Center - currentCenter;
			float primary = Vector2.Dot(delta, desired);
			if (primary <= 0.5f)
			{
				continue;
			}
			float perpendicular = Math.Abs(delta.X * desired.Y - delta.Y * desired.X);
			float score = primary + perpendicular * 2f;
			if (score < bestScore)
			{
				bestScore = score;
				best = candidate.Key;
			}
		}

		if (!best.HasValue)
		{
			// Wrap previsível quando não há item adiante: escolhe o item mais
			// distante no sentido oposto, privilegiando a mesma linha/coluna.
			foreach (KeyValuePair<T, TouchMenu<T>.ButtonDesc> candidate in navigable)
			{
				if (EqualityComparer<T>.Default.Equals(candidate.Key, focusedButton.Value))
				{
					continue;
				}
				Vector2 delta = candidate.Value.Rectangle.Center - currentCenter;
				float primary = Vector2.Dot(delta, desired);
				float perpendicular = Math.Abs(delta.X * desired.Y - delta.Y * desired.X);
				float score = primary * 1000f + perpendicular;
				if (score < bestScore)
				{
					bestScore = score;
					best = candidate.Key;
				}
			}
		}

		if (best.HasValue)
		{
			focusedButton = best.Value;
		}
	}

	private static bool Contains(List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> buttons, T key)
	{
		return buttons.Exists(pair => EqualityComparer<T>.Default.Equals(pair.Key, key));
	}

	private static KeyValuePair<T, TouchMenu<T>.ButtonDesc> Find(
		List<KeyValuePair<T, TouchMenu<T>.ButtonDesc>> buttons,
		T key)
	{
		return buttons.Find(pair => EqualityComparer<T>.Default.Equals(pair.Key, key));
	}
}
