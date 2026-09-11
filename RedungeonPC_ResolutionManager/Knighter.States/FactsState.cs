using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class FactsState : State
{
	public enum FactsPage
	{
		Achievements,
		Deaths,
		Facts
	}

	private enum Button
	{
		Left,
		Right,
		Back,
		Share
	}

	private class DeathDataItem
	{
		public Stat Stat;

		public int Count;

		public int Share;

		public string Label;
	}

	private class FieldIcon
	{
		public string Name;

		public string Frames;

		public int TicksPerFrame;

		public float Sx;

		public float Sy;

		public bool Flip;

		public float Scale;

		public FieldIcon(string name, string frames, int ticksPerFrame = 4, float sx = 0f, float sy = 0f, bool flip = false, float scale = 1f)
		{
			Name = name;
			Frames = frames;
			TicksPerFrame = ticksPerFrame;
			Sx = sx;
			Sy = sy;
			Flip = flip;
			Scale = scale;
		}
	}

	private FactsPage currentPage;

	private readonly TouchMenu<Button> touchMenu;

	private Color scrollBg;

	private RectangleF scroll;

	private float pageH;

	private float pageY;

	private float lastPageY;

	private int swipeTouchID = -1;

	private Vector2 swipeTouchStart;

	private float targetSwipeLengthH;

	private float targetSwipeLengthV;

	private float swipeLengthH;

	private float swipeLengthV;

	private float swipePageY;

	private bool swipeIsVertical;

	private bool swipeTypeDetermined;

	private float afterSwipe;

	private const int scrollDurationH = 16;

	private int scrollAnimH;

	private int scrollDirH;

	private FactsPage nextPage;

	private float buttonPY;

	private float buttonHeight = 20f;

	private bool buttonPressed;

	private string pageTitle;

	private TextProfile txtDark;

	private TextProfile txtLight;

	private TextProfile txtDarkSmall;

	private DeathDataItem totalDeathsData;

	private List<DeathDataItem> deathCausesData;

	private List<DeathDataItem> deathExtraData;

	private string timePlaying;

	private string averageRun;

	private int unlockedAchievements;

	private int totalAchievements;

	private int l;

	private RectangleF s;

	private float py;

	private static List<SId> secretAchievementLines = new List<SId>
	{
		SId.FACTS_secret_achievement_1,
		SId.FACTS_secret_achievement_2,
		SId.FACTS_secret_achievement_3,
		SId.FACTS_secret_achievement_4
	};

	public FactsState(FactsPage page = FactsPage.Achievements)
	{
		base.TransDuration = 25;
		IsOverlay = true;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 11100);
		scrollAnimH = 16;
		scrollBg = default(Color).FromRgb(13343100);
		int width = _(SpriteName.facts_scroll_edge).Width;
		scroll = new RectangleF((float)(base.core.Renderer.ScreenWidth - width) * 0.5f, 28 + base.topSafeArea, width, base.core.Renderer.ScreenHeight - 28 - base.topSafeArea - 61);
		int num = base.core.Renderer.ScreenHeight - 40;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.Back, new RectangleF(10 + 3 * num2 / 2 + 1, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.MISC_btn_back));
		touchMenu.SetupButton(Button.Share, new RectangleF(10 + num2 / 2, num, num2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(Settings.ShareIcon), icon: true, iconIsPicture: false, blink: true);
		touchMenu.SetupButton(Button.Left, new RectangleF(0f, base.core.Renderer.ScreenCenter.Y - 25f, 22f, 35f), _(SpriteName.button_arrow_left), _(SpriteName.button_arrow_left_pressed), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.none);
		touchMenu.SetupButton(Button.Right, new RectangleF(base.core.Renderer.ScreenWidth - 23, base.core.Renderer.ScreenCenter.Y - 25f, 22f, 35f), _(SpriteName.button_arrow_left), _(SpriteName.button_arrow_left_pressed), null, stretch: false, SpriteFlip.Horizontal, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.none);
		touchMenu[Button.Share].Hidden = true;
		touchMenu[Button.Back].Rectangle = new RectangleF(core.Renderer.ScreenCenter.X - 70f, num, 140f, 30f);
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		SetPage(page);
		InitData();
		InitMarkup();
		if (!base.core.ProfileData.DiscoveredFactsScreen)
		{
			base.core.ProfileData.DiscoveredFactsScreen = true;
			base.core.ProfileData.SaveIntoStorage();
		}
	}


	// Shares the original artwork, data and animated FieldIcon sequences with Settings.
	public bool ScrollEmbedded(float amount)
	{
		float next = Math.Clamp(pageY + amount, Math.Min(0, scroll.Height - pageH), 0);
		bool changed = Math.Abs(next - pageY) > 0.01f;
		pageY = next;
		return changed;
	}
	public void DrawEmbedded(RectangleF bounds, int depth)
	{
		l = depth + 4;
		float paperWidth = Math.Min(bounds.Width - 28, 450f);
		s = new RectangleF(bounds.Center.X - paperWidth / 2, bounds.Y + 15, paperWidth, bounds.Height - 28);
		scroll = s.Clone();
		txtDark.Width = (int)s.Width; txtLight.Width = (int)s.Width; txtDarkSmall.Width = (int)s.Width - 16;
		R["fg", depth, false].DrawRectangleS(s, scrollBg);
		var edge = _(SpriteName.facts_scroll_edge);
		R["fg", depth + 1, false].DrawSpriteS(edge, s.TopLeft, scale: new Vector2(s.Width / edge.Width, 1), flip: SpriteFlip.Vertical);
		R["fg", depth + 1, false].DrawSpriteS(edge, s.BottomLeft, scale: new Vector2(s.Width / edge.Width, 1), origin: SpriteOrigin.BottomLeft);
		R.SetUiClip(s);
		try { DrawContent(); } finally { R.SetUiClip(null); }
		var top = _(SpriteName.facts_scroll_top); var bottom = _(SpriteName.facts_scroll_bottom);
		DrawWide(top, s.TopLeft + new Vector2(-12,-14), s.Width + 24, Color.White, depth + 8, 26);
		DrawWide(bottom, s.BottomLeft + new Vector2(-12,-3), s.Width + 24, Color.White, depth + 8, 26);
		float trackHeight = s.Height - 8, thumb = Math.Max(12, trackHeight * Math.Min(1, s.Height / Math.Max(1, pageH)));
		float travel = Math.Max(1, pageH - s.Height);
		R["fg", depth + 9, false].DrawRectangleS(new RectangleF(s.Right + 7, s.Y + 4, 3, trackHeight), new Color(101,75,59));
		R["fg", depth + 10, false].DrawRectangleS(new RectangleF(s.Right + 7, s.Y + 4 + (-pageY / travel) * (trackHeight - thumb), 3, thumb), new Color(245,179,71));
	}

	private void DrawWide(Sprite sprite, Vector2 position, float width, Color color, int depth, int cap = 3)
	{
		cap = Math.Min(cap, sprite.Width / 3);
		var left = sprite.Reduce(0, 0, sprite.Width - cap, 0);
		var right = sprite.Reduce(sprite.Width - cap, 0, 0, 0);
		var middle = sprite.Reduce(cap, 0, cap, 0);
		R["fg", depth, false].DrawSpriteS(left, position, color);
		R["fg", depth, false].DrawSpriteS(middle, position + new Vector2(cap, 0), color, new Vector2((width - cap * 2) / middle.Width, 1));
		R["fg", depth, false].DrawSpriteS(right, position + new Vector2(width - cap, 0), color);
	}

	private Vector2 FieldScale => new Vector2((s.Width - 12) / 117f, 1);
	private void SetPage(FactsPage page)
	{
		currentPage = page;
		pageH = 10000f;
		switch (page)
		{
		case FactsPage.Achievements:
			pageTitle = __(SId.FACTS_achievements);
			break;
		case FactsPage.Deaths:
			pageTitle = __(SId.FACTS_deaths);
			break;
		case FactsPage.Facts:
			pageTitle = __(SId.FACTS_facts);
			break;
		}
		pageY = 0f;
		swipeLengthV = 0f;
	}

	public override void Update()
	{
		IsOpaque = Transition == TransType.None;
		if (scrollAnimH < 16)
		{
			scrollAnimH++;
			if (scrollAnimH == 8)
			{
				SetPage(nextPage);
			}
		}
		afterSwipe *= 0.9f;
		if (!swipeTypeDetermined)
		{
			pageY += afterSwipe;
		}
		swipeLengthH += (targetSwipeLengthH - swipeLengthH) * 0.3f;
		swipeLengthV += (targetSwipeLengthV - swipeLengthV) * 0.9f;
		if (!swipeTypeDetermined)
		{
			if (pageY > 0f)
			{
				pageY += (0f - pageY) * 0.2f;
			}
			if (pageY < scroll.Height - pageH)
			{
				pageY += (scroll.Height - pageH - pageY) * 0.2f;
			}
		}
		base.Update();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.Share].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, 80.0, -80.0, base.TransDuration));
		touchMenu[Button.Back].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, 80.0, -80.0, base.TransDuration));
		touchMenu[Button.Left].Rectangle.Shift((float)Tween.BackEaseOut(TransD(2, 2), -50.0, 50.0, base.TransDuration - 2 - 2));
		touchMenu[Button.Right].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		if (base.core.Input.WasPressed(GameAction.MoveLeft)
			|| base.core.Input.WasPressed(GameAction.PreviousCategory))
		{
			OnButtonRelease(Button.Left);
		}
		else if (base.core.Input.WasPressed(GameAction.MoveRight)
			|| base.core.Input.WasPressed(GameAction.NextCategory))
		{
			OnButtonRelease(Button.Right);
		}
		const float keyboardScrollSpeed = 3f;
		if (base.core.Input.IsDown(GameAction.MoveUp))
		{
			pageY = Math.Min(0f, pageY + keyboardScrollSpeed);
			afterSwipe = 0f;
		}
		if (base.core.Input.IsDown(GameAction.MoveDown))
		{
			pageY = Math.Max(Math.Min(0f, scroll.Height - pageH), pageY - keyboardScrollSpeed);
			afterSwipe = 0f;
		}
		touchMenu.HandleInput();
		bool flag = false;
		if (true)
		{
			foreach (TouchLocation item in base.core.TouchState)
			{
				switch (item.State)
				{
				case TouchLocationState.Moved:
				case TouchLocationState.Pressed:
					if (swipeTouchID < 0 && scroll.Contains(item.Position))
					{
						swipeTouchID = item.Id;
						swipeTouchStart = item.Position;
						swipePageY = pageY;
						lastPageY = pageY;
						afterSwipe = 0f;
						flag = true;
						if (currentPage == FactsPage.Achievements)
						{
							float num = item.Position.Y - s.Top;
							if (num >= buttonPY && num <= buttonPY + buttonHeight)
							{
								buttonPressed = true;
								SendMessage(new PlaySoundMessage(SoundName.button_down));
							}
						}
					}
					else
					{
						if (item.Id != swipeTouchID)
						{
							break;
						}
						Vector2 vector = item.Position - swipeTouchStart;
						if (!swipeTypeDetermined)
						{
							swipeIsVertical = Math.Abs(vector.X) < Math.Abs(vector.Y);
						}
						targetSwipeLengthH = (swipeIsVertical ? 0f : vector.X);
						targetSwipeLengthV = (swipeIsVertical ? vector.Y : 0f);
						int num2 = (swipeIsVertical ? 5 : 20);
						if (vector.Length() < (float)num2)
						{
							if (!swipeIsVertical)
							{
								targetSwipeLengthH = 0f;
							}
							if (swipeIsVertical)
							{
								targetSwipeLengthV = 0f;
							}
						}
						else
						{
							swipeTypeDetermined = true;
							buttonPressed = false;
							if (swipeIsVertical)
							{
								if (swipePageY + vector.Y > 0f)
								{
									vector.Y = 0f - swipePageY + (vector.Y + swipePageY) * 0.5f;
								}
								if (swipePageY + vector.Y < 0f - (pageH - scroll.Height))
								{
									vector.Y = 0f - (pageH - scroll.Height) - swipePageY + (vector.Y - (0f - (pageH - scroll.Height) - swipePageY)) * 0.5f;
								}
								pageY = swipePageY + vector.Y;
								float value = pageY - lastPageY;
								if (Math.Abs(value) > Math.Abs(afterSwipe))
								{
									afterSwipe = value;
								}
								lastPageY = pageY;
							}
						}
						flag = true;
					}
					break;
				case TouchLocationState.Released:
					if (item.Id != swipeTouchID)
					{
						break;
					}
					swipeTouchID = -1;
					if (buttonPressed)
					{
						SendMessage(new PlaySoundMessage(SoundName.button_up));
						OpenSystemAchievements();
					}
					buttonPressed = false;
					if (swipeTypeDetermined && !swipeIsVertical && Math.Abs(targetSwipeLengthH) > 40f)
					{
						if (targetSwipeLengthH < 0f)
						{
							OnButtonRelease(Button.Right);
						}
						else
						{
							OnButtonRelease(Button.Left);
						}
					}
					break;
				}
			}
		}
		if (!flag)
		{
			swipeTouchID = -1;
			targetSwipeLengthH = 0f;
			targetSwipeLengthV = 0f;
			swipeTypeDetermined = false;
		}
		base.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		int num = (int)currentPage;
		int length = Enum.GetValues(typeof(FactsPage)).Length;
		switch (button)
		{
		case Button.Back:
			OnBackButtonPressed();
			break;
		case Button.Left:
			if (scrollAnimH == 16)
			{
				num = ((num == 0) ? (length - 1) : (num - 1));
				nextPage = (FactsPage)num;
				scrollAnimH = 0;
				scrollDirH = 1;
				SendMessage(new PlaySoundMessage(SoundName.swoosh_1, 0.7f));
			}
			break;
		case Button.Right:
			if (scrollAnimH == 16)
			{
				num = ((num != length - 1) ? (num + 1) : 0);
				nextPage = (FactsPage)num;
				scrollAnimH = 0;
				scrollDirH = -1;
				SendMessage(new PlaySoundMessage(SoundName.swoosh_1, 0.7f));
			}
			break;
		}
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		TransitionOut(CoreEvent.HideGetCoins);
		base.OnBackButtonPressed();
	}

	public void OpenSystemAchievements()
	{
		// Achievements are local; there is no platform-service destination.
	}

	public override void Draw()
	{
		l = 11000;
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", l - 10, false].FillScreen(Color.Black * Component._m(num * 2.5f, 1f));
		s = scroll.Clone();
		if (base.core.TakingScreenshot)
		{
			s.Height = (float)base.core.Renderer.ScreenHeight - s.Top * 2f;
		}
		float num2 = 0f;
		if (scrollAnimH < 15)
		{
			float num3 = 0f;
			num3 = (float)Tween.SineEaseOut(scrollAnimH % 8, 0.0, 1.0, 8.0);
			int num4 = base.core.Renderer.ScreenWidth - 20;
			num2 = (float)(scrollDirH * num4) * num3 + (float)((scrollAnimH >= 8) ? (-scrollDirH * num4) : 0);
		}
		num2 += swipeLengthH * 0.5f;
		s.Shift(num2, (float)(-base.core.Renderer.ScreenHeight) * 0.7f * (float)Tween.BackEaseOut(Component._m(base.Trans, (float)base.TransDuration / 1.5f), 1.0, -1.0, (float)base.TransDuration / 1.5f) + Math.Abs(num2) * 0.1f);
		s.Height = 45f + (s.Height - 45f) * (float)Tween.BackEaseOut(Component._M(base.Trans - 5, 0f), 0.0, 1.0, base.TransDuration - 5);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.shop_wall), base.core.Renderer.ScreenCenter, Color.White * num, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", l, false].DrawRectangleS(s, scrollBg);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_scroll_edge), s.TopLeft, null, null, 0f, SpriteFlip.Vertical);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_scroll_edge), s.BottomLeft, null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
		DrawContent();
		base.core.Renderer["fg", l + 1, false].DrawRectangleS(new RectangleF(s.Left - 5f, -5f, s.Width + 10f, s.Top), Color.Black);
		base.core.Renderer["fg", l + 1, false].DrawRectangleS(new RectangleF(s.Left - 5f, s.Bottom + 5f, s.Width + 10f, 50f), Color.Black);
		base.core.Renderer["fg", l + 5, false].DrawSpriteS(_(SpriteName.facts_scroll_top), s.TopLeft.Shift(-14f, -17f));
		base.core.Renderer["fg", l + 5, false].DrawSpriteS(_(SpriteName.facts_scroll_bottom), s.BottomLeft.Shift(-14f, -3f));
		if (!base.core.TakingScreenshot)
		{
			int length = Enum.GetValues(typeof(FactsPage)).Length;
			for (int i = 0; i < length; i++)
			{
				base.core.Renderer["fg", l + 1, false].DrawSpriteS(_(SpriteName.circle_4), new Vector2((float)base.core.Renderer.ScreenWidth * 0.5f - 10f * (float)length / 2f + (float)(i * 10) + 5f, 6.5f + (float)base.topSafeArea), default(Color).FromRgb((i == (int)currentPage) ? 11827562 : 6239030) * num, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			touchMenu.Draw();
		}
		base.Draw();
	}

	private void InitMarkup()
	{
		txtDark = new TextProfile
		{
			Decoration = TextDecoration.None,
			Color = default(Color).FromRgb(7883070),
			Scale = 1f,
			Width = 129,
			BoxAlignment = Alignment2D.Left,
			TextAlignment = Alignment2D.Left
		};
		txtLight = txtDark.Alter(default(Color).FromRgb(9462096));
		txtDarkSmall = txtDark.Alter(null, null, null, null, null, null, null, null, 0.6f);
	}

	private string TicksToTimespan(int tt)
	{
		if (tt < 60)
		{
			return "0" + __(SId.MISC_seconds);
		}
		int num = tt / 5184000;
		int num2 = tt / 216000 - num * 24;
		int num3 = tt / 3600 - num2 * 60 - num * 24 * 60;
		int num4 = tt / 60 - num3 * 60 - num2 * 60 * 60 - num * 24 * 60 * 60;
		string text = "";
		if (num > 0)
		{
			text = text + num + __(SId.MISC_days) + " ";
		}
		if (num2 > 0)
		{
			text = text + num2 + __(SId.MISC_hours) + " ";
		}
		if (num3 > 0)
		{
			text = text + num3 + __(SId.MISC_minutes) + " ";
		}
		if (num4 > 0 && num == 0)
		{
			text = text + num4 + __(SId.MISC_seconds);
		}
		return text.Trim();
	}

	private void InitData()
	{
		deathCausesData = new List<DeathDataItem>();
		foreach (Stat value in Enum.GetValues(typeof(Stat)))
		{
			if (value.ToString().StartsWith("KilledBy", StringComparison.InvariantCulture))
			{
				deathCausesData.Add(new DeathDataItem
				{
					Stat = value,
					Count = _stat(value)
				});
			}
		}
		int num = 0;
		foreach (DeathDataItem deathCausesDatum in deathCausesData)
		{
			num += deathCausesDatum.Count;
		}
		foreach (DeathDataItem deathCausesDatum2 in deathCausesData)
		{
			deathCausesDatum2.Share = ((num != 0) ? (100 * deathCausesDatum2.Count / num) : 0);
		}
		deathCausesData = deathCausesData.OrderBy((DeathDataItem d) => -d.Count).ToList();
		totalDeathsData = new DeathDataItem
		{
			Stat = Stat.Attempts,
			Count = num,
			Share = 100
		};
		deathExtraData = new List<DeathDataItem>();
		deathExtraData.Add(new DeathDataItem
		{
			Count = _stat(Stat.DiedInSpiderWeb),
			Label = "while in spider web"
		});
		deathExtraData.Add(new DeathDataItem
		{
			Count = _stat(Stat.DiedInMist),
			Label = "while blinded"
		});
		deathExtraData.Add(new DeathDataItem
		{
			Count = _stat(Stat.DiedFrozen),
			Label = "while frozen"
		});
		deathExtraData.Add(new DeathDataItem
		{
			Count = _stat(Stat.DiedPoisoned),
			Label = "while poisoned"
		});
		deathExtraData.Add(new DeathDataItem
		{
			Count = _stat(Stat.DiedConfused),
			Label = "while confused"
		});
		deathExtraData = deathExtraData.OrderBy((DeathDataItem d) => -d.Count).ToList();
		timePlaying = TicksToTimespan(_stat(Stat.TicksInGame));
		averageRun = TicksToTimespan(_stat(Stat.TicksInGame) / (int)Component._M(_stat(Stat.Attempts), 1f));
		unlockedAchievements = 0;
		totalAchievements = 0;
		foreach (Achievement value2 in Enum.GetValues(typeof(Achievement)))
		{
			totalAchievements++;
			if ((Achievements.IsIncremental(value2) && base.core.Achievments.GetProgress(value2) >= Achievements.Targets[value2]) || (!Achievements.IsIncremental(value2) && base.core.Achievments.GetProgress(value2) > 0))
			{
				unlockedAchievements++;
			}
		}
	}

	private void _space(int height)
	{
		py += height;
	}

	private void _heading(string text, float scale = 1f)
	{
		int num = 13;
		if (py > (float)(-num) && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawTextS(text, new Vector2(s.Left, s.Top + py), txtDark.Alter(null, null, null, null, null, null, Alignment2D.Center, null, scale));
		}
		py += num;
	}

	private void _image(SpriteName image)
	{
		Sprite sprite = _(image);
		int height = sprite.Height;
		if (py > (float)(-height) && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawSpriteS(sprite, new Vector2(s.Left + s.Width / 2f, s.Top + py), null, null, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		}
		py += height + 3;
	}

	private void _button()
	{
		buttonPY = py;
		int num = 20;
		if (py > (float)(-num) && py < s.Height)
		{
			if (buttonPressed)
			{
				base.core.Renderer["fg", l, false].DrawRectangleS(new RectangleF(s.Left + 6.5f, s.Top + py + 1f, 116f, 19f), default(Color).FromRgb(7883070) * 0.2f);
			}
			DrawWide(_(SpriteName.facts_line_frame), new Vector2(s.Left + 6, s.Top + py), s.Width - 12, Color.White, l);
			base.core.Renderer["fg", l, false].DrawTextS(__(SId.FACTS_view_in_google_play), new Vector2(s.Left + s.Width / 2f, s.Top + py), txtLight.Alter(null, null, null, textAlignment: Alignment2D.Middle, boxAlignment: Alignment2D.Center, height: 20, width: (int)s.Width, font: null, scale: 0.8f));
		}
		py += num + 4;
	}

	private void _percentField(int count, int share, FieldIcon icon)
	{
		int num = 20;
		if (py > (float)(-num) && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawRectangleS(new RectangleF(s.Left + 6.5f, s.Top + py + 1f, (s.Width - 13) * share * 0.01f, 19f), default(Color).FromRgb(7883070) * 0.2f);
			DrawWide(_(SpriteName.facts_line_frame), new Vector2(s.Left + 6, s.Top + py), s.Width - 12, Color.White, l);
			base.core.Renderer["fg", l, false].DrawTextS(count.ToString(), new Vector2(s.Left + 31f, s.Top + py + 4f), txtDark);
			base.core.Renderer["fg", l, false].DrawTextS(share + "%", new Vector2(s.Left + s.Width * 0.7f, s.Top + py + 4f), txtLight);
			__icon(icon);
		}
		py += num + 3;
	}

	private void _label(string label)
	{
		int num = 10;
		if (py > (float)(-num) && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawTextS(label.ToUpper(), new Vector2(s.Left + 8f, s.Top + py + 4f), txtDarkSmall);
		}
		py += num + 2;
	}

	private void __icon(FieldIcon i)
	{
		if (i != null)
		{
			base.core.Renderer["fg", l + 1, false].DrawSpriteS(_((i.Frames == "") ? i.Name : (i.Name + i.Frames[base.ticks / i.TicksPerFrame % i.Frames.Length])), new Vector2(s.Left + 10f + i.Sx, s.Top + py + 2f + i.Sy), null, flip: i.Flip ? SpriteFlip.Horizontal : SpriteFlip.None, scale: Vector2.One * i.Scale);
		}
	}

	private void _field(string val, FieldIcon icon)
	{
		int num = 20;
		if (py > (float)(-num) && py < s.Height)
		{
			DrawWide(_(SpriteName.facts_line_frame_filled), new Vector2(s.Left + 6, s.Top + py), s.Width - 12, Color.White, l);
			base.core.Renderer["fg", l, false].DrawTextS(val, new Vector2(s.Left + 31f, s.Top + py + 4f), txtDark);
			__icon(icon);
		}
		py += num + 3;
	}

	private void _field(int val, FieldIcon icon)
	{
		_field(val.ToString(), icon);
	}

	private void _labeledField(string label, string val, FieldIcon icon)
	{
		_label(label);
		_field(val, icon);
	}

	private void _labeledField(string label, int val, FieldIcon icon)
	{
		_labeledField(label, val.ToString(), icon);
	}

	private void _achievement(Achievement a)
	{
		int num = 37;
		if (!(py > (float)(-num)) || !(py < s.Height))
		{
			py += num + 3;
			return;
		}
		AchievementMeta achievementMeta = Achievements.Metas[a];
		string text = "";
		int num2 = 0;
		int num3 = 1;
		bool flag = Achievements.IsIncremental(a);
		if (flag)
		{
			num3 = Achievements.Targets[a];
			num2 = base.core.Achievments.GetProgress(a);
			switch (a)
			{
			case Achievement.UnlockAllOfThem:
				num3++;
				num2++;
				text += string.Format(__(SId.FACTS_achievement_progress), num2, num3);
				break;
			case Achievement.PlayForOneHour:
				text += string.Format(__(SId.FACTS_achievement_progress), TicksToTimespan(num2), "1" + __(SId.MISC_hours));
				break;
			case Achievement.MageSpendThreeMinutesInSloMo:
				text += string.Format(__(SId.FACTS_achievement_progress), TicksToTimespan(num2), "3" + __(SId.MISC_minutes));
				break;
			default:
				text += string.Format(__(SId.FACTS_achievement_progress), num2, num3);
				break;
			}
		}
		if (achievementMeta.Hidden)
		{
			text = __(secretAchievementLines[(base.ticks + (int)a * 300) % (secretAchievementLines.Count * 140) / 140]);
			text = text.ToUpper();
		}
		if (Achievements.IsIncremental(a) ? (num2 >= num3) : base.core.ProfileData.IsAchievementUnlocked(a))
		{
			base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_achievement_bg), new Vector2(s.Left + 7f, s.Top + py - 0.5f), achievementMeta.ColorBG, FieldScale);
			base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_achievement_glow), new Vector2(s.Left + 6f, s.Top + py - 1f), achievementMeta.ColorFG, FieldScale);
			DrawWide(_(SpriteName.facts_achievement_frame), new Vector2(s.Left + 6, s.Top + py - 1), s.Width - 12, achievementMeta.ColorFrame, l);
			Vector2 vector = new Vector2(achievementMeta.IconDx + Component._sin((float)(base.ticks + 50 * (int)a) * 0.035f), achievementMeta.IconDy + Component._cos((float)(base.ticks + 340 * (int)a) * 0.035f));
			base.core.Renderer["fg", l, false].DrawSpriteS(_(achievementMeta.Icon), new Vector2(s.Left + 6f + 20f, s.Top + py + 20f) + vector * 0.2f, null, rotation: Component._sin((float)(base.ticks + 50 * (int)a) * 0.05f) * 0.1f, scale: Vector2.One * (1.1f + Component._cos((float)(base.ticks + 50 * (int)a) * 0.02f) * 0.1f), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
			Renderer renderer = base.core.Renderer["fg", l, false];
			string text2 = __(achievementMeta.Name).ToUpper();
			Vector2 position = new Vector2(s.Right - 5f, s.Top + py + 2f);
			TextProfile textProfile = txtDark;
			Color? color = achievementMeta.ColorFG;
			float? scale = ((a == Achievement.BragFireHundredTimes) ? 0.75f : 0.9f);
			renderer.DrawTextS(text2, position, textProfile.Alter(color, null, null, (int)s.Width - 52, 37, Alignment2D.Right, Alignment2D.Right, null, scale));
			base.core.Renderer["fg", l, false].DrawTextS(__(achievementMeta.Debriefing).ToUpper(), new Vector2(s.Right - 8f, s.Top + py + 16f), txtDarkSmall.Alter(achievementMeta.ColorFG * 0.5f, null, null, scale: 0.4f, width: (int)s.Width - 56, height: 16, textAlignment: Alignment2D.RightBottom, boxAlignment: Alignment2D.Right));
		}
		else
		{
			if (flag && !achievementMeta.Hidden)
			{
				int width = _(SpriteName.facts_achievement_bg).Width;
				base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_achievement_bg).Reduce(0, 0, (int)((float)width * (1f - (float)num2 / (float)num3)), 0), new Vector2(s.Left + 7f, s.Top + py), default(Color).FromRgb(7883070) * 0.2f, FieldScale);
			}
			DrawWide(_(SpriteName.facts_achievement_frame), new Vector2(s.Left + 6, s.Top + py - 1), s.Width - 12, default(Color).FromRgb(7883070), l);
			if (!achievementMeta.Hidden)
			{
				base.core.Renderer["fg", l, false].DrawTextS(__(achievementMeta.Name).ToUpper(), new Vector2(s.Left + 10f, s.Top + py + 2f), txtDarkSmall.Alter(txtDark.Color, null, null, null, null, null, null, null, 0.7f));
				base.core.Renderer["fg", l, false].DrawTextS(__(achievementMeta.Briefing).ToUpper(), new Vector2(s.Left + 10f, s.Top + py + 10.5f), txtDarkSmall.Alter(txtLight.Color, null, null, scale: 0.5f, width: (int)s.Width - 22, height: 16, boxAlignment: null, textAlignment: Alignment2D.Left));
				base.core.Renderer["fg", l, false].DrawTextS(text, new Vector2(s.Left + 10f, s.Top + py + 23f), txtDark.Alter(txtLight.Color, null, null, null, null, null, null, null, 0.9f));
			}
			else
			{
				base.core.Renderer["fg", l, false].DrawTextS(text, new Vector2(s.Center.X - 3f, s.Top + py - 2f), txtDark.Alter(default(Color).FromRgb(7883070) * 0.2f, null, null, scale: 0.9f, width: (int)(s.Width * 0.8f), height: 37, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Middle));
			}
		}
		py += num + 3;
	}

	private FieldIcon __iconFromStat(Stat stat)
	{
		FieldIcon result = null;
		switch (stat)
		{
		case Stat.KilledByGravity:
			result = new FieldIcon("facts_field_fall_", "1111123456789", 3, -1f, -1f);
			break;
		case Stat.KilledByCrossbow:
			result = new FieldIcon("crossbow_", "11112333345", 4, -1f, -0.5f, flip: true);
			break;
		case Stat.KilledBySlime:
			result = new FieldIcon("slime_", "1232", 8, -2f, -1f);
			break;
		case Stat.KilledByFlame:
			result = new FieldIcon("grill_burn_", "1234", 6, 0f, -2f);
			break;
		case Stat.KilledByZapper:
			result = new FieldIcon("zapball_", "123456", 4, -3f, -1.5f);
			break;
		case Stat.KilledBySaw:
			result = new FieldIcon("saw_", "1234", 4, -2f, 3f, flip: false, 0.9f);
			break;
		case Stat.KilledBySpikes:
			result = new FieldIcon("spikes_", "111112345555432", 4, 0f, -4f);
			break;
		case Stat.KilledByAxe:
			result = new FieldIcon("facts_field_axe_", "111123444444445678", 4, -4f, 1.5f, flip: false, 0.9f);
			break;
		case Stat.KilledByBat:
			result = new FieldIcon("bat_", "1234", 6, -5f, 2f, flip: true);
			break;
		case Stat.KilledByPiston:
			result = new FieldIcon("facts_field_piston_", "11112345666632", 4, -4f, -8f, flip: false, 0.9f);
			break;
		case Stat.KilledByFollower:
			result = new FieldIcon("facts_field_follower_", "1234", 6, 1f, -3f);
			break;
		case Stat.KilledByDarkness:
			result = new FieldIcon("facts_field_grue_", "123456", 6, -7f);
			break;
		case Stat.KilledByRotoblade:
			result = new FieldIcon("facts_field_blade_", "1111123444445", 3, 0f, -4f, flip: false, 0.9f);
			break;
		case Stat.KilledByDeadBattery:
			result = new FieldIcon("facts_field_battery_", "12", 30, -5f, -0.5f);
			break;
		case Stat.KilledBySerpent:
			result = new FieldIcon("facts_field_serpent_", "12345678", 4, -4f, 1f);
			break;
		}
		return result;
	}

	private void DrawContent()
	{
		py = pageY;
		_space(13);
		_image((currentPage == FactsPage.Achievements) ? SpriteName.facts_achievements_title : ((currentPage == FactsPage.Deaths) ? SpriteName.facts_deaths_title : SpriteName.facts_facts_title));
		_heading(pageTitle);
		_space(5);
		switch (currentPage)
		{
		case FactsPage.Deaths:
			_space(5);
			_heading(__(SId.FACTS_h_total), 0.8f);
			_space(-2);
			_percentField(totalDeathsData.Count, totalDeathsData.Share, new FieldIcon("facts_field_death", "", 0, 1f, 3.5f + Component._sin((float)base.ticks * 0.05f)));
			_space(5);
			_heading(__(SId.FACTS_h_causes), 0.8f);
			_space(-2);
			foreach (DeathDataItem deathCausesDatum in deathCausesData)
			{
				if (deathCausesDatum.Stat != Stat.KilledByDeadBattery || base.core.ProfileData.Characters[Character.PanicBot].Unlocked)
				{
					_percentField(deathCausesDatum.Count, deathCausesDatum.Share, __iconFromStat(deathCausesDatum.Stat));
				}
			}
			break;
		case FactsPage.Facts:
			_space(5);
			_heading(__(SId.FACTS_h_general), 0.8f);
			_space(-2);
			_labeledField(__(SId.FACTS_f_time_in_game), timePlaying, new FieldIcon("facts_field_time", "", 4, 0f, -1f));
			_labeledField(__(SId.FACTS_f_runs), _stat(Stat.Attempts), new FieldIcon("facts_field_run_1", "", 10, 0f, -2f));
			_labeledField(__(SId.FACTS_f_avg_run_duration), averageRun, new FieldIcon("facts_field_run_2", "", 10, 0f, -2f));
			_labeledField(__(SId.FACTS_f_distance_walked), _stat(Stat.MetersWalked) + __(SId.MISC_meters), new FieldIcon("facts_field_steps_", "12345678", 5, 1f, -1.5f));
			_labeledField(__(SId.FACTS_f_distance_slid), _stat(Stat.MetersSlided) + __(SId.MISC_meters), new FieldIcon("facts_field_slide_", "1213", 5, -1f));
			_labeledField(__(SId.FACTS_f_jumps), _stat(Stat.JumpersUsed), new FieldIcon("facts_field_pusher_", "1111234", 4, 0.5f, 2f));
			_labeledField(__(SId.FACTS_f_webs), _stat(Stat.WebsBroken), new FieldIcon("facts_field_web", "", 10, -1f));
			_labeledField(__(SId.FACTS_f_pots), _stat(Stat.PotsBroken), new FieldIcon("pot_hit_", "0000e111120000n111120000w111120000s11112", 5, -2f, -1f));
			_labeledField(__(SId.FACTS_f_chests), _stat(Stat.ChestsLooted), new FieldIcon("chest_", "11123444432", 4, -6f, -7.5f));
			_labeledField(__(SId.FACTS_f_coins), _stat(Stat.CoinsCollected), new FieldIcon("coin_gold_", "123456", 4, 3f, 3f));
			_space(8);
			_heading(__(SId.FACTS_h_monsters_killed), 0.8f);
			_space(-2);
			_field(_stat(Stat.SlimesKilled), new FieldIcon("slime_", "1232", 8, -2f, -1f));
			_field(_stat(Stat.BatsKilled), new FieldIcon("bat_", "1234", 6, -5f, 2f, flip: true));
			_field(_stat(Stat.WispsKilled), new FieldIcon("facts_field_wisp_", "123456", 5, -2.5f, -1f));
			_field(_stat(Stat.FollowersKilled), new FieldIcon("facts_field_follower_", "1234", 6, 1f, -3f));
			_field(_stat(Stat.SerpentsKilled), new FieldIcon("facts_field_serpent_", "12345678", 4, -4f, 1f));
			_space(8);
			_heading(__(SId.FACTS_h_traps_destroyed), 0.8f);
			_space(-2);
			_field(_stat(Stat.SpikesBroken), new FieldIcon("spikes_", "111112345555432", 4, 0f, -4f));
			_field(_stat(Stat.SawsBroken), new FieldIcon("saw_", "1234", 4, -2f, 3f, flip: false, 0.9f));
			_field(_stat(Stat.CrossbowsBroken), new FieldIcon("crossbow_", "11112333345", 4, -1f, -0.5f, flip: true));
			_field(_stat(Stat.RotobladesBroken), new FieldIcon("facts_field_blade_", "1111123444445", 3, 0f, -4f, flip: false, 0.9f));
			_field(_stat(Stat.PistonsBroken), new FieldIcon("facts_field_piston_", "11112345666632", 4, -4f, -8f, flip: false, 0.9f));
			_field(_stat(Stat.ZappersBroken), new FieldIcon("facts_field_zapper_", "111111111111111423423423423423", 2, -2f, -5f));
			break;
		case FactsPage.Achievements:
			_space(-5);
			_heading(string.Format(__(SId.FACTS_achievement_progress), unlockedAchievements, totalAchievements), 0.8f);
			_space(5);
			foreach (Achievement value in Enum.GetValues(typeof(Achievement)))
			{
				_achievement(value);
			}
			break;
		}
		_space(10);
		pageH = py - pageY;
	}
}
