using Microsoft.Xna.Framework;
using Knighter.Input;

namespace Knighter.UI;

/// <summary>
/// Traduz teclado e gamepad em <see cref="MenuAction"/>. É a única peça do
/// sistema que conhece Keys/Buttons — todo o resto do menu (widgets, painel,
/// navegador de foco) só fala a linguagem de MenuAction.
///
/// O mouse não passa por aqui: ele continua sendo lido via Core.TouchState
/// (MouseTouchAdapter), exatamente como o resto do jogo já faz, e cada
/// widget faz seu próprio hit-test — ver <see cref="IMenuWidget"/>.
///
/// Suporta "repeat": segurar uma direção move o foco de novo depois de um
/// atraso inicial, e continua repetindo num intervalo menor, como em
/// qualquer menu de console/PC.
/// </summary>
public class MenuInputRouter
{
	private const float InitialRepeatDelay = 0.35f;
	private const float RepeatInterval = 0.12f;

	private readonly InputManager input;

	private MenuAction heldDirection = MenuAction.None;
	private float heldTimer;
	private bool firstRepeatDone;

	public MenuInputRouter(InputManager input)
	{
		this.input = input;
	}

	/// <summary>Deve ser chamado uma vez por frame, antes de consultar as ações.</summary>
	public void Update(float dt)
	{
		MenuAction directionNow = ReadDirection();

		if (directionNow != MenuAction.None && directionNow == heldDirection)
		{
			heldTimer += dt;
		}
		else
		{
			heldDirection = directionNow;
			heldTimer = 0f;
			firstRepeatDone = false;
		}
	}

	/// <summary>
	/// Ação de navegação (mover foco) deste frame: o toque inicial da
	/// tecla/botão, ou um "tick" de auto-repeat se a direção continua
	/// segurada. Retorna MenuAction.None na maioria dos frames.
	/// </summary>
	public MenuAction ConsumeDirectionalAction()
	{
		if (heldDirection == MenuAction.None)
		{
			return MenuAction.None;
		}

		if (heldTimer == 0f)
		{
			return heldDirection;
		}

		float threshold = firstRepeatDone ? RepeatInterval : InitialRepeatDelay;
		if (heldTimer < threshold)
		{
			return MenuAction.None;
		}

		firstRepeatDone = true;
		heldTimer = 0f;
		return heldDirection;
	}

	public bool ConfirmPressed => input.WasPressed(GameAction.Confirm);

	public bool NextCategoryPressed => input.WasPressed(GameAction.NextCategory);

	public bool PreviousCategoryPressed => input.WasPressed(GameAction.PreviousCategory);

	private MenuAction ReadDirection()
	{
		Vector2 direction = input.GetHeldDirection();
		if (direction.Y < 0f)
		{
			return MenuAction.MoveUp;
		}
		if (direction.Y > 0f)
		{
			return MenuAction.MoveDown;
		}
		if (direction.X < 0f)
		{
			return MenuAction.MoveLeft;
		}
		if (direction.X > 0f)
		{
			return MenuAction.MoveRight;
		}
		return MenuAction.None;
	}
}
