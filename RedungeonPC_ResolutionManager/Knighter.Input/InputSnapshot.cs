using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter.Input;

/// <summary>
/// Leitura imutável dos dispositivos em um único frame. A separação entre
/// captura e interpretação permite futuramente gravar/reproduzir partidas de
/// QA sem alterar o código que consome input.
/// </summary>
public readonly struct InputSnapshot
{
	public KeyboardState Keyboard { get; }

	public MouseState Mouse { get; }

	public GamePadState GamePad { get; }

	public InputSnapshot(KeyboardState keyboard, MouseState mouse, GamePadState gamePad)
	{
		Keyboard = keyboard;
		Mouse = mouse;
		GamePad = gamePad;
	}
}

public interface IInputSource
{
	InputSnapshot Capture();
}

/// <summary>Fonte de produção que lê teclado, mouse e o primeiro gamepad.</summary>
public sealed class MonoGameInputSource : IInputSource
{
	public InputSnapshot Capture()
	{
		return new InputSnapshot(
			Keyboard.GetState(),
			Mouse.GetState(),
			GamePad.GetState(PlayerIndex.One));
	}
}
