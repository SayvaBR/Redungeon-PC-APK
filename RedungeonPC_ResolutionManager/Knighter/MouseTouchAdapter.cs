using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;

/// <summary>
/// Converte o estado do Mouse em uma TouchCollection compatível com o resto do jogo,
/// sem depender da simulação interna TouchPanel.EnableMouseTouchPoint do MonoGame
/// (que se mostrou pouco confiável no DesktopGL). Preserva 100% da arquitetura original:
/// todo o jogo continua lendo Core.TouchState normalmente, sem saber a diferença.
/// </summary>
public static class MouseTouchAdapter
{
	private const int MouseTouchId = 1;

	private static ButtonState lastLeftButton = ButtonState.Released;

	private static Microsoft.Xna.Framework.Vector2 lastLogicalPosition;

	private static bool hasLastLogicalPosition;

	/// <summary>
	/// Retorna a TouchCollection do frame atual, convertendo a posição do mouse
	/// (pixels de janela) para coordenadas lógicas do jogo via
	/// ResolutionManager.WindowToLogical — a mesma fonte usada para configurar
	/// TouchPanel.DisplayWidth/Height, já considerando eventuais barras de
	/// letterbox (DestRect) em janelas com proporção anormal.
	/// </summary>
	public static TouchCollection GetState()
	{
		MouseState mouse = Mouse.GetState();
		var locations = new List<TouchLocation>(1);
		var game = Core.Instance.Game;
		var bounds = game.Window.ClientBounds;
		var destination = Core.Instance.ResolutionManager.DestRect;
		bool pointerIsUsable = game.IsActive
			&& mouse.X >= 0 && mouse.Y >= 0
			&& mouse.X < bounds.Width && mouse.Y < bounds.Height
			&& destination.Contains(mouse.X, mouse.Y);

		if (!pointerIsUsable)
		{
			// Cancela um gesto que saiu da janela/letterbox. Sem isso, o release
			// podia reaparecer sobre outro botão quando o cursor voltava ao jogo.
			if (lastLeftButton == ButtonState.Pressed && hasLastLogicalPosition)
			{
				locations.Add(new TouchLocation(MouseTouchId, TouchLocationState.Released, lastLogicalPosition));
			}
			lastLeftButton = ButtonState.Released;
			hasLastLogicalPosition = false;
			return new TouchCollection(locations.ToArray());
		}

		var logicalPos = Core.Instance.ResolutionManager.WindowToLogical(
			new Microsoft.Xna.Framework.Vector2(mouse.X, mouse.Y));
		lastLogicalPosition = logicalPos;
		hasLastLogicalPosition = true;

		if (mouse.LeftButton == ButtonState.Pressed)
		{
			TouchLocationState state = (lastLeftButton == ButtonState.Released)
				? TouchLocationState.Pressed
				: TouchLocationState.Moved;
			locations.Add(new TouchLocation(MouseTouchId, state, logicalPos));
		}
		else if (lastLeftButton == ButtonState.Pressed)
		{
			// Botão acabou de soltar: emite Released uma única vez neste frame.
			locations.Add(new TouchLocation(MouseTouchId, TouchLocationState.Released, logicalPos));
		}

		lastLeftButton = mouse.LeftButton;

		return new TouchCollection(locations.ToArray());
	}

	/// <summary>Descarta qualquer gesto pendente quando a janela perde o foco.</summary>
	public static void Reset()
	{
		lastLeftButton = ButtonState.Released;
		hasLastLogicalPosition = false;
	}
}
