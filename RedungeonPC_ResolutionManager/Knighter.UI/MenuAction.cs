namespace Knighter.UI;

/// <summary>
/// Ações lógicas de navegação de menu, desacopladas do dispositivo de entrada.
/// Teclado, gamepad e mouse convergem todos para estas ações através do
/// <see cref="MenuInputRouter"/>, então nenhum widget precisa saber se o
/// jogador está usando teclas, botões de gamepad ou o mouse.
/// </summary>
public enum MenuAction
{
	None,
	MoveUp,
	MoveDown,
	MoveLeft,
	MoveRight,
	Confirm,
	Cancel,
	NextCategory,
	PreviousCategory
}
