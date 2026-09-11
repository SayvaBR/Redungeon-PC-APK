using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter;

public enum InputDevice
{
	KeyboardMouse,
	Gamepad
}

/// <summary>
/// Marca de fabricante do gamepad, usada só pra escolher qual conjunto de
/// ícones de prompt mostrar (ex.: Ⓐ Xbox vs ✕ PlayStation). O MonoGame não
/// expõe de forma confiável o fabricante real do gamepad em todas as
/// plataformas — por isso <see cref="InputDeviceTracker.DetectBrand"/> é um
/// placeholder (assume Xbox) até termos uma forma melhor de detectar isso.
/// É por isso que o DevTools tem um atalho pra forçar a marca manualmente
/// durante testes.
/// </summary>
public enum GamepadBrand
{
	Xbox,
	PlayStation
}

/// <summary>
/// Rastreia qual foi o último dispositivo de entrada usado (teclado/mouse
/// ou gamepad), pra decidir que tipo de prompt de UI mostrar. Chamado uma
/// vez por frame via <see cref="Update"/> a partir de Core.Update().
/// </summary>
public static class InputDeviceTracker
{
	public static InputDevice CurrentDevice { get; private set; } = InputDevice.KeyboardMouse;
	public static bool UsingMouse { get; private set; } = true;

	public static GamepadBrand CurrentGamepadBrand { get; private set; } = GamepadBrand.Xbox;

	private static GamepadBrand? forcedBrand;

	private static KeyboardState previousKeyboard;
	private static MouseState previousMouse;
	private static GamePadState previousGamePad;
	private static bool hasPrevious;

	public static void Update(KeyboardState keyboard, MouseState mouse, GamePadState gamePad)
	{
		if (!hasPrevious)
		{
			// Se o controle já estava conectado ao abrir o jogo, a primeira
			// legenda deve usar A/B imediatamente — não só depois do primeiro
			// movimento do analógico ou pressionamento de botão.
			CurrentDevice = gamePad.IsConnected ? InputDevice.Gamepad : InputDevice.KeyboardMouse;
			UsingMouse = !gamePad.IsConnected;
		}
		else
		{
			bool keyboardActivity = AnyKeyChanged(keyboard, previousKeyboard);
			bool mouseActivity = mouse.X != previousMouse.X || mouse.Y != previousMouse.Y
				|| mouse.LeftButton != previousMouse.LeftButton || mouse.RightButton != previousMouse.RightButton;
			bool gamepadActivity = AnyButtonChanged(gamePad, previousGamePad)
				|| gamePad.ThumbSticks.Left.LengthSquared() > 0.1f
				|| gamePad.ThumbSticks.Right.LengthSquared() > 0.1f;

			if (keyboardActivity || mouseActivity)
			{
				UsingMouse = mouseActivity && !keyboardActivity;
				CurrentDevice = InputDevice.KeyboardMouse;
			}
			else if (gamepadActivity)
			{
				UsingMouse = false;
				CurrentDevice = InputDevice.Gamepad;
			}
			else if (!gamePad.IsConnected && CurrentDevice == InputDevice.Gamepad)
			{
				CurrentDevice = InputDevice.KeyboardMouse;
			}
		}

		CurrentGamepadBrand = forcedBrand
			?? (Core.Instance?.OptionsData != null
				? (Core.Instance.OptionsData.PromptStyle == 1 ? GamepadBrand.PlayStation : GamepadBrand.Xbox)
				: DetectBrand(gamePad));

		previousKeyboard = keyboard;
		previousMouse = mouse;
		previousGamePad = gamePad;
		hasPrevious = true;
	}

	public static void Reset()
	{
		hasPrevious = false;
		previousKeyboard = default;
		previousMouse = default;
		previousGamePad = default;
	}

	/// <summary>Usado pelo DevTools pra testar os prompts de cada marca sem precisar do gamepad físico.</summary>
	public static void ForceBrand(GamepadBrand brand)
	{
		forcedBrand = brand;
	}

	private static GamepadBrand DetectBrand(GamePadState state)
	{
		// Placeholder: MonoGame/SDL não expõe o fabricante de forma
		// confiável e multiplataforma aqui. Assume Xbox até existir uma
		// forma melhor de detectar (ex.: mapa de VID/PID).
		return GamepadBrand.Xbox;
	}

	private static bool AnyKeyChanged(KeyboardState current, KeyboardState previous)
	{
		Keys[] currentKeys = current.GetPressedKeys();
		if (currentKeys.Length > 0)
		{
			foreach (Keys key in currentKeys)
			{
				if (!previous.IsKeyDown(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool AnyButtonChanged(GamePadState current, GamePadState previous)
	{
		return current.Buttons.A != previous.Buttons.A
			|| current.Buttons.LeftShoulder != previous.Buttons.LeftShoulder
			|| current.Buttons.RightShoulder != previous.Buttons.RightShoulder
			|| current.Buttons.B != previous.Buttons.B
			|| current.Buttons.X != previous.Buttons.X
			|| current.Buttons.Y != previous.Buttons.Y
			|| current.DPad.Up != previous.DPad.Up
			|| current.DPad.Down != previous.DPad.Down
			|| current.DPad.Left != previous.DPad.Left
			|| current.DPad.Right != previous.DPad.Right
			|| current.Buttons.Start != previous.Buttons.Start
			|| current.Buttons.Back != previous.Buttons.Back;
	}
}
