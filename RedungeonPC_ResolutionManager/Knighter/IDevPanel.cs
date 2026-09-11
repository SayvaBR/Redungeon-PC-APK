#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Knighter.Graphics;

namespace Knighter;

/// <summary>
/// Um painel de debug independente (ex.: Performance, Sprite Inspector).
/// Cada painel só decide O QUE mostrar (linhas de texto); COMO fica visual —
/// a "janela" com título, fundo e cor — é sempre desenhado do mesmo jeito
/// pelo EngineDiagnostics, então todos os painéis ficam consistentes.
/// </summary>
public interface IDevPanel
{
	/// <summary>Nome mostrado na barra de título da janela.</summary>
	string Title { get; }

	/// <summary>Cor da barra de título — só pra diferenciar painéis visualmente.</summary>
	Color TitleColor { get; }

	/// <summary>Chamado todo frame. Painéis com tecla de atalho própria (ex.: Ctrl+I) devem ler o teclado aqui.</summary>
	void Update(GameTime gameTime);

	/// <summary>
	/// Preenche "lines" com o conteúdo deste frame. Lista vazia = a janela
	/// inteira (título incluso) não é desenhada — útil pra painéis que só
	/// aparecem quando têm algo relevante a mostrar (ex.: Sprite Inspector).
	/// </summary>
	void CollectLines(Renderer renderer, List<string> lines);
}
#endif
