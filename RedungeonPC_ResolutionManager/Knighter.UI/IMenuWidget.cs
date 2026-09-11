using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>
/// Contrato comum a todo elemento de um menu de configurações: botão,
/// toggle, seletor de valor, slider ou separador. O <see cref="MenuFocusController"/>
/// só conhece esta interface — nunca os tipos concretos — então adicionar um
/// widget novo no futuro não exige tocar no controlador.
/// </summary>
public interface IMenuWidget
{
	/// <summary>Área ocupada pelo widget, em coordenadas de tela. Definida pelo layout do painel.</summary>
	RectangleF Bounds { get; set; }

	/// <summary>Falso para separadores/títulos de seção, que existem só como espaçamento visual.</summary>
	bool IsFocusable { get; }

	/// <summary>
	/// Se verdadeiro, MoveLeft/MoveRight são entregues a este widget via
	/// <see cref="Adjust"/> em vez de mover o foco para o widget vizinho.
	/// Usado por seletores de valor e sliders.
	/// </summary>
	bool ConsumesHorizontalInput { get; }

	void Update(float dt);

	/// <summary>
	/// baseDepth é o início de uma faixa reservada só pra este widget (uso
	/// interno: cada elemento desenhado — fundo, ícone, texto, controle —
	/// deve usar um valor diferente dentro dessa faixa, nunca repetir o
	/// mesmo baseDepth em duas chamadas de desenho, ou a ordem entre elas
	/// deixa de ser garantida).
	/// </summary>
	void Draw(bool isFocused, int baseDepth);

	/// <summary>Chamado ao confirmar (Enter/A/clique) com o widget em foco.</summary>
	void Activate();

	/// <summary>Chamado com direction = -1 (esquerda) ou +1 (direita) quando ConsumesHorizontalInput é verdadeiro.</summary>
	void Adjust(int direction);

	/// <summary>
	/// Chamado pelo mouse quando um clique solto acontece dentro de Bounds.
	/// point está em coordenadas de tela. Cada widget decide se isso conta
	/// como Activate() ou como Adjust(-1)/Adjust(1), conforme sua área de
	/// controle desenhada.
	/// </summary>
	void HandleClick(Vector2 point);
}
