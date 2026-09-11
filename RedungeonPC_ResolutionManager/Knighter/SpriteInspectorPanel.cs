#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Knighter.Graphics;

namespace Knighter;

/// <summary>
/// Ctrl+I liga/desliga. Com o painel ligado, passar o mouse por cima de um
/// sprite mostra os dados dele (nome no atlas, textura, retângulo, posição,
/// escala/rotação/tint, layer/depth e o "dono" — a entidade que desenhou,
/// se houver). Clique esquerdo trava a seleção atual (pra poder ler com
/// calma sem o mouse continuar mudando o alvo); clique de novo pra soltar.
/// </summary>
public class SpriteInspectorPanel : IDevPanel
{
	public string Title => locked ? "Sprite Inspector (travado)" : "Sprite Inspector";

	public Color TitleColor => new Color(70, 130, 210);

	private bool enabled;

	private bool locked;

	private bool comboWasDown;

	private bool leftWasDown;

	private SpritePickResult lastDrawnPick;

	public void Update(GameTime gameTime)
	{
		KeyboardState keyboard = Keyboard.GetState();
		bool ctrlDown = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
		bool comboDown = ctrlDown && keyboard.IsKeyDown(Keys.I);
		if (comboDown && !comboWasDown)
		{
			enabled = !enabled;
			locked = false;
		}
		comboWasDown = comboDown;

		if (!enabled)
		{
			return;
		}

		MouseState mouse = Mouse.GetState();
		bool leftDown = mouse.LeftButton == ButtonState.Pressed;
		if (leftDown && !leftWasDown)
		{
			locked = !locked;
		}
		leftWasDown = leftDown;

		if (!locked)
		{
			Vector2 logicalPos = Core.Instance.ResolutionManager.WindowToLogical(new Vector2(mouse.X, mouse.Y));
			Core.Instance.Renderer.PickQuery = logicalPos;
		}
	}

	public void CollectLines(Renderer renderer, List<string> lines)
	{
		if (!enabled)
		{
			return;
		}

		Core core = Core.Instance;
		SpritePickResult pick = renderer.LastPick;
		lastDrawnPick = pick;

		if (!pick.Hit)
		{
			lines.Add(locked ? "(travado, mas nada foi selecionado)" : "clique num sprite pra travar a seleção");
			return;
		}

		string spriteName = core.SpriteManager.FindSpriteName(pick.Texture, pick.SourceRect);
		string textureName = core.SpriteManager.GetTextureName(pick.Texture);
		string ownerName = pick.Owner != null ? pick.Owner.GetType().Name : "-";

		lines.Add($"Sprite: {spriteName}");
		lines.Add($"Textura: {textureName}");
		lines.Add($"Atlas: {pick.SourceRect.X},{pick.SourceRect.Y} {pick.SourceRect.Width}x{pick.SourceRect.Height}");
		lines.Add($"Posição: {pick.Position.X:0.0}, {pick.Position.Y:0.0}  Depth: {pick.Depth}");
		lines.Add($"Escala: {pick.Scale.X:0.00}x{pick.Scale.Y:0.00}  Rot: {MathHelper.ToDegrees(pick.Rotation):0.0}°  Flip: {pick.Flip}");
		lines.Add($"Tint: R{pick.Tint.R} G{pick.Tint.G} B{pick.Tint.B} A{pick.Tint.A}");
		lines.Add($"Owner: {ownerName}");
	}

	/// <summary>
	/// Desenha o preview ampliado do recorte do atlas. Chamado à parte pelo
	/// EngineDiagnostics (não faz parte da interface IDevPanel porque nenhum
	/// outro painel precisa desenhar uma imagem, só texto).
	/// </summary>
	public void DrawPreview(Renderer renderer, int layer, float x, float y)
	{
		if (!enabled || !lastDrawnPick.Hit)
		{
			return;
		}

		SpritePickResult pick = lastDrawnPick;
		const float maxPreviewSize = 128f;
		float zoom = MathHelper.Clamp(maxPreviewSize / Math.Max(pick.SourceRect.Width, pick.SourceRect.Height), 2f, 10f);

		Renderer r = renderer["fg", layer, false];

		Sprite previewSprite = new Sprite
		{
			X = pick.SourceRect.X,
			Y = pick.SourceRect.Y,
			SrcWidth = pick.SourceRect.Width,
			SrcHeight = pick.SourceRect.Height,
			Width = pick.SourceRect.Width,
			Height = pick.SourceRect.Height,
			TextureName = Core.Instance.SpriteManager.GetTextureName(pick.Texture)
		};
		r.DrawSpriteS(previewSprite, new Vector2(x + 4f, y + 4f), Color.White, new Vector2(zoom));
	}
}
#endif
