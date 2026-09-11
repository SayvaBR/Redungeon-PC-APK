using System;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.Gameplay;

public class PlayerControlSchemeTouchMap : PlayerControlScheme
{
	private enum Button
	{
		Action
	}

	public Vector2 SwipeTouchDir;

	private bool swiping;

	private int swipeTouchId;

	private Vector2 origin;

	private Vector2 touchPos;

	private Vector2 offset;

	private const int MAP_SCALE = 10;

	private int untilJump;

	private const int JUMP_REPEAT_DELAY = 5;

	private readonly TouchMenu<Button> touchMenu;
	private Point layoutSize;
	private bool layoutLeftHanded;

	public PlayerControlSchemeTouchMap(PlayState playState)
		: base(playState)
	{
		SwipeTouchDir = Vector2.Zero;
		int num = base.core.Renderer.ScreenHeight / 3;
		int num2 = base.core.Renderer.ScreenHeight - num;
		touchMenu = new TouchMenu<Button>(OnButtonPress);
		RectangleF rectangle = new RectangleF(base.core.OptionsData.LeftHandedMode ? (5f / Settings.GuiScale) : ((float)base.core.Renderer.ScreenWidth - 45f / Settings.GuiScale), (float)num2 - 30f / Settings.GuiScale, 40f / Settings.GuiScale, 40f / Settings.GuiScale);
		touchMenu.SetupButton(Button.Action, rectangle, null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.none, SoundName.none);
		if (!playState.Hud.AbilitiesHud.HasActiveSkill)
		{
			touchMenu[Button.Action].Hidden = true;
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
		if (button == Button.Action)
		{
			TapSkillButton();
		}
	}

	public override void Draw()
	{
		RelayoutSkillButton();
		if (DrawGamepadHud()) return;
		if (!base.core.TakingScreenshot)
		{
			DrawSkillButton(touchMenu[Button.Action].Rectangle.Center, touchMenu[Button.Action].IsDown);
			if (swiping)
			{
				float opacity = MathHelper.Clamp(core.OptionsData.ControlOpacity, 0f, 1f);
				base.core.Renderer["fg"].DrawDotS(origin, Color.Yellow * (0.5f * opacity), 8f);
				base.core.Renderer["fg"].DrawDotS(touchPos, Color.Red * (0.5f * opacity), 8f);
				base.core.Renderer["fg"].DrawDotW(playState.Player.WorldCenter + 16f * (touchPos - origin) / 10f, Color.Red * (0.5f * opacity), 8f);
			}
			base.Draw();
		}
	}

	public override void HandleInput()
	{
		RelayoutSkillButton();
		HandleDigitalInput();
		if (touchMenu.HandleInput())
		{
			return;
		}
		foreach (TouchLocation item in base.core.TouchState)
		{
			if (!swiping && (item.State == TouchLocationState.Pressed || item.State == TouchLocationState.Moved))
			{
				swipeTouchId = item.Id;
				origin = item.Position;
				swiping = true;
				offset = new Vector2(0f);
				untilJump = 0;
			}
			if (!swiping || item.Id != swipeTouchId)
			{
				continue;
			}
			touchPos = item.Position;
			Vector2 vector = touchPos - origin;
			vector.X = Convert.ToInt32(vector.X / 10f);
			vector.Y = Convert.ToInt32(vector.Y / 10f);
			Vector2 vector2 = (vector - offset).Direction();
			switch (item.State)
			{
			case TouchLocationState.Moved:
				if (!vector.IsEqualTo(offset))
				{
					playState.Player.FacingDirection = vector2;
					if (untilJump == 0)
					{
						playState.Jump(vector2);
						offset += vector2;
						untilJump = 5;
						origin = touchPos;
						offset = new Vector2(0f);
					}
					else
					{
						untilJump--;
					}
				}
				break;
			case TouchLocationState.Released:
				swiping = false;
				break;
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
		offset = Vector2.Zero;
		untilJump = 0;
		base.Reset();
	}
}
