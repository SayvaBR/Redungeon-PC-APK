using System;
using System.Collections.Generic;
using Knighter;
using Knighter.Input;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.UI;

/// <summary>
/// Peça central que une uma lista de <see cref="IMenuWidget"/> ao teclado,
/// gamepad e mouse. Nenhuma tela precisa reimplementar navegação: ela só
/// entrega os widgets (via SetWidgets) e chama Update/Draw todo frame.
///
/// Cliques/toques continuam vindo de Core.TouchState (MouseTouchAdapter),
/// igual ao resto do jogo. A única leitura direta de Mouse.GetState() é
/// para saber onde o cursor está *sem* estar pressionado, já que
/// TouchCollection só existe enquanto o botão está apertado — é preciso
/// para destacar visualmente o item sob o cursor antes do clique.
/// </summary>
public class MenuFocusController : Component
{
	private readonly MenuInputRouter router;
	private List<IMenuWidget> widgets = new();
	private int focusIndex = -1;
	private int? pressedIndex;
	private Point? lastMousePosition;
	private readonly MenuCursor cursor = new();

	public event Action OnNextCategoryRequested;
	public event Action OnPreviousCategoryRequested;

	public IMenuWidget FocusedWidget => (focusIndex >= 0 && focusIndex < widgets.Count) ? widgets[focusIndex] : null;

	public MenuFocusController(InputManager input)
	{
		router = new MenuInputRouter(input);
	}

	/// <summary>Substitui a lista de widgets ativa (ex.: ao trocar de categoria) e foca o primeiro item navegável.</summary>
	public void SetWidgets(List<IMenuWidget> newWidgets)
	{
		widgets = newWidgets ?? new List<IMenuWidget>();
		pressedIndex = null;
		focusIndex = -1;
		MoveFocus(1);
	}

	public override void Update()
	{
		float dt = 1f / 60f;
		router.Update(dt);

		foreach (IMenuWidget widget in widgets)
		{
			widget.Update(dt);
		}

		HandleDirectionalInput();
		HandleConfirmInput();
		HandleCategoryInput();
		HandleMouse();
		cursor.Step(FocusedWidget?.Bounds);

		base.Update();
	}

	public override void Draw()
	{
		int baseDepth = 3000;
		for (int i = 0; i < widgets.Count; i++)
		{
			widgets[i].Draw(i == focusIndex, baseDepth + i * 20);
		}
		base.Draw();
		cursor.Render("fg", 9000);
	}

	private void HandleDirectionalInput()
	{
		MenuAction action = router.ConsumeDirectionalAction();
		if (action == MenuAction.None)
		{
			return;
		}

		IMenuWidget focused = FocusedWidget;
		if (focused is FactsBrowserWidget facts && (action == MenuAction.MoveDown || action == MenuAction.MoveUp) && facts.Scroll(action == MenuAction.MoveDown ? 1 : -1)) return;
		if (focused != null && focused.ConsumesHorizontalInput && (action == MenuAction.MoveLeft || action == MenuAction.MoveRight))
		{
			focused.Adjust(action == MenuAction.MoveLeft ? -1 : 1);
			return;
		}

		switch (action)
		{
		case MenuAction.MoveDown:
		case MenuAction.MoveRight:
			MoveFocus(1);
			break;
		case MenuAction.MoveUp:
		case MenuAction.MoveLeft:
			MoveFocus(-1);
			break;
		}
	}

	private void HandleConfirmInput()
	{
		if (router.ConfirmPressed)
		{
			FocusedWidget?.Activate();
		}
	}

	private void HandleCategoryInput()
	{
		if (router.NextCategoryPressed)
		{
			OnNextCategoryRequested?.Invoke();
		}
		if (router.PreviousCategoryPressed)
		{
			OnPreviousCategoryRequested?.Invoke();
		}
	}

	private void HandleMouse()
	{
		foreach (IMenuWidget widget in widgets)
			if (widget is MenuFooterButtonWidget footer) footer.PointerPressed = false;
		MouseState mouseState = core.Input.Current.Mouse;
		bool moved = !lastMousePosition.HasValue || lastMousePosition.Value != mouseState.Position;
		lastMousePosition = mouseState.Position;
		if (InputDeviceTracker.CurrentDevice == InputDevice.Gamepad)
		{
			pressedIndex = null;
			return;
		}
		Vector2 hoverPosition = core.ResolutionManager.WindowToLogical(new Vector2(mouseState.X, mouseState.Y));
		for (int i = 0; moved && i < widgets.Count; i++)
		{
			if (widgets[i].IsFocusable && widgets[i].Bounds?.Contains(hoverPosition) == true)
			{
				if (focusIndex != i)
				{
					focusIndex = i;
					PlayNavigationSound();
				}
				break;
			}
		}

		foreach (TouchLocation touch in core.TouchState)
		{
			if (touch.State == TouchLocationState.Pressed)
			{
				pressedIndex = FindWidgetAt(touch.Position);
			}
			else if (touch.State == TouchLocationState.Released)
			{
				int? releasedIndex = FindWidgetAt(touch.Position);
				if (pressedIndex.HasValue && releasedIndex == pressedIndex)
				{
					focusIndex = releasedIndex.Value;
					widgets[releasedIndex.Value].HandleClick(touch.Position);
				}
				pressedIndex = null;
			}
			if (pressedIndex.HasValue && pressedIndex.Value < widgets.Count &&
				widgets[pressedIndex.Value] is MenuFooterButtonWidget footer)
				footer.PointerPressed = footer.Bounds.Contains(touch.Position);
		}
	}

	private int? FindWidgetAt(Vector2 point)
	{
		for (int i = 0; i < widgets.Count; i++)
		{
			if (widgets[i].IsFocusable && widgets[i].Bounds.Contains(point) == true)
			{
				return i;
			}
		}
		return null;
	}

	private void MoveFocus(int direction)
	{
		if (widgets.Count == 0)
		{
			focusIndex = -1;
			return;
		}

		int startIndex = focusIndex;
		int next = focusIndex;
		for (int attempts = 0; attempts < widgets.Count; attempts++)
		{
			next = ((next + direction) % widgets.Count + widgets.Count) % widgets.Count;
			if (widgets[next].IsFocusable)
			{
				if (next != startIndex)
				{
					focusIndex = next;
					if (startIndex != -1)
					{
						PlayNavigationSound();
					}
				}
				return;
			}
		}
	}

	private void PlayNavigationSound()
	{
		SendMessage(new PlaySoundMessage(SoundName.button_up, 0.5f));
	}
}
