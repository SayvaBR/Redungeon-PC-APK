using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Knighter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.States;

public class ShopState : State
{
	protected override bool ShowContextPromptLegend => false;

	private int insufficientCoinsTicks;

	private enum Button
	{
		Left,
		Right,
		Buy,
		Play,
		Back,
		Share,
		GetCoins,
		XPromo
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	public Character CurrentCharacter;

	private CharDescription charDesc;

	private int charLevel;

	private int upgradePrice;

	private Sprite charPortrait;

	private Sprite charNameImage;

	private string charNameLabel;

	private string charPriceLabel;

	private bool charUnlocked;

	private bool enoughCoins;

	private bool maxLevel;

	private int charShowingTicks;

	private bool immediatePurchase;

	private bool gameOver;

	public Skill? shownSkill;

	private int shownPanelLeft;

	private int shownPanelRight;

	private bool skillsShown;

	private int skillAnim;

	private const int skillAnimMax = 10;

	private int scrollAnim;

	private const int scrollDuration = 10;

	private int scrollDir;

	private Character nextChar;

	private bool showGuide = true;

	private float guideTargetX = -30f;

	private float guideX = -30f;

	private int guideTtl = 350;

	private int nextGuideTimer;

	private AbilitiesHud abilitiesHud;

	private Sprite pedestal;

	private Sprite lockIcon;

	private Sprite columnTop;

	private Sprite column;

	private Animation fire1;

	private Animation fire2;

	private bool startedGame;

	private bool upgrading;

	private bool showNewSkillOnReturn;

	private int swipeTouchID = -1;

	private Vector2 swipeTouchStart;

	private float targetSwipeLength;

	private float swipeLength;

	public ShopState(Character? initialCharacter = null, bool immediatePurchase = false)
	{
		gameOver = false;
		gameOver = (base.core.GetCurrentState() as MenuState)?.GameOver ?? false;
		base.core.AudioManager.PlayMusic("tribe");
		base.TransDuration = 20;
		IsOpaque = true;
		this.immediatePurchase = immediatePurchase;
		scrollAnim = 10;
		showGuide = true;
		pedestal = _(SpriteName.char_pedestal);
		columnTop = _(SpriteName.shop_column_top);
		column = _(SpriteName.shop_column);
		lockIcon = _(SpriteName.icon_lock);
		fire1 = new Animation();
		fire1.Add("burn", "menu_fire_", "1234");
		fire1.Play("burn");
		fire2 = new Animation();
		fire2.Add("burn", "menu_fire_", "1234");
		fire2.Play("burn");
		fire2.SkipToRandomFrame();
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 4);
		int num = base.core.Renderer.ScreenHeight / 3;
		float num2 = (float)(base.core.Renderer.ScreenWidth / 3) * 0.75f;
		int num3 = (int)base.core.Renderer.ScreenCenter.Y;
		touchMenu.SetupButton(Button.Left, new RectangleF(0f, num3 - num / 2 + 10 + base.topSafeArea, num2, num / 2), _(SpriteName.button_arrow_left), _(SpriteName.button_arrow_left_pressed), null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.none);
		touchMenu.SetupButton(Button.Right, new RectangleF((float)base.core.Renderer.ScreenWidth - num2, num3 - num / 2 + 10 + base.topSafeArea, num2, num / 2), _(SpriteName.button_arrow_left), _(SpriteName.button_arrow_left_pressed), null, stretch: false, SpriteFlip.Horizontal, ButtonColor.Orange, "", null, icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.none);
		int num4 = base.core.Renderer.ScreenHeight - 40;
		float num5 = (float)(base.core.Renderer.ScreenWidth - 22) / 4f;
		touchMenu.SetupButton(Button.GetCoins, new RectangleF(12f + num5 * 3f, num4 - 35, num5, 30f), null, null, null, stretch: false, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_cart_big));
		touchMenu.SetupButton(Button.XPromo, new RectangleF(10f, num4 - 35, num5, 30f), _(SpriteName.ppanic_button_up), _(SpriteName.ppanic_button_down));
		touchMenu.SetupButton(Button.Buy, new RectangleF(10f + num5 + 1f, num4 - 35, num5 * 2f, 30f), _(SpriteName.button_green), _(SpriteName.button_green_pressed), _(SpriteName.button_disabled), stretch: true);
		touchMenu.SetupButton(Button.Back, new RectangleF(10f, num4, num5, 30f), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_exit));
		touchMenu.SetupButton(Button.Play, new RectangleF(10f + num5 + 1f, num4, num5 * 2f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_play));
		touchMenu.SetupButton(Button.Share, new RectangleF(12f + num5 * 3f, num4, num5, 30f), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_camera), icon: true, iconIsPicture: false, blink: false, null, null, -3f, 0f, 1f, "", 0.095f, drawShadow: false, SoundName.button_down, SoundName.camera);
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			Button.Play, Button.Buy, Button.Back, Button.Left, Button.Right,
			Button.Share, Button.GetCoins, Button.XPromo);
		ChangeCurrentCharacter(initialCharacter ?? base.core.ProfileData.Character);
		touchMenu[Button.Share].Hidden = true;
		touchMenu[Button.GetCoins].Hidden = true;
		touchMenu[Button.XPromo].Hidden = true;
		touchMenu[Button.Play].Rectangle.Width += num5;
	}

	public override void Load()
	{
		Screen("shop");
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		if (immediatePurchase)
		{
			OnButtonRelease(Button.Buy);
		}
		base.Load();
	}

	public override void Update()
	{
		if (insufficientCoinsTicks > 0) insufficientCoinsTicks--;
		fire1.Update();
		fire2.Update();
		charShowingTicks++;
		if (scrollAnim < 10)
		{
			scrollAnim++;
			if (scrollAnim == 5)
			{
				ChangeCurrentCharacter(nextChar);
			}
		}
		swipeLength += (targetSwipeLength - swipeLength) * 0.3f;
		guideX += (guideTargetX - guideX) * 0.2f;
		if (guideTargetX > 0f)
		{
			guideTtl--;
			if (guideTtl == 0)
			{
				guideTargetX = -30f;
				nextGuideTimer = 350;
			}
		}
		if (nextGuideTimer > 0)
		{
			nextGuideTimer--;
			if (nextGuideTimer == 0)
			{
				guideTtl = 350;
				FindGuideTarget();
			}
		}
		if (skillsShown && skillAnim < 10)
		{
			skillAnim++;
		}
		if (!skillsShown && skillAnim > 0)
		{
			skillAnim--;
			if (skillAnim == 0)
			{
				shownSkill = null;
			}
		}
		if (abilitiesHud != null)
		{
			abilitiesHud.Update();
		}
		base.Update();
	}

	public override void OnReturn()
	{
		ChangeCurrentCharacter(CurrentCharacter);
		nextGuideTimer = 1;
		upgrading = false;
		if (showNewSkillOnReturn)
		{
			nextGuideTimer = 350;
			foreach (KeyValuePair<Skill, int> item in charDesc.Levels[charLevel - 1].Abilities.SkillLevel)
			{
				if ((item.Value != 0 && (charLevel == 1 || charDesc.Levels[charLevel - 2].Abilities.SkillLevel[item.Key] == 0)) || charDesc.Levels[charLevel - 1].Highlight == item.Key)
				{
					shownSkill = item.Key;
					AbilitiesHud.AbilityPanel abilityPanel = abilitiesHud.skillPanels[shownSkill.Value];
					shownPanelLeft = (int)abilityPanel.Left + 1;
					shownPanelRight = (int)(abilityPanel.Left + abilityPanel.Width - 2f);
					ShowSkills(show: true);
					break;
				}
			}
		}
		showNewSkillOnReturn = false;
		base.OnReturn();
	}

	public override void UpdateTransition()
	{
		touchMenu[Button.GetCoins].Rectangle.Shift((float)Tween.CubicEaseOut(base.Trans, 60.0, -60.0, base.TransDuration - 1));
		touchMenu[Button.Back].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(0, 4), 50.0, -50.0, base.TransDuration - 4));
		touchMenu[Button.Play].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(2, 4), 50.0, -50.0, base.TransDuration - 4));
		touchMenu[Button.Share].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(4, 4), 50.0, -50.0, base.TransDuration - 4 - 2));
		touchMenu[Button.XPromo].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(2, 4), 90.0, -90.0, base.TransDuration - 4));
		touchMenu[Button.Buy].Rectangle.Shift(0f, (float)Tween.BackEaseOut(TransD(2, 4), 90.0, -90.0, base.TransDuration - 4));
		touchMenu[Button.Left].Rectangle.Shift((float)Tween.BackEaseOut(TransD(2, 2), -50.0, 50.0, base.TransDuration - 2 - 2));
		touchMenu[Button.Right].Rectangle.Shift((float)Tween.BackEaseOut(base.Trans, 50.0, -50.0, base.TransDuration));
		base.UpdateTransition();
	}

	public override void HandleInput()
	{
		touchMenu[Button.GetCoins].Hidden = true;
		touchMenu[Button.XPromo].Hidden = true;
		bool flag = true;
		if (base.Trans >= base.TransDuration)
		{
			if (core.Input.WasPressed(GameAction.PreviousCategory)) { touchMenu.ActivateButton(Button.Left); return; }
			if (core.Input.WasPressed(GameAction.NextCategory)) { touchMenu.ActivateButton(Button.Right); return; }
			if (core.Input.WasPressed(GameAction.Secondary)) { touchMenu.ActivateButton(Button.Buy); return; }
			flag = !touchMenu.HandleInput();
			if (flag)
			{
				flag = !menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
			}
		}
		if (abilitiesHud != null)
		{
			Skill? skill = null;
			InjuryType? injuryType = null;
			foreach (TouchLocation item in base.core.TouchState)
			{
				if (item.State != TouchLocationState.Pressed)
				{
					continue;
				}
				foreach (KeyValuePair<Skill, AbilitiesHud.AbilityPanel> skillPanel in abilitiesHud.skillPanels)
				{
					if (skillPanel.Value.ContainsPoint(item.Position))
					{
						skill = skillPanel.Key;
						shownPanelLeft = (int)skillPanel.Value.Left + 1;
						shownPanelRight = (int)(skillPanel.Value.Left + skillPanel.Value.Width - 2f);
						flag = false;
					}
				}
			}
			if (skill.HasValue || injuryType.HasValue)
			{
				if (skill.HasValue && skill == shownSkill)
				{
					ShowSkills(show: false);
				}
				else
				{
					shownSkill = skill;
					ShowSkills(show: true);
				}
				flag = false;
			}
			else if (base.core.TouchState.Count > 0 && base.core.TouchState[0].State == TouchLocationState.Pressed)
			{
				ShowSkills(show: false);
				flag = false;
			}
		}
		bool flag2 = false;
		if (flag)
		{
			foreach (TouchLocation item2 in base.core.TouchState)
			{
				switch (item2.State)
				{
				case TouchLocationState.Moved:
				case TouchLocationState.Pressed:
					if (swipeTouchID < 0)
					{
						swipeTouchID = item2.Id;
						swipeTouchStart = item2.Position;
						flag2 = true;
					}
					else if (item2.Id == swipeTouchID)
					{
						targetSwipeLength = item2.Position.X - swipeTouchStart.X;
						if (Math.Abs(targetSwipeLength) < 10f)
						{
							targetSwipeLength = 0f;
						}
						flag2 = true;
					}
					break;
				case TouchLocationState.Released:
					if (item2.Id != swipeTouchID)
					{
						break;
					}
					swipeTouchID = -1;
					if (Math.Abs(targetSwipeLength) > 30f)
					{
						if (targetSwipeLength < 0f)
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
		if (!flag2 || skillsShown)
		{
			swipeTouchID = -1;
			targetSwipeLength = 0f;
		}
		base.HandleInput();
	}

	public void DrawCharPillar(Vector2 charCenter, float cTrans)
	{
		base.core.Renderer["fg", -1, false].DrawSpriteS(column, (charCenter - pedestal.Link).Shift((float)(pedestal.Width - columnTop.Width) * 0.5f, pedestal.Height - 2), charUnlocked ? Color.White : Color.DarkGray, new Vector2(1f, (float)base.core.Renderer.ScreenHeight - charCenter.Y));
		base.core.Renderer["fg", -1, false].DrawSpriteS(columnTop, (charCenter - pedestal.Link).Shift((float)(pedestal.Width - columnTop.Width) * 0.5f, pedestal.Height - 2), charUnlocked ? Color.White : Color.DarkGray);
		base.core.Renderer["fg", -1, false].DrawSpriteS(pedestal, charCenter - pedestal.Link, charUnlocked ? Color.White : Color.DarkGray);
		int count = charDesc.Levels.Count;
		for (int i = 0; i < count; i++)
		{
			Sprite sprite = ((charUnlocked && i <= charLevel - 1) ? _(SpriteName.char_level_star) : _(SpriteName.char_level_star_slot));
			base.core.Renderer["fg"].DrawSpriteS(sprite, charCenter.Shift(2f - (float)(10 * count) * 0.5f + (float)(10 * i), 10f), charUnlocked ? Color.White : Color.DarkGray);
		}
		if (charPortrait != null)
		{
			if (charDesc.DrawPortraitUnderExtra || !charUnlocked || !cTrans.IsEqualTo(0f))
			{
				base.core.Renderer["fg"].DrawSpriteS(charPortrait, charCenter.Shift(0f, startedGame ? (0f - cTrans) : 0f) - charPortrait.Link, charUnlocked ? Color.White : default(Color).FromRgb(328965));
			}
			if (charUnlocked)
			{
				base.core.Renderer.DrawPortraitExtra(CurrentCharacter, charUnlocked, charCenter, charDesc, charLevel, 0, 1f, startedGame ? (0f - cTrans) : 0f);
			}
			if (!charUnlocked)
			{
				base.core.Renderer["fg"].DrawSpriteS(lockIcon, charCenter.Shift(0f, (float)(-charPortrait.Height) * 0.5f) + new Vector2(Component._cos((float)base.ticks * 0.02f) * 2.5f, Component._sin((float)base.ticks * 0.025f)), null, null, Component._sin((float)base.ticks * 0.07f) * 0.1f, SpriteFlip.None, SpriteOrigin.TopCenter);
			}
		}
	}

	public override void Draw()
	{
		if (insufficientCoinsTicks > 0)
		{
			R["fg", 20000, false].DrawTextS(__(SId.PC_insufficient_coins), new Vector2(R.ScreenWidth / 2f, R.ScreenHeight - 75f),
				TextProfile.OrangeBoldText.Alter(width: R.ScreenWidth - 20, scale: 0.7f, boxAlignment: Alignment2D.Middle, textAlignment: Alignment2D.Middle, decoration: TextDecoration.Contour, secondColor: Color.Black));
		}
		touchMenu[Button.GetCoins].Hidden = true;
		touchMenu[Button.XPromo].Hidden = true;
		base.core.Renderer["fg", -10, false].FillScreen(Color.Black);
		if (!upgrading)
		{
			touchMenu.Draw();
		}
		if (IsTopState && Transition == TransType.None)
		{
			var prompts = new List<ContextPrompt>
			{
				new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_confirm)),
				new ContextPrompt(PromptAction.Cancel, __(SId.PROMPT_cancel))
			};
			if (!touchMenu[Button.Buy].Disabled)
				prompts.Add(new ContextPrompt(PromptAction.Secondary, __(charUnlocked ? SId.SHOP_uprgade : SId.SHOP_unlock)));
			if (touchMenu[Button.Share].Visible && !touchMenu[Button.Share].Disabled)
				prompts.Add(new ContextPrompt(PromptAction.Screenshot, "Screenshot"));
			ContextPromptLegend.DrawVertical(R, new Vector2(R.ScreenWidth - 130f, R.ScreenHeight - 135f), prompts.ToArray());
			if (!touchMenu[Button.Left].Disabled)
				ContextPromptLegend.DrawButton(R, touchMenu[Button.Left].Rectangle.Center + new Vector2(-8f, -30f), PromptAction.PreviousCategory);
			if (!touchMenu[Button.Right].Disabled)
				ContextPromptLegend.DrawButton(R, touchMenu[Button.Right].Rectangle.Center + new Vector2(-8f, -30f), PromptAction.NextCategory);
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = (float)base.Trans / (float)base.TransDuration;
		if (Transition != TransType.None)
		{
			num = (float)Tween.BackEaseOut(base.Trans, -70.0, 70.0, base.TransDuration);
			num2 = (float)Tween.CircEaseOut(base.Trans, base.core.Renderer.ScreenHeight, -base.core.Renderer.ScreenHeight, base.TransDuration);
		}
		float num4 = 0f;
		if (scrollAnim < 10)
		{
			float num5 = 0f;
			num5 = ((scrollAnim < 5) ? ((float)Tween.Linear(scrollAnim % 5, 0.0, 1.0, 5.0)) : ((float)Tween.Linear(scrollAnim % 5, 0.0, 1.0, 5.0)));
			float num6 = (float)base.core.Renderer.ScreenWidth * 0.5f + 40f;
			num4 = (float)scrollDir * num6 * num5 + ((scrollAnim < 5) ? 0f : ((float)(-scrollDir) * num6));
		}
		num4 += swipeLength * 0.5f;
		touchMenu[Button.Buy].Rectangle.Shift(num4 * 1.5f);
		touchMenu[Button.XPromo].Rectangle.Shift(num4 * 1.5f);
		Vector2 v = base.core.Renderer.ScreenCenter.Shift(0f, (float)base.core.Renderer.ScreenHeight * 0.06f + (float)base.topSafeArea);
		Vector2 v2 = new Vector2(num4 + v.X, (float)base.core.Renderer.ScreenHeight * 0.165f + num + (float)base.topSafeArea);
		Vector2 charCenter = v.Shift(0f, -2f + num2);
		if (!base.core.TakingScreenshot)
		{
			charCenter += new Vector2(num4 + Component._cos((float)base.ticks * 0.02f) * 2.5f, Component._sin((float)base.ticks * 0.025f)) * num3;
		}
		Vector2 vector = v.Shift(0f, -35f).Shift((0f - Component._cos((float)base.ticks * 0.02f)) * 2.5f * 0.5f, (0f - Component._sin((float)base.ticks * 0.025f)) * 0.5f);
		base.core.Renderer["fg", -4, false].DrawSpriteS(_(SpriteName.shop_wall), vector, Color.White * num3, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		Vector2 v3 = vector.Clone();
		v3.Y -= 20f;
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.menu_torch), v3.Shift(35f, 11f), Color.White * num3, new Vector2(0.75f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(fire1.GetCurrentFrame(), v3.Shift(35f, 0f), Color.White * num3, new Vector2(0.75f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.glow_huge), v3.Shift(35f, 0f), scale: new Vector2(0.75f), tint: Color.Orange * (0.4f + Component._sin((float)base.ticks * 0.05f) * 0.1f) * num3, rotation: 0f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.menu_torch), v3.Shift(-35f, 11f), Color.White * num3, new Vector2(0.75f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(fire2.GetCurrentFrame(), v3.Shift(-35f, 0f), Color.White * num3, new Vector2(0.75f), 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.glow_huge), v3.Shift(-35f, 0f), scale: new Vector2(0.75f), tint: Color.Orange * (0.4f + Component._cos((float)base.ticks * 0.05f) * 0.1f) * num3, rotation: 0f, flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.glow_huge), v.Shift(num4, -38f).Shift(Component._cos((float)base.ticks * 0.07f) * 2.5f, Component._sin((float)base.ticks * 0.08f)), charDesc.Color1 * (0.8f + Component._sin((float)base.ticks * 0.035f) * 0.2f) * (charUnlocked ? charDesc.BacklightDim : 1f) * num3, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.glow_huge), v.Shift(num4, -38f).Shift(Component._sin((float)base.ticks * 0.05f) * 2.5f, Component._cos((float)base.ticks * 0.06f)), charDesc.Color2 * (0.85f + Component._cos((float)base.ticks * 0.05f) * 0.15f) * (charUnlocked ? charDesc.BacklightDim : 1f) * num3, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		DrawCharPillar(charCenter, num2);
		if (!charUnlocked)
		{
			base.core.Renderer["fg"].DrawTextS(charNameLabel, v2.Shift(num4 * 1.5f, -5f), TextProfile.OrangeBoldText.Alter(boxAlignment: Alignment2D.BottomCenter, textAlignment: Alignment2D.BottomCenter, width: 200, height: 25, color: default(Color).FromRgb(5859700), secondColor: null, decoration: TextDecoration.None));
			base.core.Renderer["fg"].DrawTextS(__(charDesc.Bio), v2.Shift(num4 * 1.5f, 5f), TextProfile.GravestoneText.Alter(boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Middle, width: base.core.Renderer.ScreenWidth - 20, height: (int)((float)(base.core.Renderer.ScreenHeight - 41) - (touchMenu[Button.Buy].Rectangle.CenterTop.Y + 30f)), color: default(Color).FromRgb(5463138), secondColor: null, decoration: null, font: null, scale: 0.75f));
		}
		else
		{
			abilitiesHud.Top = v2.Y + 5f;
			abilitiesHud.dLeft = num4 * 1.5f;
			base.core.Renderer["fg"].DrawSpriteS(charNameImage, v2.Shift(0f, 3f), null, null, 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			if (abilitiesHud.skillPanels.Count == 0)
			{
				base.core.Renderer["fg"].DrawTextS(__(SId.SHOP_upgrade_to_learn), v2.Shift(-2f + num4 * 1.5f, 7f), TextProfile.OrangeBoldText.Alter(font: Font.Thin, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, width: base.core.Renderer.ScreenWidth - 10, height: 30, color: default(Color).FromRgb(5463138), secondColor: null, decoration: TextDecoration.None, scale: 0.75f));
			}
		}
		if (skillsShown || skillAnim > 0)
		{
			float num7 = (float)base.core.Renderer.ScreenWidth * 0.7f;
			float num8 = base.core.Renderer.ScreenCenter.X - num7 / 2f;
			if ((float)shownPanelLeft < num8)
			{
				num8 = shownPanelLeft;
			}
			if ((float)shownPanelRight > num8 + num7)
			{
				num8 = (float)shownPanelRight - num7;
			}
			float num9 = v2.Y + 31f;
			float height = 40f;
			RectangleF rectangleF = new RectangleF(num8, num9 - 5f * (float)(10 - skillAnim) / 10f, num7, height);
			float num10 = (float)skillAnim / 10f;
			Vector2 vector2 = rectangleF.CenterTop.Shift(0f, 3f);
			if (charUnlocked)
			{
				AbilityDesc abilityDesc = null;
				if (shownSkill.HasValue)
				{
					abilityDesc = Abilities.SkillDesc[shownSkill.Value];
				}
				if (abilityDesc != null)
				{
					height = base.core.Renderer["fg", 11, false].DrawTextS(__(abilityDesc.Name), vector2, TextProfile.OrangeBoldText.Alter(textAlignment: Alignment2D.Center, boxAlignment: Alignment2D.Center, width: (int)num7 - 14, decoration: TextDecoration.Extrude1, color: default(Color).FromRgb(15967806) * num10, secondColor: default(Color).FromRgb(3939629) * num10, height: null, font: null, scale: 1f / Settings.GuiScale)).Height;
					vector2 = vector2.Shift(0f, height + 5f);
					TextProfile textProfile = TextProfile.GravestoneText.Alter(font: Font.Thin, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, color: default(Color).FromRgb(11629924) * num10, decoration: TextDecoration.Extrude1, secondColor: default(Color).FromRgb(3939629) * num10, width: (int)num7 - 14, height: null, scale: 0.75f / Settings.GuiScale);
					int num11 = charDesc.Levels[charLevel - 1].Abilities.SkillLevel[shownSkill.Value];
					string text = string.Format(__(abilityDesc.Description), num11);
					height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
					vector2 = vector2.Shift(0f, height + 8f);
					if (abilityDesc.Illustration.HasValue)
					{
						Sprite sprite = _(abilityDesc.Illustration.Value);
						base.core.Renderer["fg", 11, false].DrawSpriteS(sprite, vector2, Color.White * num10, Vector2.One * 0.75f / Settings.GuiScale, 0f, SpriteFlip.None, SpriteOrigin.TopCenter);
						vector2 = vector2.Shift(0f, (float)sprite.Height * 0.75f + 3f);
					}
					textProfile = textProfile.Alter(font: Font.Bold, color: default(Color).FromRgb(13659170) * num10, secondColor: null, decoration: TextDecoration.None);
					switch (abilityDesc.Kind)
					{
					case AbilityKind.Consumable:
						text = string.Format(__(SId.SKILL_x_per_run), num11);
						height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
						vector2 = vector2.Shift(0f, height + 7f);
						break;
					case AbilityKind.Permanent:
						text = __(SId.SKILL_permanent_effect);
						switch (shownSkill)
						{
						case Skill.CoinMagnetRadius:
							text = __(SId.SKILL_radius) + ": " + num11 + __(SId.MISC_meters);
							break;
						case Skill.Petrification:
							text = __(SId.SKILL_speed) + ": " + ((num11 == 1) ? __(SId.SKILL_speed_low) : __(SId.SKILL_speed_high));
							break;
						case Skill.FireShield:
						case Skill.BetterFireShield:
							text = __(SId.SKILL_charge_10_fireballs);
							break;
						case Skill.Parrot:
							text = __(SId.SKILL_duration) + ": 10 " + __(SId.MISC_seconds_long);
							break;
						case Skill.Electronic:
							text = __(SId.SKILL_battery_life) + ": " + num11 + " " + __(SId.MISC_seconds_long);
							break;
						}
						height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
						vector2 = vector2.Shift(0f, height + 7f);
						if (shownSkill == Skill.BetterFireShield)
						{
							vector2 = vector2.Shift(0f, -7f);
							text = __(SId.SKILL_active_at_start);
							height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
							vector2 = vector2.Shift(0f, height + 7f);
						}
						break;
					case AbilityKind.Rechargeable:
					{
						switch (shownSkill)
						{
						case Skill.PanicLaser:
						case Skill.Gunshot:
							text = __(SId.SKILL_tap_to_shoot);
							break;
						default:
							text = __(SId.SKILL_tap_when_charged);
							break;
						}
						height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2.Shift(-3f, 0f), textProfile.Alter(null, null, null, null, null, null, null, Font.Thin)).Height;
						vector2 = vector2.Shift(0f, height + 7f);
						Color color = default(Color).FromRgb(abilityDesc.Color1);
						Color color2 = default(Color).FromRgb(abilityDesc.Color2);
						Color color3 = default(Color).FromRgb(abilityDesc.Color3);
						Vector2 position = vector2.Shift(-14f, -3f);
						base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c1), position, color * num10);
						base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c2), position, color2 * num10);
						base.core.Renderer["fg", 12, false].DrawSpriteS(_(SpriteName.skill_button_up_c3), position, color3 * num10);
						base.core.Renderer["fg", 12, false].DrawSpriteS(_(abilityDesc.HudMainIcon.Value), vector2.Shift(-1f, 11f), Color.White * num10, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
						vector2 = vector2.Shift(0f, 30f);
						if (shownSkill == Skill.Flight)
						{
							text = __(SId.SKILL_duration) + ": " + ((num11 > 15) ? 2 : 5) + " " + __(SId.MISC_seconds_long);
							height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
							vector2 = vector2.Shift(0f, height);
						}
						if (shownSkill == Skill.FullMoon)
						{
							text = __(SId.SKILL_duration) + ": " + (num11 > 15 ? 3 : 5) + " " + __(SId.MISC_seconds_long);
							height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
							vector2 = vector2.Shift(0f, height);
						}
						if (shownSkill == Skill.SloMo)
						{
							text = __(SId.SKILL_duration) + ": " + ((num11 > 15) ? 2 : 5) + " " + __(SId.MISC_seconds_long);
							height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
							vector2 = vector2.Shift(0f, height);
						}
						switch (shownSkill)
						{
						case Skill.PanicLaser:
							text = __(SId.SKILL_barrery_impact) + ": " + num11 + "%";
							break;
						case Skill.Blaze:
							text = __(SId.SKILL_consumes_active_shield);
							break;
						case Skill.Gunshot:
							text = string.Format(__(SId.SKILL_shot_price), 15);
							if (num11 == 2)
							{
								text = text + "\n" + string.Format(__(SId.SKILL_kill_reward), 15);
							}
							if (num11 == 3)
							{
								text = text + "\n" + string.Format(__(SId.SKILL_kill_reward), 20);
							}
							break;
						default:
							text = __(SId.SKILL_recharge) + ": " + num11 + " " + __(SId.MISC_seconds_long);
							break;
						}
						height = base.core.Renderer["fg", 11, false].DrawTextS(text, vector2, textProfile).Height;
						vector2 = vector2.Shift(0f, height + 7f);
						break;
					}
					}
				}
			}
			rectangleF.Height = Component._M(vector2.Y - num9, 7f) + 3f;
			RectangleF rectangleF2 = rectangleF.Clone();
			rectangleF2.X = 0f;
			rectangleF2.Width = base.core.Renderer.ScreenWidth;
			rectangleF2.Height = base.core.Renderer.ScreenHeight;
			rectangleF2.Y -= 25f;
			base.core.Renderer["fg", 6, false].DrawRectangleS(rectangleF2, Color.Black * 0.8f * ((float)skillAnim / 10f));
			base.core.Renderer["fg", 10, false].DrawWoodenPanel(rectangleF, num10);
		}
		if (charUnlocked && abilitiesHud != null)
		{
			abilitiesHud.Draw();
		}
		if (guideX > -25f && !upgrading)
		{
			Sprite sprite2 = ((Component._sin((float)base.ticks * 0.07f) > 0.5f) ? _(SpriteName.cursor_hand_touch) : _(SpriteName.cursor_hand));
			base.core.Renderer["fg", 15, false].DrawSpriteS(_(SpriteName.glow_big), new Vector2(guideX + 5f + num4 * 1.5f, v2.Y + 13f + 17f), Color.Black * 0.7f, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 15, false].DrawSpriteS(sprite2, new Vector2(guideX + 10f - Component._m(0.5f, Component._sin((float)base.ticks * 0.07f)) * 2f + num4 * 1.5f, v2.Y + 13f + 12f), null, null, (0f - Component._m(0.5f, Component._sin((float)base.ticks * 0.07f))) * 0.05f, SpriteFlip.None, SpriteOrigin.Center);
			base.core.Renderer["fg", 15, false].DrawSpriteS(_(SpriteName.cursor_question), new Vector2(guideX + 14f - Component._m(0.5f, Component._sin((float)base.ticks * 0.07f)) + num4 * 1.5f, v2.Y + 13f + 13f));
		}
		if (touchMenu[Button.Share].Visible && charUnlocked && base.ticks % 300 <= 50 && !upgrading)
		{
			base.core.Renderer["fg", 5, false].DrawSpriteS(_(SpriteName.camera_flash), touchMenu[Button.Share].Rectangle.TopRight.Shift(-11.5f, 10f + (float)(touchMenu[Button.Share].IsDown ? 3 : 0)), null, rotation: (float)(-base.ticks) * 0.08f, scale: Vector2.One * Component._sin((float)(base.ticks % 200) / 50f * (float)Math.PI), flip: SpriteFlip.None, origin: SpriteOrigin.Center);
		}
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		int currentCharacter = (int)CurrentCharacter;
		int length = Enum.GetValues(typeof(Character)).Length;
		switch (button)
		{
		case Button.Left:
			if (scrollAnim == 10)
			{
				currentCharacter = ((currentCharacter == 0) ? (length - 1) : (currentCharacter - 1));
				nextChar = (Character)currentCharacter;
				scrollAnim = 0;
				scrollDir = 1;
				SendMessage(new PlaySoundMessage(SoundName.swoosh_1, 0.7f));
			}
			break;
		case Button.Right:
			if (scrollAnim == 10)
			{
				currentCharacter = ((currentCharacter != length - 1) ? (currentCharacter + 1) : 0);
				nextChar = (Character)currentCharacter;
				scrollAnim = 0;
				scrollDir = -1;
				SendMessage(new PlaySoundMessage(SoundName.swoosh_1, 0.7f));
			}
			break;
		case Button.Buy:
			if (enoughCoins)
			{
				if (!charUnlocked)
				{
					base.core.ProfileData.Coins -= charDesc.UnlockPrice;
					base.core.ProfileData.Characters[CurrentCharacter].Unlocked = true;
					showNewSkillOnReturn = true;
					SendMessage(new PushStateMessage(new NewCharState(CurrentCharacter)));
					ChangeCurrentCharacter(CurrentCharacter);
					ApplyCharacterSelection();
					Event(AnalyticsCategory.Shop, "unlock-character", CurrentCharacter.ToString(), base.core.ProfileData.Coins + charDesc.UnlockPrice);
					base.core.Cloud.Sync();
				}
				else if (!maxLevel)
				{
					base.core.ProfileData.Coins -= upgradePrice;
					base.core.ProfileData.Characters[CurrentCharacter].Level++;
					ApplyCharacterSelection();
					upgrading = true;
					showNewSkillOnReturn = true;
					SendMessage(new PushStateMessage(new UpgradeState(this)));
					Event(AnalyticsCategory.Shop, "upgrade-character", CurrentCharacter.ToString(), base.core.ProfileData.Coins + upgradePrice);
					base.core.Cloud.Sync();
				}
			}
			else
			{
				insufficientCoinsTicks = 150;
			}
			break;
		case Button.Play:
			if (charUnlocked)
			{
				ApplyCharacterSelection();
				base.core.NextRunFromShop();
				TransitionOut(CoreEvent.ResetAndStartGame);
				SendMessage(new PlaySoundMessage(SoundName.trans_2), 20);
				startedGame = true;
			}
			break;
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
	}

	private void ApplyCharacterSelection()
	{
		base.core.ProfileData.Character = CurrentCharacter;
		base.core.ProfileData.SaveIntoStorage();
	}

#if DEBUG
	internal void BuyForQa() => OnButtonRelease(Button.Buy);
	internal void PlayForQa() => OnButtonRelease(Button.Play);
#endif

	private void ShowSkills(bool show)
	{
		showGuide = !show;
		guideTargetX = -30f;
		nextGuideTimer = 350;
		skillsShown = show;
		touchMenu[Button.Buy].Disabled = show || (charUnlocked && maxLevel);
		touchMenu[Button.Left].Disabled = show;
		touchMenu[Button.Right].Disabled = show;
		touchMenu[Button.GetCoins].Disabled = show;
		touchMenu[Button.Back].Disabled = show;
		touchMenu[Button.Share].Disabled = show || !charUnlocked;
		touchMenu[Button.Play].Disabled = show || !charUnlocked;
		if (abilitiesHud != null)
		{
			if (!show)
			{
				abilitiesHud.SelectedSkill = null;
			}
			else
			{
				abilitiesHud.SelectedSkill = shownSkill;
			}
		}
	}

	public void ChangeCurrentCharacter(Character character)
	{
		if (CurrentCharacter != character)
		{
			charShowingTicks = 0;
		}
		CurrentCharacter = character;
		charDesc = CharDescription.Get[character];
		charPortrait = _(charDesc.Portrait);
		charNameImage = _(charDesc.NameImage + "_" + Locale.ShortName[base.core.LocaleManager.CurrentLocale], charDesc.NameImage);
		charNameLabel = __(charDesc.Name);
		charPriceLabel = "^" + charDesc.UnlockPrice;
		charUnlocked = base.core.ProfileData.Characters[CurrentCharacter].Unlocked;
		charLevel = base.core.ProfileData.Characters[CurrentCharacter].Level;
		maxLevel = charLevel == charDesc.Levels.Count;
		if (charUnlocked && !maxLevel)
		{
			upgradePrice = charDesc.Levels[charLevel].Price;
		}
		if (!charUnlocked)
		{
			enoughCoins = base.core.ProfileData.Coins >= charDesc.UnlockPrice;
		}
		else if (!maxLevel)
		{
			enoughCoins = base.core.ProfileData.Coins >= upgradePrice;
		}
		abilitiesHud = null;
		if (charUnlocked)
		{
			if (guideTtl > 0)
			{
				guideTtl = 350;
			}
			abilitiesHud = new AbilitiesHud(charDesc.Levels[charLevel - 1].Abilities.SkillLevel, shopMode: true);
			abilitiesHud.Update();
		}
		FindGuideTarget();
		touchMenu[Button.Play].Disabled = !charUnlocked;
		touchMenu[Button.Share].Disabled = !charUnlocked;
		touchMenu[Button.XPromo].Visible = character == Character.PanicBot && charUnlocked;
		TouchMenu<Button>.ButtonDesc buttonDesc = touchMenu[Button.Buy];
		buttonDesc.Color = ((!enoughCoins) ? ButtonColor.Orange : ButtonColor.Green);
		if (enoughCoins)
		{
			buttonDesc.Sprite = _(SpriteName.button_green);
			buttonDesc.PressedSprite = _(SpriteName.button_green_pressed);
		}
		else
		{
			buttonDesc.Sprite = _(SpriteName.button);
			buttonDesc.PressedSprite = _(SpriteName.button_pressed);
		}
		buttonDesc.Label = ((!charUnlocked) ? (__(SId.SHOP_unlock) + " " + charPriceLabel) : ((!maxLevel) ? (__(SId.SHOP_uprgade) + " ^" + upgradePrice) : __(SId.SHOP_max_level)));
		buttonDesc.Disabled = charUnlocked && maxLevel;
		buttonDesc.Init();
		ShowSkills(skillsShown && charUnlocked);
	}

	private void FindGuideTarget()
	{
		if (charUnlocked)
		{
			if (showGuide)
			{
				AbilitiesHud.AbilityPanel abilityPanel = null;
				if (abilitiesHud.skillPanels.Count > 0)
				{
					abilityPanel = abilitiesHud.skillPanels.First().Value;
				}
				if (abilityPanel != null)
				{
					guideTargetX = abilityPanel.Left + abilityPanel.Width * 0.5f;
				}
				else
				{
					guideTargetX = -30f;
				}
			}
		}
		else
		{
			guideTargetX = -30f;
		}
	}

	public override void OnBackButtonPressed()
	{
		if (skillsShown)
		{
			ShowSkills(show: false);
			return;
		}
		SendMessage(new PlaySoundMessage(SoundName.trans_1));
		if (gameOver)
		{
			TransitionOut(CoreEvent.PopState);
		}
		else
		{
			TransitionOut(CoreEvent.ResetGame);
		}
		base.OnBackButtonPressed();
	}
}
