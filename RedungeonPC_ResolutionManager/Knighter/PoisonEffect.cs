using Knighter.Messages;
using Knighter.States;

namespace Knighter;

public class PoisonEffect : SpellEffect
{
	private int duration = 600;

	public PoisonEffect(PlayState playState)
		: base(playState)
	{
		transD = 40;
	}

	public override void Activate()
	{
		base.Activate();
		SendMessage(new PlaySoundMessage(SoundName.poisoning_on));
		Strength = duration;
		base.core.Renderer.PostEffectManager.EnableEffect(PostEffectType.Drunk);
	}

	public override void Deactivate()
	{
		if (base.Active)
		{
			SendMessage(new PlaySoundMessage(SoundName.poisoning_off));
		}
		base.Deactivate();
		Strength = 0;
	}

	public override void DeactivatePosteffects()
	{
		base.core.Renderer.PostEffectManager.DrunkDoublingA = 0f;
		base.core.Renderer.PostEffectManager.DisableEffect(PostEffectType.Drunk);
	}

	public override void UpdatePosteffects()
	{
		base.core.Renderer.PostEffectManager.DrunkA = 0.02f * base.transA;
		base.core.Renderer.PostEffectManager.DrunkDoublingA = 0.1f * base.transA;
	}

	public override void Update()
	{
		base.Update();
		if (base.Active)
		{
			Strength--;
			if (Strength == 0)
			{
				Deactivate();
			}
		}
	}
}
