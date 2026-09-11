using System;
using System.Collections.Generic;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class GameHud : Component
{
	private enum Button
	{
		Pause
	}

	public enum AlertKind
	{
		Stripe,
		Text
	}

	private class Alert
	{
		public string Id;

		public string Text;

		public int TTL;

		public int Age;

		public Color Color;

		public Sprite Icon;

		public AlertKind Kind;
	}

	/// <summary>
	/// Popup de conquista desbloqueada, estilo Steam: caixinha fixa no
	/// canto inferior direito, com ícone + nome, que desliza pra cima ao
	/// aparecer e desce de volta ao sumir (ver DrawAchievementToast).
	/// </summary>
	private class AchievementToast
	{
		public string Text;

		public Sprite Icon;

		public Color ColorBG;

		public Color ColorFG;

		public Color ColorFrame;

		public int TTL;

		public int Age;
	}

	private readonly PlayState playState;

	private readonly TouchMenu<Button> touchMenu;

	public AbilitiesHud AbilitiesHud;

	private float displayCoins;

	private Queue<Alert> alerts;

	private readonly Queue<AchievementToast> achievementToasts = new Queue<AchievementToast>();

	private Sprite swoosh;

	public GameHud(PlayState playState)
	{
		this.playState = playState;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease);
		// Keep the pause hit target fully inside the portrait safe area. The
		// original -5 offset was harmless on the wide layout, but clipped the
		// icon and its touch target on tall phones.
		float topMargin = Settings.IsTouchDevice && base.core.Renderer.ScreenHeight > base.core.Renderer.ScreenWidth ? 6f : -5f;
		touchMenu.SetupButton(Button.Pause, new RectangleF(topMargin, topMargin + base.topSafeArea, 30f, 30f), _(SpriteName.button_pause), _(SpriteName.button_pause_pressed));
		alerts = new Queue<Alert>();
		swoosh = _(SpriteName.alert_swoosh);
	}

	public override void Load()
	{
		AbilitiesHud = new AbilitiesHud(playState.Player.Abilities.SkillLevel, shopMode: false, 18 + base.topSafeArea);
		base.Load();
	}

	public override void Update()
	{
		int collectedCoins = playState.Session.CollectedCoins;
		if (Math.Abs((float)collectedCoins - displayCoins) > 1f)
		{
			displayCoins += ((float)collectedCoins - displayCoins) * 0.1f;
		}
		else
		{
			displayCoins = collectedCoins;
		}
		AbilitiesHud.Update();
		if (alerts.Count > 0)
		{
			Alert alert = alerts.Peek();
			alert.Age++;
			if (alert.Age > alert.TTL)
			{
				alerts.Dequeue();
			}
		}
		if (achievementToasts.Count > 0)
		{
			AchievementToast toast = achievementToasts.Peek();
			toast.Age++;
			if (toast.Age > toast.TTL)
			{
				achievementToasts.Dequeue();
			}
		}
		base.Update();
	}

	/// <summary>
	/// Mostra o popup estilo Steam de conquista desbloqueada (canto
	/// inferior direito, com som e uma vibração bem curta). Enfileira se
	/// já houver outro popup na tela, pra nunca sobrepor dois de uma vez.
	/// </summary>
	public void ShowAchievementUnlocked(AchievementMeta meta)
	{
		achievementToasts.Enqueue(new AchievementToast
		{
			Text = __(meta.Name),
			Icon = _(meta.Icon),
			ColorBG = meta.ColorBG,
			ColorFG = meta.ColorFG,
			ColorFrame = meta.ColorFrame,
			TTL = 260,
			Age = 0
		});
		SendMessage(new PlaySoundMessage(SoundName.upgrade));
		Rumble.Pulse("achievement-unlocked", 0.3f, 0.15f, 12);
	}

	public void ShowAlert(string id, string message, Color color, int ttl = 120, SpriteName? icon = null, AlertKind kind = AlertKind.Stripe)
	{
		Alert alert = new Alert
		{
			Id = id,
			Text = message,
			Color = color,
			TTL = ttl,
			Age = 0,
			Icon = ((!icon.HasValue) ? null : _(icon.Value)),
			Kind = kind
		};
		bool flag = false;
		foreach (Alert alert2 in alerts)
		{
			if (alert2.Id == id)
			{
				alert2.Text = alert.Text;
				alert2.Color = alert.Color;
				alert2.TTL = ttl + alert2.Age;
				alert2.Icon = alert.Icon;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			alerts.Enqueue(alert);
		}
	}

	public bool HandleInput()
	{
		if (playState.Transition != State.TransType.None)
		{
			return false;
		}
		return touchMenu.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		TryToPause();
	}

	public void TryToPause()
	{
		if (!base.core.CurrentPlayState.Player.Dead)
		{
			SendMessage(new CoreEventMessage(CoreEvent.MakeScreenshotWhileDrawing));
			base.core.CurrentPlayState.Pause(enteringBackground: false);
			playState.TransitionOut(CoreEvent.ShowPause);
		}
	}

	private void DrawCoinChain()
	{
		var chain = playState.Session.CoinChain;
		if (chain.Remaining == 0) return;
		float x = core.Renderer.ScreenWidth - 92f;
		float y = topSafeArea + 30f;
		float fade = Math.Min(1f, chain.Remaining / 25f);
		R["fg", 9000, false].DrawRectangleS(new RectangleF(x - 4, y - 2, 88, 25), new Color(30, 23, 29) * (0.9f * fade));
		float pulse = 1f + chain.PulseTicks / 80f;
		R["fg", 9001, false].DrawTextS($"COMBO x{chain.Multiplier}", new Vector2(x, y), TextProfile.OrangeBoldText.Alter(scale: 0.6f * pulse, color: Color.Gold * fade, width: 80, height: 16));
		R["fg", 9001, false].DrawRectangleS(new RectangleF(x, y + 16, 80, 3), new Color(89, 62, 40) * fade);
		R["fg", 9002, false].DrawRectangleS(new RectangleF(x, y + 16, 80 * chain.Fill, 3), Color.Gold * fade);
	}

	public override void Draw()
	{
		if (base.core.TakingScreenshot)
		{
			return;
		}
		if (playState == base.core.GetCurrentState())
		{
			float num = 0f;
			if (playState.Transition != State.TransType.None)
			{
				num = (float)Tween.BackEaseOut(playState.Trans, -50.0, 50.0, playState.TransDuration);
			}
			touchMenu[Button.Pause].Rectangle.Shift(num);
			base.core.Renderer["fg", -1, false].DrawRectangleS(new RectangleF(-1f, num - 10f, base.core.Renderer.ScreenWidth + 2, AbilitiesHud.panelMiddle.Height - 3 + 10 + base.topSafeArea), Color.Black * 0.5f);
			base.core.Renderer["fg", -2, false].DrawRectangleS(new RectangleF(-1f, num - 10f, base.core.Renderer.ScreenWidth + 2, AbilitiesHud.panelMiddle.Height - 3 + 18 + base.topSafeArea), Color.Black * 0.25f);
			base.core.Renderer["fg", -1, false].DrawSpriteS(AbilitiesHud.panelBottom, new Vector2(0f, num + (float)base.topSafeArea + 21f), null, new Vector2(base.core.Renderer.ScreenWidth, 1f));
		}
		if (base.core.GetCurrentState() != playState || !playState.Started)
		{
			return;
		}
		touchMenu.Draw();
		DrawCoinChain();
		bool newBest = base.core.CurrentPlayState.NewBest;
		string text = playState.Session.Distance + __(SId.MISC_meters);
		string text2 = "";
		if (base.core.ProfileData.BestDistance != 0)
		{
			text2 = (newBest ? __(SId.HUD_new_best) : string.Format(__(SId.HUD_best), base.core.ProfileData.BestDistance + __(SId.MISC_meters)));
		}
		bool portrait = Settings.IsTouchDevice && base.core.Renderer.ScreenHeight > base.core.Renderer.ScreenWidth;
		float num2 = (float)Tween.BackEaseOut(playState.Trans, -15.0, 15.0, playState.TransDuration) + (float)base.topSafeArea;
		float hudTextScale = portrait ? 1.2f : 1f;
		float sideTextScale = portrait ? 0.78f : (newBest ? (0.55f + (float)Math.Sin((float)base.ticks / 20f) * 0.05f) : 0.5f);
		base.core.Renderer["fg"].DrawTextS(text, new Vector2(base.core.Renderer.ScreenWidth / 2, portrait ? 12f + num2 : 8.5f + num2), new TextProfile
		{
			Font = Font.Bold,
			Color = Color.White,
			SecondColor = Color.Black,
			BoxAlignment = Alignment2D.Center,
			TextAlignment = Alignment2D.Middle,
			Decoration = TextDecoration.Contour,
			Width = base.core.Renderer.ScreenWidth,
			Scale = hudTextScale
		});
		base.core.Renderer["fg"].DrawTextS(text2, portrait
			? new Vector2(42f, 12f + num2)
			: new Vector2(base.core.Renderer.ScreenWidth / 4, 9f + num2), new TextProfile
		{
			Font = Font.Bold,
			Color = TextProfile.OrangeLight,
			SecondColor = Color.Black,
			BoxAlignment = portrait ? Alignment2D.LeftMiddle : Alignment2D.Center,
			TextAlignment = portrait ? Alignment2D.LeftMiddle : Alignment2D.Middle,
			Decoration = TextDecoration.Contour,
			Width = portrait ? 105 : base.core.Renderer.ScreenWidth / 6,
			Scale = sideTextScale
		});
		string text3 = "+^" + (int)Math.Ceiling(displayCoins);
		base.core.Renderer["fg", 10000, false].DrawTextS(text3, new Vector2(base.core.Renderer.ScreenWidth - (portrait ? 8f : 3f), portrait ? 10f + num2 : 4f + num2), TextProfile.OrangeBoldText.Alter(null, boxAlignment: Alignment2D.Right, textAlignment: Alignment2D.Right, width: base.core.Renderer.ScreenWidth, decoration: TextDecoration.Contour, secondColor: Color.Black, scale: portrait ? 1.2f : null));
		if (base.core.GetCurrentState() == playState)
		{
			AbilitiesHud.Draw();
			if (alerts.Count > 0)
			{
				Alert alert = alerts.Peek();
				int num3 = ((alert.Kind == AlertKind.Text) ? 25 : 20);
				float num4 = 1f;
				if (alert.Age < num3)
				{
					num4 = (float)Tween.BackEaseOut(alert.Age, 0.0, 1.0, num3);
				}
				if (alert.Age > alert.TTL - num3)
				{
					num4 = 0f - (float)Tween.BackEaseOut(alert.TTL - alert.Age, 0.0, 1.0, num3);
				}
				float num5 = Math.Abs(num4);
				switch (alert.Kind)
				{
				case AlertKind.Stripe:
				{
					base.core.Renderer["fg", 1001, false].DrawRectangleS(new RectangleF(-1f, 54f - 8f * num5 + (float)base.topSafeArea, base.core.Renderer.ScreenWidth + 2, 16f * num5), alert.Color * num5 * 0.7f);
					for (int i = 1; i <= 3; i++)
					{
						base.core.Renderer["fg", 1001, false].DrawSpriteS(swoosh, new Vector2(base.core.Renderer.ScreenWidth + 50 - base.ticks * i * 6 % (base.core.Renderer.ScreenWidth + 100), 54 + base.topSafeArea), alert.Color, new Vector2(2f, num5), 0f, (i % 2 != 0) ? SpriteFlip.Vertical : SpriteFlip.None, SpriteOrigin.Center);
					}
					float x = (float)(base.core.Renderer.ScreenWidth / 2) + (float)base.core.Renderer.ScreenWidth * ((num4 >= 0f) ? (1f - num4) : (num5 - 1f));
					float num6 = ((alert.Icon != null) ? (alert.Icon.Width + 4) : 0);
					float width = base.core.Renderer["fg", 1001, false].DrawTextS(alert.Text, new Vector2(num6 / 2f + x, 52 + base.topSafeArea), new TextProfile
					{
						Font = Font.Bold,
						Color = Color.White,
						BoxAlignment = Alignment2D.Middle,
						TextAlignment = Alignment2D.Middle,
						Decoration = TextDecoration.None,
						Width = base.core.Renderer.ScreenWidth - (int)num6,
						Height = 20,
						Scale = 1f + Component._sin((float)base.ticks * 0.07f) * 0.01f
					}).Width;
					if (alert.Icon != null)
					{
						base.core.Renderer["fg", 1001, false].DrawSpriteS(alert.Icon, new Vector2(x - width / 2f, 54 + base.topSafeArea), Color.White * num5, Vector2.One * num5, 0f, SpriteFlip.None, SpriteOrigin.Center);
					}
					break;
				}
				case AlertKind.Text:
				{
					float x = base.core.Renderer.ScreenWidth / 2;
					base.core.Renderer["fg", 1001, false].DrawTextS(alert.Text, new Vector2(x, 50f * num5 + (float)base.topSafeArea), new TextProfile
					{
						Font = Font.Bold,
						Color = alert.Color,
						SecondColor = Color.Black,
						BoxAlignment = Alignment2D.Middle,
						TextAlignment = Alignment2D.Middle,
						Decoration = TextDecoration.Contour,
						Width = (int)((float)base.core.Renderer.ScreenWidth * 1.5f),
						Height = 20,
						Scale = (2f + Component._sin((float)alert.Age * 0.07f) * 0.2f) * num5
					});
					break;
				}
				}
			}
			DrawAchievementToast();
		}
		base.Draw();
	}

	/// <summary>
	/// Caixinha fixa no canto inferior direito (selo da conquista + nome),
	/// deslizando de baixo pra cima ao entrar e de volta pra baixo ao sair
	/// — o mesmo "presence" de 0 a 1 via Tween.BackEaseOut usado nos
	/// alerts acima, só que aplicado a um deslocamento vertical em vez de
	/// horizontal. O selo reaproveita os mesmos sprites (fundo/brilho/
	/// moldura/ícone) que a tela de conquistas (FactsState._achievement)
	/// já usa, em vez de um retângulo genérico.
	/// </summary>
	private void DrawAchievementToast()
	{
		if (achievementToasts.Count == 0)
		{
			return;
		}
		AchievementToast toast = achievementToasts.Peek();
		const int introFrames = 20;
		float presence = 1f;
		if (toast.Age < introFrames)
		{
			presence = (float)Tween.BackEaseOut(toast.Age, 0.0, 1.0, introFrames);
		}
		if (toast.Age > toast.TTL - introFrames)
		{
			presence = 0f - (float)Tween.BackEaseOut(toast.TTL - toast.Age, 0.0, 1.0, introFrames);
		}
		float alpha = Math.Abs(presence);

		// A largura do selo agora é medida de verdade (Sprite.Width), não
		// chutada — era isso que tava deixando o texto fora da parte
		// colorida. A altura continua fixa (38) porque a posição vertical
		// já foi confirmada correta com esse valor; não mexo nisso agora
		// pra não voltar a quebrar o que já tá certo.
		Sprite badgeBg = _(SpriteName.facts_achievement_bg);
		float badgeW = badgeBg.Width;
		const float badgeH = 38f;
		const float textWidth = 108f;
		const float padding = 6f;
		float width = badgeW + padding * 3f + textWidth;
		float height = badgeH + padding * 2f;
		const float margin = 6f;
		float boxX = base.core.Renderer.ScreenWidth - margin - width;
		float restY = base.core.Renderer.ScreenHeight - margin - height;
		float boxY = restY + (1f - presence) * (height + margin);
		base.core.DebugWatch("achievement-badge-size", $"bg {badgeBg.Width}x{badgeBg.Height}");

		base.core.Renderer["fg", 1002, false].DrawRectangleS(new RectangleF(boxX, boxY, width, height), Color.Black * 0.75f * alpha);

		// Mesmos deslocamentos relativos que FactsState._achievement() usa
		// entre fundo/brilho/moldura/ícone (copiados de lá, não inventados),
		// só reancorados pro canto do nosso toast.
		Vector2 frameAnchor = new Vector2(boxX + padding, boxY + padding + 1f);
		base.core.Renderer["fg", 1002, false].DrawSpriteS(badgeBg, frameAnchor + new Vector2(1f, 0.5f), toast.ColorBG * alpha);
		base.core.Renderer["fg", 1002, false].DrawSpriteS(_(SpriteName.facts_achievement_glow), frameAnchor, toast.ColorFG * alpha);
		base.core.Renderer["fg", 1002, false].DrawSpriteS(_(SpriteName.facts_achievement_frame), frameAnchor, toast.ColorFrame * alpha);
		if (toast.Icon != null)
		{
			base.core.Renderer["fg", 1002, false].DrawSpriteS(toast.Icon, frameAnchor + new Vector2(20f, 21f), Color.White * alpha, Vector2.One, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}

		float textX = boxX + padding * 2f + badgeW;
		float badgeCenterY = frameAnchor.Y + badgeH / 2f;
		base.core.Renderer["fg", 1002, false].DrawTextS("CONQUISTA DESBLOQUEADA", new Vector2(textX, badgeCenterY - 7f), new TextProfile
		{
			Font = Font.Thin,
			Color = toast.ColorFG * alpha,
			BoxAlignment = Alignment2D.LeftMiddle,
			TextAlignment = Alignment2D.LeftMiddle,
			Decoration = TextDecoration.None,
			Width = (int)textWidth,
			Height = 10,
			Scale = 0.6f
		});
		base.core.Renderer["fg", 1002, false].DrawTextS(toast.Text, new Vector2(textX, badgeCenterY + 7f), new TextProfile
		{
			Font = Font.Bold,
			Color = Color.White * alpha,
			SecondColor = Color.Black * alpha,
			BoxAlignment = Alignment2D.LeftMiddle,
			TextAlignment = Alignment2D.LeftMiddle,
			Decoration = TextDecoration.Contour,
			Width = (int)textWidth,
			Height = 14,
			Scale = 0.6f
		});
	}
}
