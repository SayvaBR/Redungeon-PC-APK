using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter;

public class ConfusionEffect : SpellEffect
{
	private int duration = 600;

	private int startTick;

	private Light light;

	public ConfusionEffect(PlayState playState)
		: base(playState)
	{
		transD = 30;
	}

	public override void Activate()
	{
		base.Activate();
		startTick = playState.WorldTicks;
		Strength = duration;
		SendMessage(new PlaySoundMessage(SoundName.confusion));
		if (light != null)
		{
			light.Die();
		}
		light = base.core.CurrentPlayState.LightManager.AddLight(default(Color).FromRgb(16497664), 5f, 0.2f, base.player);
		light.FollowRate = 1f;
		light.ChangeRate = 0.1f;
		light.Radius = 15f;
		light.TargetRadius = 15f;
		light.Intencity = 1.5f;
		light.TargetIntencity = 0.2f;
		playState.Camera.Shake("confusion", 3f);
	}

	public override void Deactivate()
	{
		if (base.Active)
		{
			playState.Camera.Shake("confusion", 3f);
		}
		base.Deactivate();
		Strength = 0;
		light?.Die();
	}

	public override void DeactivatePosteffects()
	{
	}

	public override void Draw()
	{
		if (base.transA < 0.01f)
		{
			return;
		}
		int num = playState.WorldTicks - startTick;
		int num2 = 7;
		for (int i = 0; i < num2; i++)
		{
			int num3 = duration - transD * 7;
			float num4 = (float)transD + (float)(i + 1) * (float)num3 / (float)num2;
			if (!((float)num >= num4))
			{
				float num5 = (((float)num < num4 - 15f) ? 0f : (1f - (num4 - (float)num) / 15f));
				float num6 = (float)i * (float)Math.PI * 2f / (float)num2;
				float num7 = Component._cos((float)num * 0.08f + num6) * 7f;
				float num8 = Component._sin((float)num * 0.08f + num6) * 3f;
				base.core.Renderer[base.player.LastLayer, base.player.LastZ + ((!(num8 < 0f)) ? 1 : (-1)), false].DrawSpriteW(_(SpriteName.confusion_star), base.player.WorldCenter + base.player.LastSpriteShift.Shift(0f + num7, num8 - 15f), ((num5 > 0f) ? Color.White : default(Color).FromRgb(16640817)) * base.player.LastSpriteAlpha, Vector2.One * 0.5f * base.transA * ((num5 > 0f) ? (2.5f * (1f - num5)) : 1f), (float)num * 0.1f + (float)(i * 5), SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		float num9 = 0f;
		if (num <= 20)
		{
			num9 = (float)(20 - num) / 20f;
		}
		if (Strength <= 20)
		{
			num9 = 1f - (float)(20 - Strength) / 20f;
		}
		if (!num9.IsZero() && !base.player.Dead)
		{
			base.core.Renderer["fg"].FillScreen(default(Color).FromRgb(16774009) * num9 * 0.8f);
		}
		base.Draw();
	}

	public override void UpdatePosteffects()
	{
	}

	public override void Update()
	{
		base.Update();
		if (!base.Active)
		{
			return;
		}
		Strength--;
		if (Strength == 0)
		{
			Deactivate();
		}
		int num = playState.WorldTicks - startTick;
		int num2 = 7;
		for (int i = 0; i < num2; i++)
		{
			int num3 = duration - transD * 7;
			float num4 = (float)transD + (float)(i + 1) * (float)num3 / (float)num2;
			if (num == (int)num4 - 15)
			{
				light.Intencity = ((i == num2 - 1) ? 1.5f : 0.5f);
				SendMessage(new PlaySoundMessage((i != num2 - 1) ? SoundName.confusion_tick : SoundName.confusion_last_tick, 1f, (i != num2 - 1) ? ((float)(num2 - i) * 0.1f) : 0f));
			}
		}
	}
}
