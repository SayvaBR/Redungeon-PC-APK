using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

public class RenderTargets : Component
{
	public Texture2D DefaultRenderTarget;

	public Texture2D LightMapTarget { get; private set; }

	public Texture2D MainTarget { get; private set; }

	public Texture2D AuxTarget { get; private set; }

	public RenderTargets()
	{
		Allocate();
	}

	private void Allocate()
	{
		LightMapTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight);
		MainTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight, preserve: true);
		AuxTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight);
	}

	/// <summary>
	/// Recria os três render targets no tamanho atual do backbuffer.
	/// Necessário porque eles são sempre do tamanho exato do backbuffer:
	/// se a janela é redimensionada ou o modo fullscreen muda sem isso,
	/// os targets antigos ficam com o tamanho errado e a renderização quebra
	/// (distorção, recorte incorreto, ou exceções de tamanho no SetData).
	/// </summary>
	public void Recreate()
	{
		Unload();
		Allocate();
	}

	public override void Unload()
	{
		LightMapTarget?.Dispose();
		MainTarget?.Dispose();
		AuxTarget?.Dispose();
		LightMapTarget = MainTarget = AuxTarget = null;
	}
}
