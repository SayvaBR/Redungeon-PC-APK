using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class PopupState : State
{
	public enum PopupKind
	{
		Message,
		Coins,
		HighlightFactsState
	}

	private enum Button
	{
		Achievements
	}

	private PopupKind kind;

	private string message;

	private bool coins;

	private ParticleEmitter coinsEmitter;

	private RectangleF buttonRect;

	private TouchMenu<Button> touchMenu;

	public PopupState(string message)
	{
		base.TransDuration = 30;
		IsOverlay = true;
		kind = PopupKind.Message;
		this.message = message;
	}

	public PopupState(RectangleF buttonRect)
	{
		base.TransDuration = 30;
		IsOverlay = true;
		kind = PopupKind.HighlightFactsState;
		ShowCoins = false;
		int num = base.core.Renderer.ScreenHeight - 40;
		float num2 = (float)(base.core.Renderer.ScreenWidth - 22) / 4f;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 10000);
		touchMenu.SetupButton(Button.Achievements, new RectangleF(10f + 1f * num2, num - 32, num2, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_achievements));
		this.buttonRect = buttonRect;
	}

	public PopupState(int coins)
	{
		kind = PopupKind.Coins;
		base.TransDuration = 20;
		IsOverlay = true;
		message = "^" + coins;
		this.coins = true;
		SendMessage(new PlaySoundMessage(SoundName.coins_falling));
		coinsEmitter = base.core.ParticleManager.AddEmitter(inWorld: false, base.core.Renderer.ScreenCenter, 10f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			if (p.Age < 50)
			{
				p.Position += p.Offset;
				p.Offset *= 0.9f;
				p.Velocity.X = 1f;
			}
			else
			{
				p.Position += (new Vector2(base.core.Renderer.ScreenWidth / 2, 5f) - p.Position) * 0.08f;
				p.Velocity.X = 1f - (float)(p.Age - 50) / 20f;
			}
			p.Dead = p.Age > 70;
		})
			.OnDraw(delegate(Particle p)
			{
				base.core.Renderer["fg", 9000, false].DrawSpriteS(_("coin_gold_" + ((int)((float)p.Age * 0.25f) % 6 + 1)), p.Position, Color.White * p.Velocity.X, Vector2.One * p.Velocity.X);
			})
			.Emit(5, 2, once: true, (int)Component._m(10f, coins / 5));
	}

	public override void Draw()
	{
		float num = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 9000, false].FillScreen(Color.Black * (1f - num * num * num) * 0.8f);
		switch (kind)
		{
		case PopupKind.Message:
		case PopupKind.Coins:
		{
			float num2 = (float)base.core.Renderer.ScreenWidth * 0.8f;
			int num3 = 90;
			RectangleF rectangleF = new RectangleF(base.core.Renderer.ScreenCenter.X - num2 * 0.5f, base.core.Renderer.ScreenCenter.Y - (float)num3 * 0.5f, num2, num3);
			rectangleF.Shift(0f, (float)Tween.BackEaseOut(base.Trans, -300.0, 300.0, base.TransDuration) + 10f);
			base.core.Renderer["fg", 9001, false].DrawTextS(message, rectangleF.CenterTop.Shift(0f, 20f), TextProfile.OrangeBoldText.Alter(textAlignment: Alignment2D.Center, boxAlignment: Alignment2D.Center, width: (int)num2, decoration: TextDecoration.Contour, color: default(Color).FromRgb(15967806) * (1f - num), secondColor: Color.Black, height: null, font: null, scale: 2f + num + Component._sin((float)base.ticks * 0.05f) * 0.06f));
			break;
		}
		case PopupKind.HighlightFactsState:
		{
			float height = base.core.Renderer["fg", 9001, false].DrawTextS(__(SId.MESSAGES_facts_hint_screen_name), new Vector2(base.core.Renderer.ScreenCenter.X, buttonRect.Top - 20f), TextProfile.OrangeBoldText.Alter(textAlignment: Alignment2D.BottomCenter, boxAlignment: Alignment2D.BottomCenter, width: (int)((float)base.core.Renderer.ScreenWidth * 0.8f), decoration: TextDecoration.None, color: default(Color).FromRgb(15967806) * (1f - num), secondColor: null, height: null, font: null, scale: 1.5f)).Height;
			base.core.Renderer["fg", 9001, false].DrawTextS(__(SId.MESSAGES_facts_hint), new Vector2(base.core.Renderer.ScreenCenter.X, buttonRect.Top - 20f - height), TextProfile.OrangeBoldText.Alter(textAlignment: Alignment2D.BottomCenter, boxAlignment: Alignment2D.BottomCenter, width: (int)((float)base.core.Renderer.ScreenWidth * 0.8f), decoration: TextDecoration.None, color: default(Color).FromRgb(16777215) * (1f - num), secondColor: null, height: null, font: null, scale: 1f));
			base.core.Renderer["fg", 9000, false].DrawRectangleS(touchMenu[Button.Achievements].Rectangle, Color.Black * (1f - num));
			base.core.Renderer["fg", 9000, false].DrawSpriteS(_(SpriteName.circle_21), touchMenu[Button.Achievements].Rectangle.Center, default(Color).FromRgb(15967806) * (1f - num) * 0.2f, Vector2.One * 1.6f, 0f, SpriteFlip.None, SpriteOrigin.Center);
			touchMenu?.Draw();
			break;
		}
		}
		base.Draw();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None && (kind != PopupKind.HighlightFactsState || base.TicksInState >= 60))
		{
			if (base.core.Input.WasPressed(GameAction.Confirm))
			{
				if (kind == PopupKind.HighlightFactsState)
				{
					OnButtonRelease(Button.Achievements);
				}
				else
				{
					OnBackButtonPressed();
				}
				return;
			}
			if (touchMenu != null && (touchMenu.HandleInput() || touchMenu[Button.Achievements].Pressed))
			{
				return;
			}
			if (base.core.TouchState.Count > 0)
			{
				OnBackButtonPressed();
			}
			base.HandleInput();
		}
	}

	private void OnButtonRelease(Button button)
	{
		OnBackButtonPressed();
		SendMessage(new PushStateMessage(new FactsState()), 35);
	}

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.PopState);
		base.OnBackButtonPressed();
	}
}
