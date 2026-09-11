using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.Gameplay;

public class PlayerControlSchemeSwipe : PlayerControlScheme
{
	private enum Button
	{
		Action
	}

	public Vector2 SwipeTouchDir;

	private bool swiping;

	private int swipeTouchId;

	private Vector2 swipeStart;

	private Vector2 swipePos;

	private const int SWIPE_DEAD_ZONE = 10;

	private const int SWIPE_REPEAT_DELAY = 9;

	private const int SWIPE_CHANGE_THRESHOLD = 10;

	private int untilJump;

	private float tutorialOpacity = 1f;

	private int tutorialStart = -1;

	private readonly TouchMenu<Button> touchMenu;
	private Point layoutSize;
	private bool layoutLeftHanded;

	public PlayerControlSchemeSwipe(PlayState playState)
		: base(playState)
	{
		SwipeTouchDir = Vector2.Zero;
		int num = base.core.Renderer.ScreenHeight / 3;
		int num2 = base.core.Renderer.ScreenHeight - num;
		touchMenu = new TouchMenu<Button>(OnButtonPress, OnButtonRelease);
		RectangleF rectangle = new RectangleF(base.core.OptionsData.LeftHandedMode ? (5f / Settings.GuiScale) : ((float)base.core.Renderer.ScreenWidth - 45f / Settings.GuiScale), (float)num2 - 30f / Settings.GuiScale, 40f / Settings.GuiScale, 40f / Settings.GuiScale);
		touchMenu.SetupButton(Button.Action, rectangle, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none);
		if (!playState.Hud.AbilitiesHud.HasActiveSkill)
		{
			touchMenu[Button.Action].Hidden = true;
		}
		if (!base.core.ProfileData.LearnedSwipes)
		{
			tutorialStart = 30;
		}
		else
		{
			tutorialStart = -1;
		}
	}

	public override Vector2 SkillButtonCenter()
	{
		RelayoutSkillButton();
		if (touchMenu != null)
		{
			return touchMenu[Button.Action].Rectangle.Center;
		}
		return Vector2.Zero;
	}

	public override void Load()
	{
		InitSkillButton();
		base.Load();
	}

	private void OnButtonPress(Button button)
	{
	}

	private void OnButtonRelease(Button button)
	{
		if (button == Button.Action)
		{
			TapSkillButton();
		}
	}

	public override void Update()
	{
		if (base.core.ProfileData.LearnedSwipes && playState.Session.Ticks > 180 && !playState.PlayerMoved)
		{
			base.core.ProfileData.LearnedSwipes = false;
			tutorialStart = base.core.CurrentPlayState.Session.Ticks + 1;
		}
		if (playState.PlayerMoved && tutorialOpacity > 0.001f)
		{
			tutorialOpacity *= 0.8f;
			if (tutorialOpacity <= 0.001f)
			{
				tutorialStart = -1;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		RelayoutSkillButton();
		if (DrawGamepadHud()) return;
		if (base.core.TakingScreenshot)
		{
			return;
		}
		DrawSkillButton(touchMenu[Button.Action].Rectangle.Center, touchMenu[Button.Action].IsDown);
		int num = 250;
		if (tutorialStart >= 0 && playState.Session.Ticks > tutorialStart && !base.core.TakingScreenshot && !playState.Player.Dead)
		{
			int num2 = (playState.Session.Ticks - tutorialStart) % num;
			Vector2 v = new Vector2(base.core.Renderer.ScreenCenter.X, 85f);
			Vector2 v2 = v.Clone();
			bool flag = false;
			float num3 = 1f;
			bool flag2 = false;
			int num4 = 30;
			flag2 = playState.Session.Ticks - tutorialStart < 20;
			int num5 = 20;
			if (num2 < num5)
			{
				num3 = (float)num2 / (float)num5;
			}
			num2 -= num5;
			num5 = 15;
			if (num2 > 0 && num2 <= num5)
			{
				flag = true;
			}
			if (num2 > 10 && num2 - 10 <= num4)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.swipe_arrow), v2.Shift((float)Tween.CircEaseIn(num2 - 10, 0.0, -20.0, num4), 0f), Color.White * tutorialOpacity * Component._m(1f, 1f - (float)(num2 - 10 - 15) / (float)(num4 - 15)), null, 0f, SpriteFlip.Horizontal, SpriteOrigin.Center);
			}
			num2 -= num5;
			num5 = 10;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift((float)Tween.SineEaseOut(num2, 0.0, -30.0, num5), 0f);
				flag = num2 < 5;
			}
			num2 -= num5;
			num5 = 10;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(-30f, 0f);
				flag = false;
			}
			num2 -= num5;
			num5 = 20;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(-30f + (float)Tween.BackEaseOut(num2, 0.0, 30.0, num5), 0f);
				flag = false;
			}
			num2 -= num5;
			num5 = 15;
			if (num2 > 0 && num2 <= num5)
			{
				flag = true;
			}
			if (num2 > 10 && num2 - 10 <= num4)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.swipe_arrow), v2.Shift((float)Tween.CircEaseIn(num2 - 10, 0.0, 20.0, num4), 0f), Color.White * tutorialOpacity * Component._m(1f, 1f - (float)(num2 - 10 - 15) / (float)(num4 - 15)), null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			num2 -= num5;
			num5 = 10;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift((float)Tween.SineEaseOut(num2, 0.0, 30.0, num5), 0f);
				flag = num2 < 5;
			}
			num2 -= num5;
			num5 = 10;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(30f, 0f);
				flag = false;
			}
			num2 -= num5;
			num5 = 20;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(30f - (float)Tween.SineEaseOut(num2, 0.0, 30.0, num5), (float)Tween.SineEaseIn(num2, 0.0, 15.0, num5));
				flag = false;
			}
			num2 -= num5;
			num5 = 15;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(0f, 15f);
				flag = true;
			}
			if (num2 > 10 && num2 - 10 <= num4)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.swipe_arrow), v2.Shift(0f, 15f + (float)Tween.CircEaseIn(num2 - 10, 0.0, -20.0, num4)), Color.White * tutorialOpacity * Component._m(1f, 1f - (float)(num2 - 10 - 15) / (float)(num4 - 15)), null, -(float)Math.PI / 2f, SpriteFlip.None, SpriteOrigin.Center);
			}
			if (num2 > 30 && num2 - 30 <= num4)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.swipe_arrow), v2.Shift(0f, 15f + (float)Tween.CircEaseIn(num2 - 30, 0.0, -20.0, num4)), Color.White * tutorialOpacity * Component._m(1f, 1f - (float)(num2 - 30 - 15) / (float)(num4 - 15)), null, -(float)Math.PI / 2f, SpriteFlip.None, SpriteOrigin.Center);
			}
			if (num2 > 50 && num2 - 50 <= num4)
			{
				base.core.Renderer["fg", 1, false].DrawSpriteS(_(SpriteName.swipe_arrow), v2.Shift(0f, 15f + (float)Tween.CircEaseIn(num2 - 50, 0.0, -20.0, num4)), Color.White * tutorialOpacity * Component._m(1f, 1f - (float)(num2 - 50 - 15) / (float)(num4 - 15)), null, -(float)Math.PI / 2f, SpriteFlip.None, SpriteOrigin.Center);
			}
			num2 -= num5;
			num5 = 25;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(0f, 15f - (float)Tween.SineEaseOut(num2, 0.0, 50.0, num5));
				flag = true;
			}
			num2 -= num5;
			num5 = 30;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(0f, -35f);
				flag = true;
			}
			num2 -= num5;
			num5 = 20;
			if (num2 > 0 && num2 <= num5)
			{
				v = v.Shift(0f, -35f);
				flag = false;
				num3 = (float)(num5 - num2) / (float)num5;
			}
			num2 -= num5;
			if (num2 > 0)
			{
				num3 = 0f;
			}
			base.core.Renderer["fg"].DrawRectangleS(new RectangleF(0f, v2.Y - 45f - 17f, base.core.Renderer.ScreenWidth + 2, 92f), Color.Black * (flag2 ? num3 : 1f) * 0.6f * tutorialOpacity);
			base.core.Renderer["fg", 1, false].DrawSpriteS(flag ? _(SpriteName.swipe_hand_2) : _(SpriteName.swipe_hand_1), v.Shift(-3f, -5f), Color.White * num3 * tutorialOpacity);
		}
		base.Draw();
	}

	public override void HandleInput()
	{
		RelayoutSkillButton();
		HandleDigitalInput();
		if (touchMenu.HandleInput())
		{
			swiping = false;
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (!swiping && (item.State == TouchLocationState.Pressed || item.State == TouchLocationState.Moved))
			{
				swipeTouchId = item.Id;
				swipeStart = item.Position;
				swiping = true;
				SwipeTouchDir.X = 0f;
				SwipeTouchDir.Y = 0f;
			}
			if (item.Id == swipeTouchId)
			{
				swipePos = item.Position;
				Vector2 v = swipePos - swipeStart;
				switch (item.State)
				{
				case TouchLocationState.Moved:
					if (v.Length() > 10f)
					{
						SwipeTouchDir = v.Direction();
					}
					else
					{
						untilJump = 0;
					}
					break;
				case TouchLocationState.Released:
					if (v.Length() < 10f && base.core.OptionsData.TapToStep)
					{
						SwipeTouchDir.Y = -1f;
						Jump();
					}
					swiping = false;
					untilJump = 0;
					break;
				}
			}
			if (item.Id == swipeTouchId && item.State == TouchLocationState.Moved && (item.Position - swipeStart).Length() > 10f)
			{
				if (untilJump == 0)
				{
					Jump();
					untilJump = 9;
				}
				else
				{
					untilJump--;
				}
			}
		}
	}

	private void RelayoutSkillButton()
	{
		Point size = new(R.ScreenWidth, R.ScreenHeight);
		bool leftHanded = core.OptionsData.LeftHandedMode;
		if (size == layoutSize && leftHanded == layoutLeftHanded) return;
		Reset();
		touchMenu[Button.Action].Rectangle = new RectangleF(leftHanded ? 5f / Settings.GuiScale : size.X - 45f / Settings.GuiScale,
			size.Y - size.Y / 3 - 30f / Settings.GuiScale, 40f / Settings.GuiScale, 40f / Settings.GuiScale);
		layoutSize = size;
		layoutLeftHanded = leftHanded;
	}

	public override void Reset()
	{
		touchMenu.ReleaseButtons();
		swiping = false;
		SwipeTouchDir = Vector2.Zero;
		untilJump = 0;
		base.Reset();
	}

	private void Jump()
	{
		playState.Jump(SwipeTouchDir);
		untilJump = 9;
		base.core.ProfileData.LearnedSwipes = true;
	}
}
