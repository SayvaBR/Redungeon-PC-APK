using Knighter;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Shaders;

public class DefaultShader : Shader
{
	public Texture2D LightMap;

	public override void Load()
	{
		LoadFromFile("Shaders/Default");
		base.Load();
	}

	public override void InitializeUniforms()
	{
		base.Effect.Parameters["WorldViewProj"].SetValue(BuildDefaultWorldViewProjMatrix());
		base.InitializeUniforms();
	}

	public override void UpdateUniforms()
	{
		base.Effect.Parameters["LightMap"].SetValue(LightMap);
		base.core.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
		base.UpdateUniforms();
	}
}
