using System;
using Knighter.Entities;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Input;
using Knighter.States;
using Microsoft.Xna.Framework;

namespace Knighter.Gameplay;

public abstract class PlayerControlScheme : Component
{
	protected PlayState playState;

	private float skillCharge;

	private int skillAnim;

	private bool skillAppearing;

	private Vector2 heldDigitalDirection;

	private int digitalHoldingTicks = -1;

	private bool firstDigitalRepeat = true;

	protected Sprite barLeft;

	protected Sprite barMiddle;

	protected Sprite barRight;

	protected AbilityDesc activeSkillDesc;

	protected const int REPEAT_DELAY = 11;

	public abstract Vector2 SkillButtonCenter();

	protected void InitSkillButton()
	{
		if (playState.Hud.AbilitiesHud.HasActiveSkill)
		{
			Skill activeSkill = playState.Hud.AbilitiesHud.ActiveSkill;
			activeSkillDesc = Abilities.SkillDesc[activeSkill];
			barLeft = _(activeSkillDesc.HudChargeBar.Value);
			barMiddle = barLeft.Reduce(3, 0, 2, 0);
			barRight = barLeft.Reduce(5, 0, 0, 0);
			barLeft = barLeft.Reduce(0, 0, 5, 0);
		}
	}

	private void DrawSkillButtonColored(Vector2 bp, Color c1, Color c2, Color c3, bool pressed, float p = 1f)
	{
		float opacity = MathHelper.Clamp(core.OptionsData.ControlOpacity, 0f, 1f);
		c1 *= opacity;
		c2 *= opacity;
		c3 *= opacity;
		if (base.core.OptionsData.SeeThroughMode)
		{
			c1 *= 0f;
			c2 *= (pressed ? 0.3f : 0f);
			c3 *= (pressed ? 0f : 0.6f);
		}
		Sprite sprite = _(SpriteName.skill_button_up_c1);
		int num = (int)((1f - p) * (float)sprite.Height);
		float y = (float)num / Settings.GuiScale;
		if (!pressed)
		{
			base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c1).Reduce(0, num, 0, 0), bp.Shift(0f, y), c1, Vector2.One / Settings.GuiScale);
			base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c2).Reduce(0, num, 0, 0), bp.Shift(0f, y), c2, Vector2.One / Settings.GuiScale);
			base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c3).Reduce(0, num, 0, 0), bp.Shift(0f, y), c3, Vector2.One / Settings.GuiScale);
		}
		else
		{
			base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_down_c1).Reduce(0, num, 0, 0), bp.Shift(0f, y), c1, Vector2.One / Settings.GuiScale);
			base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_down_c2).Reduce(0, num, 0, 0), bp.Shift(0f, y), c2, Vector2.One / Settings.GuiScale);
		}
		if (p < 0.999f)
		{
			float num2 = ((p < 0.2f) ? Component._sin(p / 0.2f * (float)Math.PI / 2f) : ((p > 0.95f) ? (1f - Component._sin((p - 0.95f) / 0.05f * (float)Math.PI / 2f)) : 1f));
			Vector2 position = bp + new Vector2(-4.5f, (1f - p) * (float)(sprite.Height - 3) - 2f) / Settings.GuiScale;
			base.core.Renderer["fg", 13, false].DrawSpriteS(_(SpriteName.button_charge_1), position, c3 * (num2 + num2 * 0.1f * Component._cos((float)base.ticks * 0.2f)), Vector2.One / Settings.GuiScale);
			base.core.Renderer["fg", 13, false].DrawSpriteS(_(SpriteName.button_charge_2), position, Color.White * opacity * (num2 + num2 * 0.1f * Component._sin((float)base.ticks * 0.2f)), Vector2.One / Settings.GuiScale);
		}
	}

	protected void DrawSkillButton(Vector2 center, bool pressed)
	{
		if (playState.Hud.AbilitiesHud.HasActiveSkill)
		{
			Vector2 v = center;
			float num = (float)Tween.CircEaseOut(base.core.CurrentPlayState.Trans, 60.0, -60.0, base.core.CurrentPlayState.TransDuration);
			if (base.core.OptionsData.LeftHandedMode)
			{
				num *= -1f;
			}
			v = v.Shift(num, 0f);
			if (skillAnim > 0 && skillAppearing)
			{
				v += SciHelper.GetRandomVectorInCircle(2f);
			}
			Vector2 bp = v + new Vector2(-14f, -12f) / Settings.GuiScale;
			Color c;
			Color c2;
			Color c3;
			if (!skillCharge.Equals(1f))
			{
				c = default(Color).FromRgb(2435639);
				c2 = default(Color).FromRgb(3619654);
				c3 = default(Color).FromRgb(5463138);
				DrawSkillButtonColored(bp, c, c2, c3, pressed);
			}
			c = default(Color).FromRgb(activeSkillDesc.Color1);
			c2 = default(Color).FromRgb(activeSkillDesc.Color2);
			c3 = default(Color).FromRgb(activeSkillDesc.Color3);
			DrawSkillButtonColored(bp, c, c2, c3, pressed, skillCharge);
			Sprite sprite = _(activeSkillDesc.HudMainIcon.Value);
			base.core.Renderer["fg", 12, false].DrawSpriteS(sprite, v + new Vector2(-1 - sprite.Width / 2, 2.5f + (float)(pressed ? 3 : 0) - (float)(sprite.Height / 2)) / Settings.GuiScale, Color.White * MathHelper.Clamp(core.OptionsData.ControlOpacity, 0f, 1f), Vector2.One / Settings.GuiScale);
		}
	}

	protected PlayerControlScheme(PlayState playState)
	{
		this.playState = playState;
	}

	public override void Update()
	{
		Skill activeSkill = playState.Hud.AbilitiesHud.ActiveSkill;
		float num = skillCharge;
		skillCharge = playState.Player.Abilities.SkillCharge[activeSkill];
		if (!num.Equals(1f) && skillCharge.Equals(1f))
		{
			skillAppearing = true;
			skillAnim = 15;
		}
		if (num.Equals(1f) && !skillCharge.Equals(1f))
		{
			skillAppearing = false;
			skillAnim = 15;
		}
		if (skillAnim > 0)
		{
			skillAnim--;
		}
		base.Update();
	}

	protected void TapSkillButton()
	{
		Skill activeSkill = playState.Hud.AbilitiesHud.ActiveSkill;
		var _ = Abilities.SkillDesc[activeSkill];
		if (playState.Hud.AbilitiesHud.HasActiveSkill && playState.Player.Abilities.SkillCharge[activeSkill].Equals(1f))
		{
			playState.Player.TryTriggerAbility();
			playState.OnPlayerAction();
		}
	}

	protected bool DrawGamepadHud()
	{
		if (InputDeviceTracker.CurrentDevice != InputDevice.Gamepad) return false;
		if (!core.TakingScreenshot && playState.IsTopState && playState.Hud.AbilitiesHud.HasActiveSkill)
		{
			Vector2 center = new Vector2(R.ScreenWidth - 30f, R.ScreenHeight - 34f);
			Sprite icon = _(activeSkillDesc.HudMainIcon.Value);
			R["fg", 100, false].DrawSpriteS(icon, center, Color.White, Vector2.One / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.Center);
			R["fg", 101, false].DrawRectangleS(new RectangleF(center.X - 12f, center.Y + 13f, 24f, 2f), Color.Gray * 0.6f);
			R["fg", 102, false].DrawRectangleS(new RectangleF(center.X - 12f, center.Y + 13f, 24f * MathHelper.Clamp(skillCharge, 0f, 1f), 2f), Color.White);
			Knighter.UI.ContextPromptLegend.DrawButton(R, center + new Vector2(-8f, 17f), PromptAction.Confirm);
		}
		return true;
	}

	/// <summary>
	/// Teclado e gamepad funcionam em todos os esquemas visuais de controle.
	/// Swipe/D-Pad/TouchMap passam a escolher apenas como o toque se comporta,
	/// sem retirar acessibilidade dos dispositivos físicos.
	/// </summary>
	protected bool HandleDigitalInput()
	{
		bool handled = false;
		if (base.core.Input.WasPressed(GameAction.Ability))
		{
			TapSkillButton();
			handled = true;
		}

		if (base.core.Input.TryGetPressedDirection(out Vector2 pressedDirection))
		{
			playState.Jump(pressedDirection);
			heldDigitalDirection = pressedDirection;
			digitalHoldingTicks = 0;
			firstDigitalRepeat = true;
			return true;
		}

		if (heldDigitalDirection == Vector2.Zero || !base.core.Input.IsDirectionDown(heldDigitalDirection))
		{
			heldDigitalDirection = base.core.Input.GetHeldDirection();
			digitalHoldingTicks = heldDigitalDirection == Vector2.Zero ? -1 : 0;
			firstDigitalRepeat = true;
			return handled;
		}

		if (base.core.OptionsData.HoldToRun && digitalHoldingTicks >= 0)
		{
			digitalHoldingTicks++;
			int repeatAt = REPEAT_DELAY + (firstDigitalRepeat ? 7 : 0);
			if (digitalHoldingTicks >= repeatAt)
			{
				firstDigitalRepeat = false;
				playState.Jump(heldDigitalDirection);
				digitalHoldingTicks = 0;
				handled = true;
			}
		}

		return handled;
	}

	public virtual void UpdateTransition()
	{
	}

	public abstract void HandleInput();

	public virtual void Reset()
	{
		heldDigitalDirection = Vector2.Zero;
		digitalHoldingTicks = -1;
		firstDigitalRepeat = true;
	}
}
