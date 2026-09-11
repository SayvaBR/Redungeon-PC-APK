using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class CreditsState : State
{
	private enum Button
	{
		Back
	}

	private readonly TouchMenu<Button> touchMenu;

	private Color scrollBg;

	private RectangleF scroll;

	private float optionsScrollTop;

	private float pageH;

	private float pageY;

	private float lastPageY;

	private int swipeTouchID = -1;

	private Vector2 swipeTouchStart;

	private float targetSwipeLengthV;

	private float swipeLengthV;

	private float swipePageY;

	private bool swipeIsVertical;

	private bool swipeTypeDetermined;

	private float afterSwipe;

	private float buttonPY;

	private float buttonHeight = 20f;

	private bool buttonPressed;

	private string pageTitle;

	private TextProfile txtDark;

	private int l;

	private RectangleF s;

	private float py;

	public CreditsState(float optionsScrollTop)
	{
		base.TransDuration = 30;
		IsOverlay = true;
		ShowCoins = false;
		this.optionsScrollTop = optionsScrollTop;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 11100);
		scrollBg = default(Color).FromRgb(13343100);
		int width = _(SpriteName.facts_scroll_edge).Width;
		scroll = new RectangleF((float)(base.core.Renderer.ScreenWidth - width) * 0.5f, 28 + base.topSafeArea, width, base.core.Renderer.ScreenHeight - 28 - base.topSafeArea - 61);
		int num = base.core.Renderer.ScreenHeight - 40;
		touchMenu.SetupButton(Button.Back, new RectangleF(scroll.Center.X - 35f, num, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		pageH = 50f;
		pageY = 0f;
		swipeLengthV = 0f;
		txtDark = new TextProfile
		{
			Decoration = TextDecoration.None,
			Color = default(Color).FromRgb(5185837),
			Scale = 1f,
			Width = 129,
			BoxAlignment = Alignment2D.Left,
			TextAlignment = Alignment2D.Left
		};
	}

	public override void Update()
	{
		IsOpaque = Transition == TransType.None;
		afterSwipe *= 0.9f;
		if (!swipeTypeDetermined)
		{
			pageY += afterSwipe;
		}
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
		if (Transition == TransType.Out)
		{
			pageY *= 0.8f;
		}
		base.Update();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.Back].Rectangle.Shift(0f, (float)Tween.CircEaseOut(base.Trans, 80.0, -80.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		if (Transition != TransType.None)
		{
			return;
		}
		if (base.core.Input.WasPressed(GameAction.Confirm))
		{
			OnBackButtonPressed();
			return;
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
							swipeIsVertical = true;
						}
						targetSwipeLengthV = (swipeIsVertical ? vector.Y : 0f);
						int num = (swipeIsVertical ? 5 : 20);
						if (vector.Length() < (float)num)
						{
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
					if (item.Id == swipeTouchID)
					{
						swipeTouchID = -1;
						if (buttonPressed)
						{
							SendMessage(new PlaySoundMessage(SoundName.button_up));
						}
						buttonPressed = false;
					}
					break;
				}
			}
		}
		if (!flag)
		{
			swipeTouchID = -1;
			targetSwipeLengthV = 0f;
			swipeTypeDetermined = false;
		}
		base.HandleInput();
	}

	private void OnButtonRelease(Button button)
	{
		if (button == Button.Back)
		{
			OnBackButtonPressed();
		}
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		TransitionOut(CoreEvent.HideGetCoins);
		base.OnBackButtonPressed();
	}

	public override void Draw()
	{
		l = 11000;
		float num = (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", l - 10, false].FillScreen(Color.Black * Component._m((float)base.Trans / 5f, 1f));
		s = scroll.Clone();
		if (base.core.TakingScreenshot)
		{
			s.Height = (float)base.core.Renderer.ScreenHeight - s.Top * 2f;
		}
		s.Shift(0f, (optionsScrollTop - 35f) * (float)Tween.BackEaseOut(Component._m(base.Trans, (float)base.TransDuration / 1.5f), 1.0, -1.0, (float)base.TransDuration / 1.5f));
		s.Height = 31f + (s.Height - 31f) * (float)Tween.BackEaseOut(Component._M(base.Trans, 0f), 0.0, 1.0, base.TransDuration);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.shop_wall), base.core.Renderer.ScreenCenter, Color.White * num, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", l, false].DrawRectangleS(s, scrollBg);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_scroll_edge), s.TopLeft, null, null, 0f, SpriteFlip.Vertical);
		base.core.Renderer["fg", l, false].DrawSpriteS(_(SpriteName.facts_scroll_edge), s.BottomLeft, null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomLeft);
		DrawContent();
		pageH = 480f;
		if (base.Trans > 5)
		{
			base.core.Renderer["fg", l + 1, false].DrawRectangleS(new RectangleF(s.Left - 5f, -5f, s.Width + 10f, s.Top), Color.Black);
			base.core.Renderer["fg", l + 1, false].DrawRectangleS(new RectangleF(s.Left - 5f, s.Bottom + 5f, s.Width + 10f, 50f), Color.Black);
		}
		base.core.Renderer["fg", l + 5, false].DrawSpriteS(_(SpriteName.facts_scroll_top), s.TopLeft.Shift(-14f, -17f));
		base.core.Renderer["fg", l + 5, false].DrawSpriteS(_(SpriteName.facts_scroll_bottom), s.BottomLeft.Shift(-14f, -3f));
		touchMenu.Draw();
		base.Draw();
	}

	private void _space(int height)
	{
		py += height;
	}

	private void _h1(string text, float scale = 1f)
	{
		int num = 13;
		if (py > (float)(-num) && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawTextS(text, new Vector2(s.Left, s.Top + py), txtDark.Alter(null, null, null, null, null, null, Alignment2D.Center, null, scale));
		}
		py += num;
	}

	private void _h2(string text, bool bright = true, float scale = 0.7f, float dx = 0f)
	{
		float num = 13f * scale;
		if (py > 0f - num && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawTextS(text, new Vector2(s.Left + dx, s.Top + py), txtDark.Alter(textAlignment: Alignment2D.Center, color: default(Color).FromRgb(bright ? 5185837 : 7883070), secondColor: null, decoration: null, width: null, height: null, boxAlignment: null, font: null, scale: scale));
		}
		py += num;
	}

	private void _image(SpriteName image, float scale = 0.4f, float dx = 0f)
	{
		Sprite sprite = _(image);
		float num = (float)sprite.Height * scale;
		if (py > 0f - num && py < s.Height)
		{
			base.core.Renderer["fg", l, false].DrawSpriteS(sprite, new Vector2(s.Left + s.Width / 2f + dx, s.Top + py + 4f), null, Vector2.One * scale, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
		}
		py += num + 3f;
	}

	private void DrawContent()
	{
		py = pageY;
		_space(5);
		_h1(__(SId.CREDITS_credits));
		_space(5);
		if (base.Trans >= 2)
		{
			_h2("-= REDUNGEON =-", bright: false);
			_space(5);
			float num = py;
			_h2(__(SId.CREDITS_a_game_by).ToUpper(), bright: false, 0.5f, -30f);
			_space(3);
			_image(SpriteName.credits_eneminds_logo, 0.45f, -30f);
			py = num;
			_h2(__(SId.CREDITS_published_by).ToUpper(), bright: false, 0.5f, 30f);
			_image(SpriteName.credits_nitrome_logo, 0.45f, 34f);
			_space(10);
			_h2(__(SId.CREDITS_art_and_programming).ToUpper(), bright: false, 0.5f);
			_h2(__(SId.CREDITS_13x666).ToUpper());
			_space(5);
			_h2(__(SId.CREDITS_programming).ToUpper(), bright: false, 0.5f);
			_h2(__(SId.CREDITS_iodiot).ToUpper());
			_space(5);
			_h2(__(SId.CREDITS_sound_design).ToUpper(), bright: false, 0.5f);
			_h2(__(SId.CREDITS_milena).ToUpper());
			_space(5);
			_h2(__(SId.CREDITS_music).ToUpper(), bright: false, 0.5f);
			_h2("DAVE COWEN");
			_space(5);
			_h2(__(SId.CREDITS_additional_art).ToUpper(), bright: false, 0.5f);
			_h2("MARKUS HEINEL");
			_space(10);
			_h2("-= " + __(SId.CREDITS_translators).ToUpper() + " =-".ToUpper(), bright: false);
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.uk_UA].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Юлія Мірошниченко".ToUpper());
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.pt_PT].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Ignacio Costa".ToUpper());
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.pl_PL].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Thomas Poptshyk".ToUpper());
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.de_DE].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Rick Leinichen".ToUpper());
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.fr_FR].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Romain Pillard".ToUpper());
			_space(5);
			_h2(base.core.LocaleManager.Locales[Language.es_ES].LanguageName.ToUpper(), bright: false, 0.5f);
			_h2("Alconost Inc.".ToUpper());
			_space(10);
			_h2(__(SId.CREDITS_special_thanks).ToUpper(), bright: false);
			_space(5);
			_h2("YULKA MI");
			_h2("UNCLE LEM");
			_h2("YEGORF1");
			_h2("SHARKUS");
			_h2("PREPOD");
			_h2("HEXETTE");
			_h2("LUTLIN");
			_h2("LIBRARIAN");
			_h2("LAWENARD");
			_space(10);
			_h2("2016", bright: false);
			_space(10);
		}
	}
}
