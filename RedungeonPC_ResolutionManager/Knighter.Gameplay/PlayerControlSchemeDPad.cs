using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.Gameplay;

public class PlayerControlSchemeDPad : PlayerControlScheme
{
	private enum Button
	{
		North,
		East,
		South,
		West,
		Action
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly Dictionary<Direction, Vector2> directionVectors;

	private Direction repeatDirection;

	private int holdingTicks = -1;

	private Button pressedButton;

	private bool firstRepeat = true;

	private int swipeTouchId;

	private Vector2 swipeTouchStart;

	private bool swipeInProgress;

	private const int SWIPE_DEAD_ZONE = 64;

	private const int DOUBLE_TAP_MAX_DELAY = 30;

	private const int DOUBLE_TAP_MAX_SPREAD = 30;

	private int ticksSinceLastTap;

	private bool waitingForSecondTap;

	private Vector2 firstTapPosition;

	private int secontTapId;
	private int layoutWidth = -1;
	private int layoutHeight = -1;
	private bool layoutLeftHanded;
	private float compactVisualScale;

	public bool Compact { get; private set; }

	public PlayerControlSchemeDPad(PlayState playState, bool compact = false)
		: base(playState)
	{
		Compact = compact;
		touchMenu = new TouchMenu<Button>(OnButtonPress, OnButtonRelease);
		var layout = TouchDPadLayout.Create(base.core.Renderer.ScreenWidth, base.core.Renderer.ScreenHeight, base.core.OptionsData.LeftHandedMode, compact);
		RectangleF rectangle = layout.West;
		RectangleF rectangle2 = layout.East;
		RectangleF rectangleF = layout.North;
		RectangleF rectangle3 = layout.South;
		RectangleF rectangle4 = layout.Action;
		float dpadScale = 1f / Settings.GuiScale;
		touchMenu.SetupButton(Button.West, rectangle, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_left_o) : _(SpriteName.dpad_left)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_left_pressed_o) : _(SpriteName.dpad_left_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, dpadScale, compact ? RectSector.West : RectSector.Whole);
		touchMenu.SetupButton(Button.East, rectangle2, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_right_o) : _(SpriteName.dpad_right)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_right_pressed_o) : _(SpriteName.dpad_right_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, dpadScale, compact ? RectSector.East : RectSector.Whole);
		touchMenu.SetupButton(Button.North, rectangleF, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_up_o) : _(SpriteName.dpad_up)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_up_pressed_o) : _(SpriteName.dpad_up_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, dpadScale, compact ? RectSector.North : RectSector.Whole);
		touchMenu.SetupButton(Button.South, rectangle3, compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_down_o) : _(SpriteName.dpad_down)), compact ? null : (base.core.OptionsData.SeeThroughMode ? _(SpriteName.dpad_down_pressed_o) : _(SpriteName.dpad_down_pressed)), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, dpadScale, compact ? RectSector.South : RectSector.Whole);
		touchMenu.SetupButton(Button.Action, rectangle4, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none, seeThrough: false, dpadScale);
		touchMenu[Button.Action].Hidden = !playState.Hud.AbilitiesHud.HasActiveSkill;
		directionVectors = new Dictionary<Direction, Vector2>();
		directionVectors[Direction.North] = new Vector2(0f, -1f);
		directionVectors[Direction.East] = new Vector2(1f, 0f);
		directionVectors[Direction.South] = new Vector2(0f, 1f);
		directionVectors[Direction.West] = new Vector2(-1f, 0f);
		RelayoutTouchButtons();
	}

	public override Vector2 SkillButtonCenter()
	{
		RelayoutTouchButtons();
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

	public override void UpdateTransition()
	{
		RelayoutTouchButtons();
		// Keep the cross together during its entrance, including compact sectors.
		float offset = (float)Tween.BackEaseOut(playState.Trans, 150.0, -150.0, playState.TransDuration);
		touchMenu[Button.West].Rectangle.Shift(0f, offset);
		touchMenu[Button.East].Rectangle.Shift(0f, offset);
		touchMenu[Button.North].Rectangle.Shift(0f, offset);
		touchMenu[Button.South].Rectangle.Shift(0f, offset);
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		RelayoutTouchButtons();
		HandleDigitalInput();
		touchMenu.HandleInput();
		foreach (TouchLocation item in base.core.TouchState)
		{
			bool flag = false;
			bool flag2 = !touchMenu[Button.East].Rectangle.Contains(item.Position) && !touchMenu[Button.West].Rectangle.Contains(item.Position) && !touchMenu[Button.North].Rectangle.Contains(item.Position) && !touchMenu[Button.South].Rectangle.Contains(item.Position);
			if (item.State == TouchLocationState.Pressed && !swipeInProgress && flag2)
			{
				swipeTouchId = item.Id;
				swipeTouchStart = item.Position;
				swipeInProgress = true;
			}
			if (swipeInProgress && item.Id == swipeTouchId)
			{
				Vector2 vector = item.Position - swipeTouchStart;
				TouchLocationState state = item.State;
				if (state == TouchLocationState.Released)
				{
					if (Math.Abs(vector.X) > 64f)
					{
						playState.Player.OnSwipe();
						flag = true;
					}
					swipeInProgress = false;
				}
			}
			if (!waitingForSecondTap)
			{
				if (item.Id != secontTapId && item.State == TouchLocationState.Released && !flag && flag2 && item.Position.Y < touchMenu[Button.North].Rectangle.Top)
				{
					waitingForSecondTap = true;
					ticksSinceLastTap = 0;
					firstTapPosition = item.Position;
				}
			}
			else if (item.State == TouchLocationState.Pressed && (item.Position - firstTapPosition).Length() < 30f)
			{
				secontTapId = item.Id;
				waitingForSecondTap = false;
				playState.Player.OnDoubleTap();
			}
		}
	}

	public override void Update()
	{
		if (base.core.OptionsData.HoldToRun)
		{
			if (!base.core.CurrentPlayState.IsTopState)
			{
				holdingTicks = -1;
			}
			if (holdingTicks >= 0 && touchMenu[pressedButton].IsDown)
			{
				holdingTicks++;
				if (holdingTicks >= 11 + (firstRepeat ? 7 : 0))
				{
					firstRepeat = false;
					playState.Jump(directionVectors[repeatDirection]);
					holdingTicks = 0;
				}
			}
		}
		if (waitingForSecondTap)
		{
			ticksSinceLastTap++;
			if (ticksSinceLastTap > 30)
			{
				waitingForSecondTap = false;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		RelayoutTouchButtons();
		if (DrawGamepadHud()) return;
		for (Button button = Button.North; button <= Button.West; button++)
			touchMenu[button].Opacity = core.OptionsData.ControlOpacity;
		if (!base.core.TakingScreenshot)
		{
			touchMenu.Draw();
			if (Compact)
			{
				base.core.Renderer["fg", 0, false].DrawSpriteS(_((touchMenu[Button.North].Pressed ? "dpad_n" : (touchMenu[Button.West].Pressed ? "dpad_w" : (touchMenu[Button.East].Pressed ? "dpad_e" : (touchMenu[Button.South].Pressed ? "dpad_s" : "dpad")))) + (base.core.OptionsData.SeeThroughMode ? "_o" : "")), touchMenu[Button.South].Rectangle.Center.Shift(0f, 3f), (base.core.OptionsData.SeeThroughMode ? (TextProfile.OrangeMiddle * 0.6f) : Color.White) * core.OptionsData.ControlOpacity, Vector2.One * compactVisualScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
			}
			DrawSkillButton(touchMenu[Button.Action].Rectangle.Center, touchMenu[Button.Action].IsDown);
		}
	}

	public override void Reset()
	{
		touchMenu.ReleaseButtons();
		holdingTicks = -1;
		firstRepeat = true;
		swipeInProgress = false;
		waitingForSecondTap = false;
		ticksSinceLastTap = 0;
		base.Reset();
	}

	private void OnButtonPress(Button button)
	{
		switch (button)
		{
		case Button.North:
		case Button.East:
		case Button.South:
		case Button.West:
			playState.Jump(directionVectors[(Direction)button]);
			repeatDirection = (Direction)button;
			holdingTicks = 0;
			firstRepeat = true;
			pressedButton = button;
			break;
		case Button.Action:
			TapSkillButton();
			break;
		}
	}

	private void OnButtonRelease(Button button)
	{
		holdingTicks = -1;
		firstRepeat = true;
	}

	private void RelayoutTouchButtons()
	{
		int width = base.core.Renderer.ScreenWidth;
		int height = base.core.Renderer.ScreenHeight;
		bool leftHanded = base.core.OptionsData.LeftHandedMode;
		if (width == layoutWidth && height == layoutHeight && leftHanded == layoutLeftHanded)
			return;

		Reset(); // A finger held at the old position must not keep moving the player.
		var layout = TouchDPadLayout.Create(width, height, leftHanded, Compact);
		touchMenu[Button.West].Rectangle = layout.West;
		touchMenu[Button.East].Rectangle = layout.East;
		touchMenu[Button.North].Rectangle = layout.North;
		touchMenu[Button.South].Rectangle = layout.South;
		touchMenu[Button.Action].Rectangle = layout.Action;
		if (Compact)
		{
			Sprite sprite = _(base.core.OptionsData.SeeThroughMode ? "dpad_o" : "dpad");
			compactVisualScale = layout.ArrowSize / Math.Max(sprite.Width, sprite.Height);
		}
		else
		{
			for (Button button = Button.North; button <= Button.West; button++)
			{
				var desc = touchMenu[button];
				desc.Scale = layout.ArrowSize / Math.Max(desc.Sprite.Width, desc.Sprite.Height);
			}
		}
		layoutWidth = width;
		layoutHeight = height;
		layoutLeftHanded = leftHanded;
	}
}
