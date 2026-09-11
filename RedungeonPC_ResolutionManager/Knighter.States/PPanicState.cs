using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class PPanicState : State
{
	private enum Button
	{
		Yes,
		No
	}

	private class Bot
	{
		public float X;

		public float Y;

		public int Age;

		public int Dir;

		public bool Dead;
	}

	private TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private int contentTop;

	private List<Bot> bots;

	private int nextBot;

	private int shakeT;

	public PPanicState()
	{
		base.TransDuration = 30;
		ShowCoins = false;
		IsOverlay = true;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 2000);
		contentTop = (int)base.core.Renderer.ScreenCenter.Shift(0f, -125f).Y;
		int num = contentTop + 200;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.Yes, new RectangleF(10 + 3 * num2 / 2 + 1, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.NITROME_PP_free), null, icon: true, iconIsPicture: false, blink: true);
		touchMenu.SetupButton(Button.No, new RectangleF(10 + num2 / 2, num, num2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_exit));
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, Button.No, Button.Yes);
		bots = new List<Bot>();
		for (int i = 0; i < 600; i++)
		{
			ProcessBots();
		}
	}

	public override void Load()
	{
		SendMessage(new PlaySoundMessage(SoundName.piston_extend));
		base.Load();
	}

	private void ProcessBots()
	{
		foreach (Bot bot2 in bots)
		{
			bot2.X += (float)bot2.Dir * 0.3f;
			bot2.Age++;
			if (bot2.X < -10f || bot2.X > (float)(base.core.Renderer.ScreenWidth + 10))
			{
				bot2.Dead = true;
			}
		}
		bots.RemoveAll((Bot b) => b.Dead);
		if (nextBot > 0)
		{
			nextBot--;
			return;
		}
		Bot bot = new Bot();
		int num = Component._rnd(1, 2);
		bot.X = ((num == 1) ? (-5) : (base.core.Renderer.ScreenWidth + 5));
		bot.Y = contentTop + ((num == 1) ? 115 : 155) - 1;
		bot.Age = 0;
		bot.Dir = ((num == 1) ? 1 : (-1));
		bots.Add(bot);
		nextBot = Component._rnd(60, 180);
	}

	public override void Update()
	{
		ProcessBots();
		if (shakeT > 0)
		{
			shakeT--;
		}
		base.Update();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.Yes].Rectangle.Shift(0f, (float)Tween.SineEaseOut(base.Trans, 200.0, -200.0, base.TransDuration));
		touchMenu[Button.No].Rectangle.Shift(0f, (float)Tween.SineEaseOut(base.Trans, 200.0, -200.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void Draw()
	{
		Vector2 vector = ((shakeT > 0) ? SciHelper.GetRandomVectorInCircle(3f) : Vector2.Zero);
		float num = (float)base.Trans * 2f / (float)base.TransDuration;
		if (num > 1f)
		{
			num = 1f;
		}
		base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * (0.9f * num));
		float y = (float)contentTop + (float)Tween.BackEaseOut(base.Trans, -200.0, 200.0, base.TransDuration);
		base.core.Renderer["fg", 3000, false].DrawSpriteS(_(SpriteName.ppanic_logo), new Vector2(base.core.Renderer.ScreenWidth / 2 + 7, y) + vector, null, null, (float)((Transition != TransType.In) ? 1 : (-1)) * (1f - num) * 0.2f, SpriteFlip.None, SpriteOrigin.TopCenter);
		if (Transition == TransType.None && (base.TicksInState - base.TransDuration) % 60 < 30)
		{
			base.core.Renderer["fg", 3000, false].DrawSpriteS(_(SpriteName.ppanic_logo_led), new Vector2(base.core.Renderer.ScreenWidth / 2 + 7, y) + vector, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		}
		base.core.Renderer["fg", 3000, false].DrawTextS(__(SId.NITROME_PP_description), new Vector2(base.core.Renderer.ScreenCenter.X, contentTop + 173) + vector, TextProfile.OrangeBoldText.Alter(font: Font.Thin, color: TextProfile.OrangeLight * num, secondColor: null, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, width: base.core.Renderer.ScreenWidth - 20, height: 25, decoration: TextDecoration.None, scale: 0.75f / Settings.GuiScale));
		float num2 = (float)Tween.SineEaseOut(base.Trans, -base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenWidth, base.TransDuration);
		Sprite sprite = _(SpriteName.ppanic_floor);
		for (int i = 0; i * sprite.Width < base.core.Renderer.ScreenWidth; i++)
		{
			base.core.Renderer["fg", 3000, false].DrawSpriteS(sprite, new Vector2((float)(-5 + i * sprite.Width) + num2, contentTop + 115) + vector);
			base.core.Renderer["fg", 3000, false].DrawSpriteS(sprite, new Vector2((float)(base.core.Renderer.ScreenWidth + 5 - i * sprite.Width) - num2, contentTop + 155) + vector, null, null, 0f, SpriteFlip.None, SpriteOrigin.TopRight);
		}
		foreach (Bot bot in bots)
		{
			int num3 = bot.Age / 7 % 6 + 1;
			base.core.Renderer["fg", 3000, false].DrawSpriteS(_("ppanic_bot_" + num3), new Vector2(bot.X + num2 * (float)bot.Dir, bot.Y) + vector, null, null, 0f, (bot.Dir < 0) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.BottomCenter);
		}
		touchMenu.Draw();
		base.Draw();
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		bool handled = touchMenu.HandleInput();
		if (!handled)
		{
			handled = menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
		}
		if (!handled)
		{
			foreach (TouchLocation item in base.core.TouchState)
			{
				if (item.State != TouchLocationState.Pressed)
				{
					continue;
				}
				foreach (Bot bot in bots)
				{
					if ((item.Position.Shift(0f, 10f) - new Vector2(bot.X, bot.Y)).LengthSquared() < 225f)
					{
						SendMessage(new PlaySoundMessage(SoundName.spikes_break));
						bot.Dead = true;
						shakeT = 15;
						_inc(Stat.SmashedPanicBots);
						base.core.ParticleManager.AddEmitter(inWorld: false, new Vector2(bot.X, bot.Y - 10f), 10f).OnSpawn(delegate(Particle p)
						{
							p.Velocity = p.Offset * 0.1f;
							p.Position -= p.Offset * 0.8f;
							p.Aux.X = Component._rnd(-1, 1);
							p.Aux.Y = Component._rnd(-0.1f, 0.1f);
							p.Aux.Z = p.Parent.Count;
						}).OnUpdate(delegate(Particle p)
						{
							p.Position += p.Velocity;
							p.Velocity.Y += 0.05f;
							p.Aux.X += p.Aux.Y;
							p.Dead = p.Position.Y > (float)base.core.Renderer.ScreenHeight;
						})
							.OnDraw(delegate(Particle p)
							{
								base.core.Renderer["fg", 5000, false].DrawSpriteS(_((p.Aux.Z > 4f) ? SpriteName.panicbot_arm : ((p.Aux.Z > 2f) ? SpriteName.panicbot_leg : ((p.Aux.Z > 1f) ? SpriteName.panicbot_head : SpriteName.panicbot_bulb))), p.Position, null, null, p.Aux.X, SpriteFlip.None, SpriteOrigin.Center);
							})
							.Emit(1, 5, once: true, 6);
						break;
					}
				}
			}
		}
		base.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.Yes:
			base.core.SystemCalls.OpenUrl("https://play.google.com/store/apps/details?id=com.nitrome.platformpanic");
			break;
		case Button.No:
			OnBackButtonPressed();
			break;
		}
	}

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.PopState);
		SendMessage(new PlaySoundMessage(SoundName.piston_retract));
		base.OnBackButtonPressed();
	}
}
