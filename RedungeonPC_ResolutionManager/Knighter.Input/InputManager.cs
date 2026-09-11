using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter.Input;

/// <summary>
/// Fonte única de verdade para teclado e gamepad. Captura cada dispositivo
/// uma vez por frame e expõe ações lógicas, bordas de pressionamento e
/// direções digitais para menus e gameplay.
/// </summary>
public sealed class InputManager
{
	private const float StickDeadZone = 0.5f;

	private readonly IInputSource source;
	private InputSnapshot current;
	private InputSnapshot previous;
	private bool hasState;

	public InputSnapshot Current => current;

	public bool GamepadConnected => current.GamePad.IsConnected;

	public InputManager(IInputSource source = null)
	{
		this.source = source ?? new MonoGameInputSource();
	}

	public void Update()
	{
		Update(source.Capture());
	}

	/// <summary>
	/// Sobrecarga determinística usada por testes e pelo futuro reprodutor de
	/// sessões. O executável normal chama <see cref="Update()"/>.
	/// </summary>
	public void Update(InputSnapshot snapshot)
	{
		if (!hasState)
		{
			current = snapshot;
			previous = snapshot;
			hasState = true;
		}
		else
		{
			previous = current;
			current = snapshot;
		}

		InputDeviceTracker.Update(current.Keyboard, current.Mouse, current.GamePad);
	}

	/// <summary>Descarta bordas e estados segurados após perda de foco.</summary>
	public void Reset()
	{
		hasState = false;
		current = default;
		previous = default;
		InputDeviceTracker.Reset();
	}

	public bool IsDown(GameAction action)
	{
		return Read(action, current);
	}

	public bool WasPressed(GameAction action)
	{
		return hasState && Read(action, current) && !Read(action, previous);
	}

	public bool WasReleased(GameAction action)
	{
		return hasState && !Read(action, current) && Read(action, previous);
	}

	public bool TryGetPressedDirection(out Vector2 direction)
	{
		if (WasPressed(GameAction.MoveUp))
		{
			direction = new Vector2(0f, -1f);
			return true;
		}
		if (WasPressed(GameAction.MoveRight))
		{
			direction = new Vector2(1f, 0f);
			return true;
		}
		if (WasPressed(GameAction.MoveDown))
		{
			direction = new Vector2(0f, 1f);
			return true;
		}
		if (WasPressed(GameAction.MoveLeft))
		{
			direction = new Vector2(-1f, 0f);
			return true;
		}

		direction = Vector2.Zero;
		return false;
	}

	public Vector2 GetHeldDirection()
	{
		if (IsDown(GameAction.MoveUp))
		{
			return new Vector2(0f, -1f);
		}
		if (IsDown(GameAction.MoveRight))
		{
			return new Vector2(1f, 0f);
		}
		if (IsDown(GameAction.MoveDown))
		{
			return new Vector2(0f, 1f);
		}
		if (IsDown(GameAction.MoveLeft))
		{
			return new Vector2(-1f, 0f);
		}
		return Vector2.Zero;
	}

	public bool IsDirectionDown(Vector2 direction)
	{
		if (direction.Y < 0f)
		{
			return IsDown(GameAction.MoveUp);
		}
		if (direction.Y > 0f)
		{
			return IsDown(GameAction.MoveDown);
		}
		if (direction.X < 0f)
		{
			return IsDown(GameAction.MoveLeft);
		}
		if (direction.X > 0f)
		{
			return IsDown(GameAction.MoveRight);
		}
		return false;
	}

	private static bool Read(GameAction action, InputSnapshot snapshot)
	{
		KeyboardState keyboard = snapshot.Keyboard;
		GamePadState gamePad = snapshot.GamePad;
		bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

		return action switch
		{
			GameAction.MoveUp => keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up)
				|| gamePad.IsButtonDown(Buttons.DPadUp) || gamePad.ThumbSticks.Left.Y > StickDeadZone,
			GameAction.MoveDown => keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)
				|| gamePad.IsButtonDown(Buttons.DPadDown) || gamePad.ThumbSticks.Left.Y < -StickDeadZone,
			GameAction.MoveLeft => keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)
				|| gamePad.IsButtonDown(Buttons.DPadLeft) || gamePad.ThumbSticks.Left.X < -StickDeadZone,
			GameAction.MoveRight => keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)
				|| gamePad.IsButtonDown(Buttons.DPadRight) || gamePad.ThumbSticks.Left.X > StickDeadZone,
			GameAction.Confirm => keyboard.IsKeyDown(Keys.Enter) || keyboard.IsKeyDown(Keys.Space)
				|| gamePad.IsButtonDown(Buttons.A),
			GameAction.Cancel => keyboard.IsKeyDown(Keys.Escape) || gamePad.IsButtonDown(Buttons.B),
			GameAction.Pause => keyboard.IsKeyDown(Keys.P) || gamePad.IsButtonDown(Buttons.Start),
			GameAction.Ability => keyboard.IsKeyDown(Keys.Space) || keyboard.IsKeyDown(Keys.E)
				|| gamePad.IsButtonDown(Buttons.A),
			GameAction.Secondary => keyboard.IsKeyDown(Keys.Q) || gamePad.IsButtonDown(Buttons.Y),
			GameAction.Screenshot => keyboard.IsKeyDown(Keys.C) || gamePad.IsButtonDown(Buttons.X),
			GameAction.NextCategory => (keyboard.IsKeyDown(Keys.Tab) && !shift)
				|| gamePad.IsButtonDown(Buttons.RightShoulder),
			GameAction.PreviousCategory => (keyboard.IsKeyDown(Keys.Tab) && shift)
				|| gamePad.IsButtonDown(Buttons.LeftShoulder),
			_ => false
		};
	}
}
