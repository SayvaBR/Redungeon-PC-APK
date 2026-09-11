using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter;

public class DarknessEffect : SpellEffect
{
	private int duration = 600;

	private ParticleEmitter emitter;

	private Light light;

	public DarknessEffect(PlayState playState)
		: base(playState)
	{
		transD = 40;
	}

	public override void Activate()
	{
		base.Activate();
		Strength = duration;
		base.core.Renderer.PostEffectManager.EnableEffect(PostEffectType.Spotlight);
		SendMessage(new PlaySoundMessage(SoundName.mist_on));
		if (light != null)
		{
			light.Die();
		}
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(14876838), 5f, 0.2f, base.player);
		light.FollowRate = 1f;
		light.ChangeRate = 0.1f;
		light.Radius = 10f;
		light.TargetRadius = 5f;
		light.Intencity = 0f;
		light.TargetIntencity = 0.7f;
	}

	public override void Deactivate()
	{
		if (base.Active)
		{
			SendMessage(new PlaySoundMessage(SoundName.mist_off));
		}
		base.Deactivate();
		Strength = 0;
		emitter?.Stop();
		light?.Die();
	}

	public override void DeactivatePosteffects()
	{
		base.core.Renderer.PostEffectManager.DisableEffect(PostEffectType.Spotlight);
	}

	public override void UpdatePosteffects()
	{
		var renderer = core.Renderer;
		var center = renderer.ToScreen(player.WorldCenter);
		renderer.PostEffectManager.SpotlightCenter = center / new Vector2(renderer.ScreenWidth, renderer.ScreenHeight);
		renderer.PostEffectManager.SpotlightOpacity = MathHelper.Clamp(transA, 0f, 1f);
		base.core.Renderer.PostEffectManager.SpotlightRadius = 1f - base.transA * 0.8f;
		base.core.Renderer.PostEffectManager.SpotlightGrain = 195f;
	}

	public override void Update()
	{
		base.Update();
		if (base.Active)
		{
			if (emitter != null)
			{
				emitter.Radius = 200f - 170f * base.transA;
			}
			Strength--;
			if (Strength == 0)
			{
				Deactivate();
			}
		}
	}
}
