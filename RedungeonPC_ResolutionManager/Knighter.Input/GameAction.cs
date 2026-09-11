namespace Knighter.Input;

/// <summary>
/// Ações semânticas do jogo. Telas e sistemas de gameplay consultam estas
/// ações em vez de conhecer teclas ou botões físicos diretamente.
/// </summary>
public enum GameAction
{
	MoveUp,
	MoveDown,
	MoveLeft,
	MoveRight,
	Confirm,
	Cancel,
	Pause,
	Ability,
	Secondary,
	NextCategory,
	PreviousCategory,
	Screenshot
}
