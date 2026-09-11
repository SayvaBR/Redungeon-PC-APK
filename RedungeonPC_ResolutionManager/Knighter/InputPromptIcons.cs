using Knighter.Graphics;

namespace Knighter;

public enum PromptAction
{
	Confirm,
	Cancel,
	Secondary,
	Pause,
	PreviousCategory,
	NextCategory,
	Screenshot
}

/// <summary>
/// Recortes das folhas de prompts fornecidas pelo port. A célula permanece
/// em resolução nativa e é escalada somente no ponto de desenho, evitando os
/// badges gigantes que apareciam quando o PNG inteiro era tratado como ícone.
/// </summary>
public static class InputPromptIcons
{
	private const int CellSize = 64;

	public static Sprite Get(PromptAction action)
	{
		if (Core.Instance?.SpriteManager == null)
		{
			return null;
		}

		if (InputDeviceTracker.CurrentDevice != InputDevice.Gamepad)
			return FullTexture("key-" + (action switch {
				PromptAction.Confirm => "enter", PromptAction.Cancel => "escape",
				PromptAction.Secondary => "q", PromptAction.Pause => "p",
				PromptAction.PreviousCategory => "shift", PromptAction.NextCategory => "tab", _ => "c" }));
		if (action == PromptAction.Pause && InputDeviceTracker.CurrentGamepadBrand == GamepadBrand.PlayStation)
			return FullTexture("pause-ps");
		bool playStation = InputDeviceTracker.CurrentGamepadBrand == GamepadBrand.PlayStation;
		int face = action switch { PromptAction.Confirm => 0, PromptAction.Cancel => 1,
			PromptAction.Secondary => 2, PromptAction.Screenshot => 3, _ => -1 };
		if (face >= 0)
			return new Sprite { TextureName = "input-prompts-clean", X = (face + (playStation ? 4 : 0)) * 64,
				Y = 0, SrcWidth = 64, SrcHeight = 64, Width = 64, Height = 64 };
		(string texture, int x, int y)? source = playStation
			? GetPlayStationSource(action)
			: GetXboxSource(action);
		if (!source.HasValue || Core.Instance.SpriteManager.GetTexture(source.Value.texture) == null)
		{
			return null;
		}

		return new Sprite
		{
			TextureName = source.Value.texture,
			X = source.Value.x,
			Y = source.Value.y,
			SrcWidth = CellSize,
			SrcHeight = CellSize,
			Width = CellSize,
			Height = CellSize
		};
	}

	private static Sprite FullTexture(string name)
	{
		var texture = Core.Instance.SpriteManager.GetTexture(name);
		return texture == null ? null : new Sprite { TextureName = name, Width = texture.Width, Height = texture.Height, SrcWidth = texture.Width, SrcHeight = texture.Height };
	}

	private static (string texture, int x, int y)? GetXboxSource(PromptAction action)
	{
		return action switch
		{
			PromptAction.Confirm => ("input-prompts-xbox", 128, 512), // A verde sólido
			PromptAction.Cancel => ("input-prompts-xbox", 256, 512), // B laranja sólido
			PromptAction.Secondary => ("input-prompts-xbox", 512, 512), // Y amarelo sólido
			PromptAction.Pause => ("input-prompts-xbox", 256, 448), // START
			PromptAction.PreviousCategory => ("input-prompts-xbox", 448, 192),
			PromptAction.NextCategory => ("input-prompts-xbox", 192, 128),
			PromptAction.Screenshot => ("input-prompts-xbox", 384, 512),
			_ => null
		};
	}

	private static (string texture, int x, int y)? GetPlayStationSource(PromptAction action)
	{
		return action switch
		{
			PromptAction.Confirm => ("input-prompts-playstation", 704, 704),
			PromptAction.Cancel => ("input-prompts-playstation", 576, 704),
			PromptAction.Secondary => ("input-prompts-playstation", 192, 640), // Y maps to triangle
			PromptAction.Screenshot => ("input-prompts-playstation", 64, 640), // X maps to square
			PromptAction.PreviousCategory => ("input-prompts-playstation", 128, 384),
			PromptAction.NextCategory => ("input-prompts-playstation", 640, 384),
			_ => null
		};
	}
}
