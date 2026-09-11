using System;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class LogoState : State
{
	private ParticleEmitter polyominos;

	private int t;

	public LogoState()
	{
		base.TransDuration = 30;
		ShowCoins = false;
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		if (base.core.Input.WasPressed(GameAction.Confirm))
		{
			TransitionOut(CoreEvent.ResetGame);
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (item.State == TouchLocationState.Pressed)
			{
				TransitionOut(CoreEvent.ResetGame);
				break;
			}
		}
		base.HandleInput();
	}

	public override void Update()
	{
		t++;
		float num = 1f;
		if (Transition != TransType.None)
		{
			num = (float)base.Trans / (float)base.TransDuration;
		}
		if (t == 30)
		{
			SendMessage(new PlaySoundMessage(SoundName.nate_warmup, num * 0.5f, -0.3f));
		}
		if (t % 5 == 0 && t >= 90 && t <= 120)
		{
			SendMessage(new PlaySoundMessage(NathansDroneEntity.Beeps.DrawDifferent(), num));
		}
		if (Transition == TransType.None)
		{
			if (t == 210)
			{
				TransitionOut(CoreEvent.ResetGame);
			}
			base.Update();
		}
	}

	public override void Load()
	{
		polyominos = base.core.ParticleManager.AddEmitter(inWorld: false, base.core.Renderer.ScreenCenter.Shift(0f, -50f), (float)base.core.Renderer.ScreenWidth * 0.25f).OnSpawn(delegate(Particle p)
		{
			p.Aux.X = Component._rnd(1, 7);
			p.Aux.Y = Component._rnd(0, 1);
			p.Aux.Z = Component._rnd(0, 3);
			p.Aux.W = Component._rnd(0.05f, 0.2f);
			p.Velocity.X = 50f + (0.1f - p.Aux.W) * 40f;
		}).OnUpdate(delegate(Particle p)
		{
			p.Position = p.Position.Shift(p.Offset.X * p.Aux.W, (p.Offset.Y + (float)base.core.Renderer.ScreenWidth * 0.2f) * p.Aux.W);
			p.Dead = (float)p.Age > p.Velocity.X;
		})
			.OnDraw(delegate(Particle p)
			{
				float num = (float)p.Age / p.Velocity.X;
				base.core.Renderer[p.Age].DrawSpriteS(_(SpriteName.pixel), p.Position, rotation: (float)Math.PI / 2f * p.Aux.Z, flip: (!(p.Aux.Y < 0.5f)) ? SpriteFlip.Horizontal : SpriteFlip.None, scale: Vector2.One * (2.25f * num), tint: Color.Lerp(Color.Maroon, Color.Red, num) * num * ((num > 0.7f) ? (1f - (num - 0.7f) / 0.3f) : 1f), origin: SpriteOrigin.Center);
			})
			.Emit(40, 5, once: true, 5);
		base.Load();
	}

	public override void Unload()
	{
		polyominos.Kill();
		base.Unload();
	}

	public override void Draw()
	{
		base.core.Renderer["bg"].FillScreen(Color.Black);
		float num = ((t > 90) ? (1f + Component._sin((float)(t - 90) * 0.02f) * 0.1f) : ((t < 40) ? 0f : ((float)Tween.SineEaseOut(t - 40, 0.0, 1.0, 50.0))));
		float x = ((t < 90) ? (-50f * (1f - num)) : 0f);
		base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.eneminds), base.core.Renderer.ScreenCenter.Shift(x, -50f + num * 50f), Color.Lerp(Color.DimGray, Color.White, num) * ((num > 1.2f) ? Component._M(0f, 1f - (num - 1.2f) / 1.8f) : 1f), Vector2.One * num, Component._sin((float)t * 3.14f / 90f) * (1f - num * 0.9f), SpriteFlip.None, SpriteOrigin.Center);
		if (t % 40 < 20)
		{
			base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.eneminds_light_1), base.core.Renderer.ScreenCenter.Shift(x, -50f + num * 50f), null, Vector2.One * num, Component._sin((float)t * 3.14f / 90f) * (1f - num * 0.9f), SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.eneminds_light_3), base.core.Renderer.ScreenCenter.Shift(x, -50f + num * 50f), null, Vector2.One * num, Component._sin((float)t * 3.14f / 90f) * (1f - num * 0.9f), SpriteFlip.None, SpriteOrigin.Center);
		}
		else
		{
			base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.eneminds_light_2), base.core.Renderer.ScreenCenter.Shift(x, -50f + num * 50f), null, Vector2.One * num, Component._sin((float)t * 3.14f / 90f) * (1f - num * 0.9f), SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg"].DrawSpriteS(_(SpriteName.eneminds_light_4), base.core.Renderer.ScreenCenter.Shift(x, -50f + num * 50f), null, Vector2.One * num, Component._sin((float)t * 3.14f / 90f) * (1f - num * 0.9f), SpriteFlip.None, SpriteOrigin.Center);
		}
		base.core.Renderer["fg"].FillScreen(Color.Black * (1f - (float)base.Trans / (float)base.TransDuration));
		base.Draw();
	}

	public override void OnBackButtonPressed()
	{
		base.core.SystemCalls.MinimizeGame();
		base.OnBackButtonPressed();
	}
}
