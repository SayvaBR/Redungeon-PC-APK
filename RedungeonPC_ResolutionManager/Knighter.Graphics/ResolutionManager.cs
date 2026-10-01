using System;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

/// <summary>
/// Única fonte de verdade para toda a matemática de resolução do jogo.
///
/// Fluxo: tamanho do backbuffer (pixels reais da janela/monitor)
///     -> escala de pixel art inteira (PixelScale)
///     -> resolução lógica ("pixels de jogo": LogicalWidth/LogicalHeight),
///        usada por Camera, Renderer e HUD para tudo que é ToScreen/ToWorld
///     -> DestRect: onde essa imagem lógica escalada é desenhada dentro
///        do backbuffer (normalmente ele preenche tudo; só aparece
///        letterbox/pillarbox em janelas com proporção anormal).
///
/// Ninguém além desta classe deve calcular PixelScale, ScreenWidth/Height
/// ou qualquer transformação de tela/viewport. Renderer apenas expõe (passa
/// adiante) os valores já resolvidos aqui, e Camera lê do Renderer — por
/// isso os três "usam a mesma matemática" por construção, não por acordo.
///
/// A escala é sempre um número inteiro (nunca fracionário): é isso que
/// garante pixel art perfeita em qualquer resolução, sem os sprites
/// "aumentarem de tamanho" (em proporção) nem borrarem/tremerem (shimmering)
/// por causa de amostragem em escalas não inteiras.
/// </summary>
public sealed class ResolutionManager
{
	/// <summary>
	/// Altura lógica de referência herdada do heurístico mobile original
	/// (a mesma constante "266" usada no cálculo antigo de PixelScale),
	/// para preservar o mesmo "peso" visual de HUD e sprites do design
	/// original. Dividida por GuiScale, exatamente como antes.
	/// </summary>
	private const float ReferenceLogicalHeight = 266f;

	/// <summary>
	/// Largura lógica mínima que o HUD ainda suporta sem espremer o layout.
	/// Abaixo disso, em vez de continuar encolhendo a escala, passamos a
	/// recalcular pela largura e fazer letterbox (barras) no topo/base.
	/// Isso só deve acontecer em janelas anormalmente estreitas.
	/// </summary>
	private const float MinLogicalWidth = 300f;

	public int BackbufferWidth { get; private set; } = 1;

	public int BackbufferHeight { get; private set; } = 1;

	/// <summary>Fator de escala inteiro. Nunca fracionário — ver comentário da classe.</summary>
	public int PixelScale { get; private set; } = 1;

	/// <summary>Resolução lógica ("pixels de jogo"). É isto que Camera/HUD enxergam como ScreenWidth/ScreenHeight.</summary>
	public float LogicalWidth { get; private set; }

	/// <summary>Resolução lógica ("pixels de jogo"). É isto que Camera/HUD enxergam como ScreenWidth/ScreenHeight.</summary>
	public float LogicalHeight { get; private set; }

	/// <summary>Retângulo, em pixels de backbuffer, onde a imagem lógica escalada é desenhada.</summary>
	public Rectangle DestRect { get; private set; }

	/// <summary>Matriz pronta (escala inteira + deslocamento do DestRect) para o SpriteBatch.</summary>
	public Matrix ScaleMatrix { get; private set; } = Matrix.Identity;

	public Vector2 ScreenCenter => new Vector2(LogicalWidth * 0.5f, LogicalHeight * 0.5f);

	/// <summary>Disparado sempre que a resolução é recalculada (resize, fullscreen, F11/Alt+Enter, mudança de opção).</summary>
	public event Action Changed;

	public void Recalculate(int backbufferWidth, int backbufferHeight)
	{
		BackbufferWidth = Math.Max(1, backbufferWidth);
		BackbufferHeight = Math.Max(1, backbufferHeight);

		float mobileZoom = Settings.IsTouchDevice ? 1.6f : 1.0f;
    	float targetLogicalHeight = (ReferenceLogicalHeight / Settings.GuiScale) / mobileZoom;

		int scale = Math.Max(1, (int)((float)BackbufferHeight / targetLogicalHeight));
		float logicalWidth = BackbufferWidth / (float)scale;

		float minimumWidth = Settings.IsTouchDevice && BackbufferHeight > BackbufferWidth ? 240f : MinLogicalWidth;
		if (logicalWidth < minimumWidth)
		{
			// Janela anormalmente estreita: recalcula a escala pela largura
			// e deixa sobrar barras no topo/base em vez de espremer o HUD.
			scale = Math.Max(1, (int)((float)BackbufferWidth / minimumWidth));
			logicalWidth = BackbufferWidth / (float)scale;
		}

		PixelScale = scale;
		LogicalWidth = logicalWidth;
		LogicalHeight = BackbufferHeight / (float)scale;

		int destWidth = (int)(LogicalWidth * scale);
		int destHeight = (int)(LogicalHeight * scale);
		int destX = (BackbufferWidth - destWidth) / 2;
		int destY = (BackbufferHeight - destHeight) / 2;
		DestRect = new Rectangle(destX, destY, destWidth, destHeight);

		ScaleMatrix = Matrix.CreateScale(PixelScale, PixelScale, 1f) * Matrix.CreateTranslation(DestRect.X, DestRect.Y, 0f);

		Changed?.Invoke();
	}

	/// <summary>Converte um ponto em pixels de janela/mouse (espaço do backbuffer) para coordenadas lógicas do jogo.</summary>
	public Vector2 WindowToLogical(Vector2 windowPosition)
	{
		return new Vector2(
			(windowPosition.X - DestRect.X) / PixelScale,
			(windowPosition.Y - DestRect.Y) / PixelScale);
	}
}
