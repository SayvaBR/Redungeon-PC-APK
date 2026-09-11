#if DEBUG
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Knighter.Graphics;

namespace Knighter;

/// <summary>
/// Coordenador dos painéis de debug (ver IDevPanel). Cada painel só decide
/// o que mostrar; aqui elas viram texto (título colorido + linhas em branco),
/// empilhadas organizadamente — sem nenhuma caixa/fundo, só texto simples.
/// Toggle do overlay: F1. O Sprite Inspector tem tecla própria (Ctrl+I) e
/// fica sempre no canto superior direito, independente do F1.
/// Remoção: apagar este arquivo + os *Panel.cs + IDevPanel.cs, e as chamadas
/// marcadas "EngineDiagnostics" em Core.cs.
/// </summary>
public static class EngineDiagnostics
{
	private const float LineHeight = 10f;

	private const float WindowGap = 6f;

	private const float TextScale = 0.65f;

	public static bool Enabled { get; private set; }

	private static readonly SpriteInspectorPanel spriteInspector = new SpriteInspectorPanel();

	// Janelas empilhadas à esquerda, só visíveis com o overlay (F1) ligado.
	private static readonly List<IDevPanel> textPanels = new List<IDevPanel>
	{
		new PerformancePanel(),
		new StatePanel(),
		new InputPanel(),
		new BootLogPanel()
	};

	private static readonly List<string> lineBuffer = new List<string>();

	public static void Toggle()
	{
		Enabled = !Enabled;
	}

	public static void Log(string message)
	{
		BootLogPanel.Log(message);
	}

	public static void Update(GameTime gameTime)
	{
		spriteInspector.Update(gameTime);
		if (!Enabled)
		{
			return;
		}
		foreach (IDevPanel panel in textPanels)
		{
			panel.Update(gameTime);
		}
	}

	public static void Draw(Core core)
	{
		const int layer = 20000;
		Renderer renderer = core.Renderer;

		// Sprite Inspector: roda com tecla própria, independente do F1.
		const float inspectorWidth = 240f;
		float inspectorX = renderer.ScreenWidth - inspectorWidth - 8f;
		float afterInspector = DrawWindow(renderer, layer, spriteInspector, inspectorX, 8f, inspectorWidth);
		spriteInspector.DrawPreview(renderer, layer, inspectorX, afterInspector + WindowGap);

		if (!Enabled)
		{
			return;
		}

		const float x = 8f;
		const float width = 220f;
		float y = 8f;
		foreach (IDevPanel panel in textPanels)
		{
			float bottom = DrawWindow(renderer, layer, panel, x, y, width);
			if (bottom > y)
			{
				y = bottom + WindowGap;
			}
		}
	}

	/// <summary>
	/// Desenha um painel como um título colorido seguido das linhas, sem
	/// nenhuma caixa/retângulo de fundo (só texto — é o que sempre funcionou
	/// de forma confiável). Não desenha nada (e devolve "y" sem alterar) se
	/// o painel não tiver conteúdo nesse frame.
	/// </summary>
	private static float DrawWindow(Renderer renderer, int layer, IDevPanel panel, float x, float y, float width)
	{
		lineBuffer.Clear();
		panel.CollectLines(renderer, lineBuffer);
		if (lineBuffer.Count == 0)
		{
			return y;
		}

		Renderer r = renderer["fg", layer, false];
		r.DrawSimpleTextS($"[ {panel.Title} ]", new Vector2(x, y), panel.TitleColor, TextScale);
		float lineY = y + LineHeight;
		foreach (string line in lineBuffer)
		{
			r.DrawSimpleTextS(line, new Vector2(x, lineY), Color.White, TextScale);
			lineY += LineHeight;
		}

		return lineY;
	}
}
#endif
