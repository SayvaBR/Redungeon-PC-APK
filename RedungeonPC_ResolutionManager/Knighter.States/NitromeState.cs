using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class NitromeState : State
{
	protected override bool ShowContextPromptLegend => false;

	private Animation knight;

	private Animation flame1;

	private Animation flame2;

	private Animation cuboy;

	private int t;

	private int delay = 90;

	private int duration = 240;

	private bool stepped;

	public NitromeState()
	{
		base.TransDuration = 50;
		ShowCoins = false;
		knight = new Animation(0.5f);
		knight.Add("walk", "nitrome_knight_", "123456789abc");
		knight.Play("walk");
		flame1 = new Animation(0.3f);
		flame1.Add("ignite", "nitrome_ignite_", "1234567");
		flame1.Add("burn", "nitrome_flame_", "12345678");
		flame1.Loop = false;
		flame2 = new Animation(0.3f);
		flame2.Add("ignite", "nitrome_ignite_", "1234567");
		flame2.Add("burn", "nitrome_flame_", "12345678");
		flame2.Loop = false;
		cuboy = new Animation(0.5f);
		cuboy.Add("appear", "cuboy_", "123456789abcdefghij");
		cuboy.Loop = false;
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		if (base.core.Input.WasPressed(GameAction.Confirm))
		{
			TransitionOut(CoreEvent.ShowEnemindsLogo);
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (item.State == TouchLocationState.Pressed)
			{
				TransitionOut(CoreEvent.ShowEnemindsLogo);
				break;
			}
		}
		base.HandleInput();
	}

	public override void Update()
	{
		t++;
		int num = (int)Component._M(0f, t - delay);
		float num2 = 1f;
		if (Transition != TransType.None)
		{
			num2 = (float)base.Trans / (float)base.TransDuration;
		}
		if (!stepped)
		{
			SoundName soundName = SoundName.none;
			if (knight.GetCurrentFrameNumber() == 3)
			{
				soundName = SoundName.knight_step_1;
			}
			if (knight.GetCurrentFrameNumber() == 9)
			{
				soundName = SoundName.knight_step_2;
			}
			if (soundName != SoundName.none)
			{
				stepped = true;
				SendMessage(new PlaySoundMessage(soundName, 0.3f * Component._sin(3.14f * (float)t / (float)(120 + delay * 2)) * num2, 0f, (0f - ((float)t / (float)(120 + delay * 2) - 0.5f)) * 2f));
			}
		}
		else if (knight.GetCurrentFrameNumber() != 3 && knight.GetCurrentFrameNumber() != 9)
		{
			stepped = false;
		}
		knight.Update();
		flame1.Update();
		if (flame1.JustStopped)
		{
			flame1.Play("burn");
			flame1.Loop = true;
		}
		flame2.Update();
		if (flame2.JustStopped)
		{
			flame2.Play("burn");
			flame2.Loop = true;
		}
		cuboy.Update();
		if (num == 45)
		{
			flame1.Play("ignite");
			SendMessage(new PlaySoundMessage(SoundName.nitrome_torch, num2, Component._rnd(-0.1f, 0.1f), 0.2f));
		}
		if (num == 75)
		{
			flame2.Play("ignite");
			SendMessage(new PlaySoundMessage(SoundName.nitrome_torch, num2, Component._rnd(-0.1f, 0.1f), -0.2f));
		}
		if (num == 130)
		{
			cuboy.Play("appear");
			SendMessage(new PlaySoundMessage(SoundName.nitrome, num2));
		}
		if (t == duration + delay && Transition == TransType.None)
		{
			TransitionOut(CoreEvent.ShowEnemindsLogo);
		}
		base.Update();
	}

	public override void Load()
	{
		base.core.Renderer.AddLayer("bg_overlay", -1, BlendState.AlphaBlend);
		base.core.Renderer.AddLayer("overlay", 1, BlendState.Additive);
		base.core.Renderer.AddLayer("fg_overlay", 3, BlendState.AlphaBlend);
		base.core.Renderer.CustomDrawEnabled = true;
		base.Load();
	}

	public override void Unload()
	{
		base.core.Renderer.CustomDrawEnabled = false;
		base.core.Renderer.RemoveLayer("bg_overlay");
		base.core.Renderer.RemoveLayer("overlay");
		base.core.Renderer.RemoveLayer("fg_overlay");
		base.Unload();
	}

	public override void Draw()
	{
		int num = (int)Component._M(0f, t - delay);
		base.core.Renderer["bg"].FillScreen(Color.Black);
		Vector2 v = base.core.Renderer.ScreenCenter.Shift(-5f, 10f);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_ground), v.Shift(-75f, -50f), Color.White * 0.8f);
		Vector2 vector = v.Shift(-44f, -57f);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_dim), vector);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_dim_shade), vector.Shift(0f, -1f));
		int num2 = num - 20;
		float num3 = (float)Tween.BackEaseOut(num2, 0.0, 0.699999988079071, 30.0);
		float num4 = 0.9f + 0.1f * Component._sin((float)base.ticks * 0.4f);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_e), vector, Color.White * num3 * num4);
		int num5 = num2 - 10;
		num3 = (float)Tween.BackEaseOut(num5, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_m), vector, Color.White * num3 * num4);
		int num6 = num5 - 15;
		num3 = (float)Tween.BackEaseOut(num6, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_o), vector, Color.White * num3 * num4);
		int num7 = num6 - 10;
		num3 = (float)Tween.BackEaseOut(num7, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_r), vector, Color.White * num3 * num4);
		int num8 = num7 - 10;
		num3 = (float)Tween.BackEaseOut(num8, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_t), vector, Color.White * num3 * num4);
		int num9 = num8 - 10;
		num3 = (float)Tween.BackEaseOut(num9, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_i), vector, Color.White * num3 * num4);
		num3 = (float)Tween.BackEaseOut(num9 - 5, 0.0, 0.800000011920929, 30.0);
		base.core.Renderer["bg"].DrawSpriteS(_(SpriteName.nitrome_n), vector, Color.White * num3 * num4);
		int num10 = 120;
		if (num < num10)
		{
			Vector2 vector2 = v.Shift(80f, -80f) + new Vector2(-180f, 90f) * (Component._m(num, num10) / (float)num10);
			float num11 = ((num < 30) ? ((float)num / 30f) : ((num > num10 - 30) ? (1f - (float)(num - (num10 - 30)) / 30f) : 1f));
			base.core.Renderer.DrawSpriteS(knight.GetCurrentFrame(), vector2, Color.Lerp(Color.Black, Color.White, num11));
			base.core.Renderer["bg_overlay"].DrawSoftGlow(vector2.Shift(17f, 41f), new Vector2(28f, 10f), Color.Black * num11 * 0.35f);
		}
		base.core.Renderer.DrawSpriteS(_((flame1.CurrentSequence != "") ? SpriteName.nitrome_lit_column : SpriteName.nitrome_dim_column), v.Shift(17f, -6f));
		if (flame1.CurrentSequence != "")
		{
			base.core.Renderer.DrawSpriteS(flame1.GetCurrentFrame(), v.Shift(15f, -28f));
			float num12 = Component._m(num - 45, 10f) / 10f;
			base.core.Renderer.DrawSpriteS(_(SpriteName.nitrome_flame_glow_1), v.Shift(32f, -5f), Color.White * (0.2f + 0.1f * Component._sin((float)base.ticks * 0.2f)), Vector2.One * num12, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["overlay"].DrawSpriteS(_(SpriteName.nitrome_flame_glow_2), v.Shift(32f, -5f), Color.White * (0.9f + 0.1f * Component._sin((float)base.ticks * 0.2f)), Vector2.One * num12, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.core.Renderer.DrawSpriteS(_((flame2.CurrentSequence != "") ? SpriteName.nitrome_lit_column : SpriteName.nitrome_dim_column), v.Shift(-31f, 17f));
		if (flame2.CurrentSequence != "")
		{
			base.core.Renderer.DrawSpriteS(flame2.GetCurrentFrame(), v.Shift(-33f, -5f));
			float num13 = Component._m(num - 75, 10f) / 10f;
			base.core.Renderer.DrawSpriteS(_(SpriteName.nitrome_flame_glow_1), v.Shift(-16f, 18f), Color.White * (0.2f + 0.1f * Component._sin((float)base.ticks * 0.2f)), Vector2.One * num13, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["overlay"].DrawSpriteS(_(SpriteName.nitrome_flame_glow_2), v.Shift(-16f, 18f), Color.White * (0.9f + 0.1f * Component._sin((float)base.ticks * 0.2f)), Vector2.One * num13, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (cuboy.CurrentSequence != "")
		{
			base.core.Renderer.DrawSpriteS(cuboy.GetCurrentFrame(), v.Shift(43f, -84f));
		}
		// The atlas already contains the blue torch lighting. A full-screen
		// blue alpha layer tints even the black background in the desktop port.
		if (Transition != TransType.None)
		{
			base.core.Renderer["fg_overlay"].FillScreen(Color.Black * (1f - (float)base.Trans / (float)base.TransDuration));
		}
		base.Draw();
	}

	public override void OnBackButtonPressed()
	{
		base.core.SystemCalls.MinimizeGame();
		base.OnBackButtonPressed();
	}
}
