using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class DodgeState : State
{
	protected override bool ShowContextPromptLegend => false;
	private readonly Dictionary<SignCard, DirectionPattern> directionPatterns = new();
	private bool directionalMode = true;
	private int timer;

	private int maxTimer;

	private int delay;

	private int maxDelay;

	private MedusaChar medusa;

	private List<Vector2> points;

	private readonly SignRecognizer recognizer;

	private readonly List<SignCard> signCards;

	private readonly List<SignCard> selectedSignCards;

	private Texture2D renderTarget;

	private int pointsDrawn;

	private float currentR = 1f;

	private int level;

	private bool showTutorial;

	private Animation tutorial;

	private int drawn;

	private int accessibleSignIndex;

	public DodgeState(MedusaChar medusa, int level)
	{
		base.TransDuration = 25;
		ShowCoins = false;
		IsOverlay = true;
		maxTimer = Math.Max(300, 3 * Math.Clamp(2 + level, 2, 6) * 24);
		timer = maxTimer;
		maxDelay = 20;
		delay = maxDelay;
		this.medusa = medusa;
		points = new List<Vector2>();
		recognizer = new SignRecognizer();
		signCards = new List<SignCard>();
		selectedSignCards = new List<SignCard>();
		renderTarget = base.core.Renderer.CreateTexture(base.core.Renderer.BufferWidth, base.core.Renderer.BufferHeight, preserve: true);
		this.level = level;
		showTutorial = level == 0;
		tutorial = new Animation(0.3f);
		tutorial.Add("teach", "medusa_tut_", "111123456666789abccccdefghijk");
		tutorial.Play("teach");
	}

	public override void Load()
	{
		recognizer.Load();
		int num = 0;
		foreach (string key in recognizer.SignMetas.Keys)
		{
			List<Sprite> list = new List<Sprite>();
			int num2 = 0;
			while (true)
			{
				string name = string.Format("medusa_sign_{0}_{1}", key.ToLower(), num2.ToString("X").ToLower());
				if (!base.core.SpriteManager.HasSprite(name))
				{
					break;
				}
				list.Add(_(name));
				num2++;
			}
			Sprite item = list[list.Count - 3];
			int index = list.IndexOf(item);
			list.Insert(index, item);
			list.Insert(index, item);
			Animation animation = new Animation();
			animation.AddAndPlay("play", list);
			signCards.Add(new SignCard
			{
				Name = key.ToLower(),
				Animation = animation,
				Mirrored = false,
				Rotation = SignRotation.None
			});
			num++;
		}
		SelectRandomSingCards();
		foreach (SignCard card in selectedSignCards)
		{
			int[] pattern = new int[Math.Clamp(2 + level, 2, 6)];
			for (int i = 0; i < pattern.Length; i++) pattern[i] = SciHelper.GetRandom(0, 3);
			directionPatterns[card] = new DirectionPattern(pattern);
		}
		base.Load();
	}

	private void SelectRandomSingCards()
	{
		List<int> list = new List<int>();
		if (level == 0)
		{
			list.Add(0);
		}
		while (list.Count < 3)
		{
			int random = SciHelper.GetRandom(0, signCards.Count - 1);
			SignMeta signMeta = recognizer.SignMetas[signCards[random].Name];
			if (level == 0)
			{
				if (signMeta.Complexity != 0)
				{
					continue;
				}
			}
			else if (signMeta.Complexity == 0)
			{
				continue;
			}
			if (!list.Contains(random))
			{
				list.Add(random);
			}
		}
		int num = 50;
		int num2 = base.core.Renderer.ScreenWidth / (list.Count + 1);
		foreach (int item in list)
		{
			if (item != 0 || level != 0)
			{
				signCards[item].Mirrored = SciHelper.GetRandom(0, 1) == 1;
				signCards[item].Rotation = (SignRotation)SciHelper.GetRandom(0, 3);
			}
			SignCard signCard = signCards[item];
			signCard.TargetPosition = new Vector2(num2 * (selectedSignCards.Count + 1), num);
			signCard.Position = signCard.TargetPosition.Shift(0f, -100f);
			signCard.Active = true;
			signCard.RotT = Component._rnd(0, 180);
			selectedSignCards.Add(signCard);
		}
	}

	public override void Update()
	{
		tutorial.Update();
		if (delay > 0)
		{
			delay--;
		}
		else if (timer > 0 && selectedSignCards.FindAll((SignCard c) => c.Active).Count > 0)
		{
			timer--;
			if (timer == 0)
			{
				Finish(selectedSignCards.Count == 0);
			}
		}
		base.core.CurrentPlayState.Camera.ZoomBox.Set("dodge", 1.6f, inWorld: false, 0.05f, 0.03f);
		base.core.AudioManager.MusicVolumeBox.Set("dodge", 0.3f, inWorld: false);
		int num = 1;
		foreach (SignCard selectedSignCard in selectedSignCards)
		{
			selectedSignCard.Animation.Update();
			selectedSignCard.TargetPosition.X = num * (base.core.Renderer.ScreenWidth / (selectedSignCards.Count + 1));
			selectedSignCard.Position += (selectedSignCard.TargetPosition - selectedSignCard.Position) * ((Transition == TransType.Out) ? 0.02f : 0.1f);
			selectedSignCard.RotT++;
			if (!selectedSignCard.Active)
			{
				selectedSignCard.FadeT++;
			}
			num++;
		}
		selectedSignCards.RemoveAll((SignCard card) => !card.Active && card.FadeT >= card.FadeD);
		if (selectedSignCards.Count == 0)
		{
			Finish(success: true);
		}
		base.Update();
	}

	private void Finish(bool success)
	{
		if (Transition != TransType.None)
		{
			return;
		}
		timer = -1;
		medusa.Dodged = success;
		if (!success)
		{
			base.core.CurrentPlayState.Camera.ZoomBox.SetFixed("dodge", 1.6f, inWorld: false);
			foreach (SignCard selectedSignCard in selectedSignCards)
			{
				selectedSignCard.TargetPosition.Y += 50f;
			}
			base.TransDuration = 70;
		}
		SendMessage(new PlaySoundMessage(success ? SoundName.medusa_reassemble : SoundName.medusa_death), 15);
		medusa.DodgeAftermath();
		TransitionOut(CoreEvent.PopState);
	}

	private void ProcessTouch()
	{
		directionalMode = false;
		TouchLocation touchLocation = base.core.TouchState[0];
		if (touchLocation.State == TouchLocationState.Invalid)
		{
			return;
		}
		Vector2 position = touchLocation.Position;
		if (points.Count > 0)
		{
			Vector2 vector = points[points.Count - 1];
			Vector2 vector2 = vector;
			Vector2 vector3 = position - vector;
			for (float num = 0.5f; num <= 1f; num += 0.5f)
			{
				Vector2 item = vector2 + num * vector3;
				if (Math.Abs(item.X - vector2.X) + Math.Abs(item.Y - vector2.Y) > 5f)
				{
					points.Add(item);
				}
			}
		}
		else
		{
			points.Add(position);
			SendMessage(new PlaySoundMessage(SoundName.medusa_draw));
		}
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		HandleAccessibleInput();
		if (base.core.TouchState.Count != 1)
		{
			if (points.Count <= 0)
			{
				return;
			}
			SignCard recognized = TryRecognizeSelectedSignCards();
			if (recognized != null)
			{
				showTutorial = false;
			}
			ParticleEmitter particleEmitter = base.core.ParticleManager.AddEmitter(inWorld: false, Vector2.Zero).OnSpawn(delegate(Particle p)
			{
				if (recognized == null)
				{
					p.Aux.X = -1f;
					p.Aux.Y = 0.05f;
					p.Velocity = p.Position.Shift(0f, 30 + Component._rnd(0, 10));
				}
				else
				{
					p.Aux.X = 1f;
					p.Aux.Y = 0.2f;
					p.Velocity = recognized.Position + SciHelper.GetRandomVectorInCircle(20f);
				}
			}).OnUpdate(delegate(Particle p)
			{
				p.Position += (p.Velocity - p.Position) * p.Aux.Y;
				p.Dead = p.Age >= 40;
			})
				.OnDraw(delegate(Particle p)
				{
					int num = (int)(10f * (1f - (float)p.Age / 30f));
					num = (int)Component._M(num, 1f);
					float num2 = 1f - (float)p.Age / 40f;
					base.core.Renderer["fg", 10, false].DrawSpriteS(_("circle_" + num), p.Position, ((p.Aux.X < 0f) ? Color.Red : Color.LimeGreen) * num2, Vector2.One * 1.2f, 0f, SpriteFlip.None, SpriteOrigin.Center);
				})
				.DieWhenEmpty();
			foreach (Vector2 point in points)
			{
				if (SciHelper.ChanceRoll(0.8f))
				{
					particleEmitter.SpawnParticle(point);
				}
			}
			points.Clear();
			pointsDrawn = 0;
			currentR = 1f;
		}
		else
		{
			ProcessTouch();
			base.HandleInput();
		}
	}

	private void HandleAccessibleInput()
	{
		if (delay > 0)
		{
			return;
		}
		List<SignCard> activeCards = selectedSignCards.FindAll(card => card.Active);
		if (activeCards.Count == 0)
		{
			accessibleSignIndex = 0;
			return;
		}

		accessibleSignIndex = Math.Clamp(accessibleSignIndex, 0, activeCards.Count - 1);
		if (base.core.Input.WasPressed(GameAction.PreviousCategory))
		{
			accessibleSignIndex = (accessibleSignIndex + activeCards.Count - 1) % activeCards.Count;
			SendMessage(new PlaySoundMessage(SoundName.button_up, 0.5f));
		}
		else if (base.core.Input.WasPressed(GameAction.NextCategory))
		{
			accessibleSignIndex = (accessibleSignIndex + 1) % activeCards.Count;
			SendMessage(new PlaySoundMessage(SoundName.button_up, 0.5f));
		}

		if (base.core.Input.TryGetPressedDirection(out Vector2 direction))
		{
			directionalMode = true;
			showTutorial = false;
			SignCard card = activeCards[accessibleSignIndex];
			int pressed = direction.Y < 0 ? 0 : direction.X > 0 ? 1 : direction.Y > 0 ? 2 : 3;
			DirectionPattern pattern = directionPatterns[card];
			if (pattern.Submit(pressed))
			{
				if (pattern.Complete) { CompleteSign(card); accessibleSignIndex = 0; }
			}
			else
			{
				SendMessage(new PlaySoundMessage(SoundName.medusa_sign_fail_1));
			}
		}
	}

	private SignCard TryRecognizeSelectedSignCards()
	{
		SignCard signCard = null;
		foreach (SignCard selectedSignCard in selectedSignCards)
		{
			if (selectedSignCard.Active && recognizer.RecognizeAgainst(points, selectedSignCard.Name.ToLower(), selectedSignCard.Rotation, selectedSignCard.Mirrored))
			{
				signCard = selectedSignCard;
				break;
			}
		}
		if (signCard != null)
		{
			CompleteSign(signCard);
		}
		else
		{
			SendMessage(new PlaySoundMessage((drawn == 1) ? SoundName.medusa_sign_fail_1 : ((drawn == 2) ? SoundName.medusa_sign_fail_2 : SoundName.medusa_sign_fail_3)));
		}
		return signCard;
	}

	private void CompleteSign(SignCard signCard)
	{
		_inc(Stat.MedusaSignsDrawn);
		signCard.Active = false;
		drawn++;
		SendMessage(new PlaySoundMessage((drawn == 1) ? SoundName.medusa_sign_1 : ((drawn == 2) ? SoundName.medusa_sign_2 : SoundName.medusa_sign_3)));
		timer += 25;
	}

	public override void Draw()
	{
		float num = ((timer >= 0) ? Component._m(base.TicksInState - maxDelay, base.Trans) : ((float)base.Trans)) / (float)base.TransDuration;
		base.core.Renderer["fg"].FillScreen(Color.Black * num * 0.85f);
		if (directionalMode)
		{
			DrawDirectionPatterns();
			return;
		}
		if (level == 0 && showTutorial)
		{
			DrawTutorial();
		}
		currentR = base.core.Renderer.DrawPathForMedusaIntoTexture(renderTarget, points, pointsDrawn, Vector2.Zero, points.Count == 0, currentR);
		pointsDrawn += points.Count - pointsDrawn - 1;
		pointsDrawn = Math.Max(pointsDrawn, 0);
		DrawPath();
		DrawSignCards();
		base.Draw();
	}

	private void DrawTutorial()
	{
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 10, false].DrawSpriteS(tutorial.GetCurrentFrame(), base.core.Renderer.ScreenCenter.Shift(14.299999f, 25f), Color.White * 0.8f * num, Vector2.One * 1.3f, 0f, SpriteFlip.None, SpriteOrigin.Center);
	}

	public override void Unload()
	{
		renderTarget?.Dispose();
		base.Unload();
	}

	private void DrawDirectionPatterns()
	{
		var active = selectedSignCards.FindAll(c => c.Active);
		accessibleSignIndex = Math.Clamp(accessibleSignIndex, 0, Math.Max(0, active.Count - 1));
		float width = Math.Min(R.ScreenWidth - 32f, 270f);
		float left = (R.ScreenWidth - width) / 2f;
		string[] arrows = { "web_arrow_n", "web_arrow_e", "web_arrow_s", "web_arrow_w" };
		for (int row = 0; row < active.Count; row++)
		{
			SignCard card = active[row];
			float y = 52f + row * 38f;
			R["fg", 100, false].DrawWoodenPanel(new RectangleF(left, y, width, 32f));
			if (row == accessibleSignIndex)
				R["fg", 101, false].DrawRectangleS(new RectangleF(left, y, 3f, 32f), Color.Gold);
			DirectionPattern pattern = directionPatterns[card];
			for (int i = 0; i < pattern.Steps.Count; i++)
			{
				Color color = i < pattern.Progress ? Color.LimeGreen : Color.White;
				R["fg", 102, false].DrawSpriteS(_(arrows[pattern.Steps[i]]), new Vector2(R.ScreenCenter.X + (i - (pattern.Steps.Count - 1) / 2f) * 24f, y + 16f), color, Vector2.One, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
		}
		R["fg", 103, false].DrawRectangleS(new RectangleF(left, 40f, width * Math.Clamp((float)timer / maxTimer, 0f, 1f), 3f), Color.Gold);
		R["fg", 104, false].DrawTextS(__(Knighter.Localization.SId.PC_dodge_hint), new Vector2(R.ScreenCenter.X, 180f), TextProfile.OrangeBoldText.Alter(width: (int)width, height: 22, scale: 0.55f, boxAlignment: Alignment2D.Center));
		Knighter.UI.ContextPromptLegend.DrawButton(R, new Vector2(left, 205f), PromptAction.PreviousCategory);
		Knighter.UI.ContextPromptLegend.DrawButton(R, new Vector2(left + width - 20f, 205f), PromptAction.NextCategory);
	}

	private void DrawPath()
	{
		Texture2D texture2D = renderTarget;
		base.core.SpriteManager.AddOrReplaceTexture("path", texture2D);
		Sprite sprite = new Sprite
		{
			X = 0,
			Y = 0,
			Width = texture2D.Width,
			Height = texture2D.Height,
			SrcWidth = texture2D.Width,
			SrcHeight = texture2D.Height,
			TextureName = "path"
		};
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 10, false].DrawSpriteS(sprite, Vector2.Zero, Color.White * num, new Vector2(1f));
	}

	private void DrawSignCards()
	{
		float num = (float)base.Trans / (float)base.TransDuration;
		List<SignCard> activeCards = selectedSignCards.FindAll(card => card.Active);
		if (activeCards.Count > 0)
		{
			accessibleSignIndex = Math.Clamp(accessibleSignIndex, 0, activeCards.Count - 1);
			Vector2 selected = activeCards[accessibleSignIndex].Position;
			Color focusColor = Color.White * (0.55f + 0.2f * Component._sin(base.ticks * 0.12f));
			base.core.Renderer["fg", -1, false].DrawRectangleS(
				new RectangleF(selected.X - 18f, selected.Y - 18f, 36f, 36f),
				focusColor);
		}
		for (int i = 0; i < selectedSignCards.Count; i++)
		{
			SignCard signCard = selectedSignCards[i];
			float num2 = 0f;
			switch (signCard.Rotation)
			{
			case SignRotation.None:
				num2 = 0f;
				break;
			case SignRotation.Quarter:
				num2 = 4.712389f;
				break;
			case SignRotation.Half:
				num2 = (float)Math.PI;
				break;
			case SignRotation.ThreeQuarter:
				num2 = (float)Math.PI / 2f;
				break;
			}
			Color color = Color.Lerp(Color.Red, Color.White, (float)timer / (float)maxTimer);
			base.core.Renderer["fg"].DrawSpriteS(signCard.Animation.GetCurrentFrame(), signCard.Position, (Transition == TransType.Out) ? (Color.Red * num) : (signCard.Active ? color : (Color.LimeGreen * (1f - (float)signCard.FadeT / (float)signCard.FadeD))), flip: signCard.Mirrored ? SpriteFlip.Horizontal : SpriteFlip.None, rotation: num2 + Component._sin((float)signCard.RotT * 0.04f) * 0.1f, scale: new Vector2((Transition == TransType.Out) ? (1f + (1f - num)) : (signCard.Active ? 1f : (1.5f * (1f - (float)signCard.FadeT / (float)signCard.FadeD)))), origin: SpriteOrigin.Center);
		}
	}
}
