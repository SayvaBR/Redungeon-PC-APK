using Knighter;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Shaders;

public class OverlayShader : Shader
{
	public Texture2D Destination;

	public override void Load()
	{
		LoadFromFile("Shaders/Overlay");
		base.Load();
	}

	public override void InitializeUniforms()
	{
		base.Effect.Parameters["WorldViewProj"].SetValue(BuildDefaultWorldViewProjMatrix());
		base.InitializeUniforms();
	}

	public override void UpdateUniforms()
	{
		base.Effect.Parameters["Destination"].SetValue(Destination);
		base.core.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
		base.UpdateUniforms();
	}
}
