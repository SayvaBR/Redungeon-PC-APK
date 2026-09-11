using Knighter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Shaders;

public class PostEffectShader : Shader
{
	private static readonly SamplerState CloudSampler = new() { Filter = TextureFilter.Point, AddressU = TextureAddressMode.Mirror, AddressV = TextureAddressMode.Mirror };
	public Texture2D Image;

	private PostEffectType postEffect;

	public override void Load()
	{
		LoadFromFile("Shaders/PostEffect");
		base.Load();
	}

	public override void InitializeUniforms()
	{
		base.Effect.Parameters["WorldViewProj"].SetValue(BuildDefaultWorldViewProjMatrix());
		base.InitializeUniforms();
	}

	public override void UpdateUniforms()
	{
		base.Effect.Parameters["Image"].SetValue(Image);
		base.core.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
		int num = 1800;
		int num2 = ((base.ticks / num % 2 == 0) ? (base.ticks % num) : (num - base.ticks % num));
		base.Effect.Parameters["Time"].SetValue((float)num2 / 60f);
		switch (postEffect)
		{
		case PostEffectType.Drunk:
			base.Effect.Parameters["Resolution"].SetValue(new Vector2(base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenHeight));
			base.Effect.Parameters["DrunkF"].SetValue(base.core.Renderer.PostEffectManager.DrunkF);
			base.Effect.Parameters["DrunkA"].SetValue(base.core.Renderer.PostEffectManager.DrunkA);
			base.Effect.Parameters["DrunkDoublingA"].SetValue(base.core.Renderer.PostEffectManager.DrunkDoublingA);
			break;
		case PostEffectType.Vignette:
			base.Effect.Parameters["InnerVignette"].SetValue(base.core.Renderer.PostEffectManager.InnerVignette);
			base.Effect.Parameters["OuterVignette"].SetValue(base.core.Renderer.PostEffectManager.OuterVignette);
			base.Effect.Parameters["VignetteTint"].SetValue(base.core.Renderer.PostEffectManager.VignetteTint.ToVector4());
			break;
		case PostEffectType.Spotlight:
			base.Effect.Parameters["SpotlightOpacity"].SetValue(base.core.Renderer.PostEffectManager.SpotlightOpacity);
			base.Effect.Parameters["Resolution"].SetValue(new Vector2(base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenHeight));
			base.Effect.Parameters["SpotlightCenter"].SetValue(base.core.Renderer.PostEffectManager.SpotlightCenter);
			base.Effect.Parameters["SpotlightRadius"].SetValue(base.core.Renderer.PostEffectManager.SpotlightRadius);
			base.Effect.Parameters["AuxImage"].SetValue(base.core.SpriteManager.Clouds);
			base.core.GraphicsDevice.SamplerStates[2] = CloudSampler;
			break;
		}
		base.UpdateUniforms();
	}

	public void SetPostEffect(PostEffectType postEffect)
	{
		switch (postEffect)
		{
		case PostEffectType.Drunk:
			base.Effect.CurrentTechnique = base.Effect.Techniques[0];
			break;
		case PostEffectType.Vignette:
			base.Effect.CurrentTechnique = base.Effect.Techniques[1];
			break;
		case PostEffectType.Spotlight:
			base.Effect.CurrentTechnique = base.Effect.Techniques[2];
			break;
		}
		this.postEffect = postEffect;
	}
}
