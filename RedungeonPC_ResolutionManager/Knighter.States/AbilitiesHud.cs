using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class AbilitiesHud : Component
{
	public class AbilityPanel : Component
	{
		public Skill? PanelSkill;

		public AbilitiesHud parent;

		public AbilityDesc Ability;

		public int MaxLevel;

		public int CurrentLevel;

		public float CurrentCharge;

		public float DemoCharge;

		private bool DemoBack;

		private Sprite mainIcon;

		private Sprite itemIcon;

		private Sprite itemSlot;

		private Sprite barLeft;

		private Sprite barMiddle;

		private Sprite barRight;

		public bool Hidden;

		public float Left;

		private float targetLeft;

		public bool CompactMode;

		public float addedWidth;

		private bool dropToken;

		private bool afterDropToken;

		private float dropTokenX;

		private int shake;

		public string Text = "";

		public float TargetLeft
		{
			get
			{
				return targetLeft;
			}
			set
			{
				targetLeft = value;
				if (parent.ShopMode)
				{
					Left = value;
				}
			}
		}

		public float Width => ModeWidth(CompactMode);

		public float ModeWidth(bool compact)
		{
			float num = 0f;
			num = ((mainIcon == null) ? (num + 6f) : (num + (float)(3 + mainIcon.Width + 3)));
			switch (Ability.Kind)
			{
			case AbilityKind.Consumable:
				num = (compact ? (num + (float)(itemIcon.Width + 20)) : (num + (float)(MaxLevel * (itemIcon.Width + 2) + 5)));
				break;
			case AbilityKind.Rechargeable:
				num = ((!parent.ShopMode) ? 0f : (Ability.HideChargeBar ? (num + 6f) : (num + 30f + 9f)));
				break;
			case AbilityKind.Permanent:
			{
				Skill? panelSkill = PanelSkill;
				if (panelSkill.HasValue)
				{
					Skill valueOrDefault = panelSkill.GetValueOrDefault();
					if (valueOrDefault == Skill.CoinMagnetRadius)
					{
						num = 41f;
						break;
					}
				}
				num += 3f;
				break;
			}
			}
			return num + addedWidth;
		}

		public void Init()
		{
			if (Ability.HudMainIcon.HasValue)
			{
				mainIcon = _(Ability.HudMainIcon.Value);
			}
			if (Ability.HudItemIcon.HasValue)
			{
				itemIcon = _(Ability.HudItemIcon.Value);
			}
			if (Ability.HudItemSlot.HasValue)
			{
				itemSlot = _(Ability.HudItemSlot.Value);
			}
			if (Ability.HudChargeBar.HasValue)
			{
				barLeft = _(Ability.HudChargeBar.Value);
				barMiddle = barLeft.Reduce(3, 0, 2, 0);
				barRight = barLeft.Reduce(5, 0, 0, 0);
				barLeft = barLeft.Reduce(0, 0, 5, 0);
			}
		}

		public void UpdateCurrentLevel(int newLevel)
		{
			if (newLevel < CurrentLevel)
			{
				dropToken = true;
			}
			var _ = CurrentLevel;
			CurrentLevel = newLevel;
			Hidden = CurrentLevel == 0;
		}

		public void UpdateCurrentCharge(float newCharge)
		{
			CurrentCharge = newCharge;
		}

		public override void Update()
		{
			if (shake > 0)
			{
				shake--;
			}
			if (Ability.Kind == AbilityKind.Rechargeable && CurrentLevel != 0)
			{
				if (DemoCharge < 0.3f)
				{
					DemoCharge = 0.3f;
				}
				if (!DemoBack)
				{
					int num = 60;
					float num2 = 1f / (float)(CurrentLevel * num);
					DemoCharge = Component._m(1f, DemoCharge + num2);
					if (DemoCharge >= 1f)
					{
						DemoBack = true;
					}
				}
				else
				{
					DemoCharge *= 0.95f;
					if (DemoCharge < 0.3f)
					{
						DemoBack = false;
					}
				}
			}
			if (!parent.ShopMode)
			{
				Left += (TargetLeft - Left) * 0.07f;
				afterDropToken = false;
				if (dropToken)
				{
					dropToken = false;
					if (Ability == Abilities.SkillDesc[Skill.SpareSkull])
					{
						base.core.ParticleManager.AddEmitter(inWorld: true, new Vector2(dropTokenX, parent.Top).Shift(0f, (float)itemIcon.Height / 2f + 15f)).OnSpawn(delegate(Particle p)
						{
							p.Position += base.core.CurrentPlayState.Camera.Position;
						}).OnUpdate(delegate(Particle p)
						{
							p.Velocity = p.Position;
							Vector2 worldCenter = base.core.CurrentPlayState.Player.WorldCenter;
							p.Position += (worldCenter - p.Position) * ((float)p.Age / 70f) * ((float)p.Age / 70f);
							p.Velocity += (p.Velocity - p.Position) * 5f;
							p.Dead = p.Age > 70;
						})
							.OnDraw(delegate(Particle p)
							{
								base.core.Renderer["fg", 1002, false].DrawSpriteW(_(SpriteName.rib_skull_glow), p.Position.Shift(0f, -15f + 8f * (Component._M(p.Age - 60, 0f) / 10f)), null, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
								base.core.Renderer["fg", 1001, false].DrawLineW(p.Position.Shift(-1f, -15f), p.Velocity.Shift(-1f, -15f), default(Color).FromRgb(8439569));
								base.core.Renderer["fg", 1001, false].DrawLineW(p.Position.Shift(1f, -15f), p.Velocity.Shift(1f, -15f), default(Color).FromRgb(8439569));
							})
							.Emit(1);
					}
					else
					{
						base.core.ParticleManager.AddEmitter(inWorld: false, new Vector2(dropTokenX, parent.Top).Shift(itemIcon.Width / 2, itemIcon.Height / 2)).OnUpdate(delegate(Particle p)
						{
							p.Position += new Vector2(0f, 0.4f);
							p.Dead = p.Age == 50;
						}).OnDraw(delegate(Particle p)
						{
							base.core.Renderer["fg", 1002, false].DrawSpriteS(itemIcon, p.Position, Color.White * ((float)(50 - p.Age) / 50f), null, (float)p.Age / 20f, SpriteFlip.None, SpriteOrigin.Center);
						})
							.Emit(1);
					}
					afterDropToken = true;
				}
			}
			base.Update();
		}

		public override void Draw()
		{
			if (Ability.Kind == AbilityKind.Rechargeable && !parent.ShopMode)
			{
				return;
			}
			Vector2 vector = new Vector2(0f);
			if (shake > 0)
			{
				vector.Y = Math.Abs(Component._sin((float)Math.PI * 3f * (float)shake / 60f) * (float)shake * 0.75f);
			}
			int num = (parent.ShopMode ? 10 : 1000);
			int num2;
			int num3;
			if (parent.ShopMode)
			{
				num2 = (parent.SelectedSkill.HasValue ? 1 : 0);
				if (num2 != 0)
				{
					num3 = ((parent.SelectedSkill == PanelSkill) ? 1 : 0);
					goto IL_00d8;
				}
			}
			else
			{
				num2 = 0;
			}
			num3 = 0;
			goto IL_00d8;
			IL_00d8:
			bool flag = (byte)num3 != 0;
			float num4 = Left + vector.X;
			if (!Hidden && !base.core.GetCurrentState().IsOverlay)
			{
				num4 += (float)Tween.BackEaseOut(base.core.GetCurrentState().Trans, -100.0, 100.0, base.core.GetCurrentState().TransDuration);
			}
			num4 += parent.dLeft;
			float num5 = num4;
			float num6 = parent.Top + vector.Y;
			float width = Width;
			if (num2 != 0 && !flag)
			{
				base.core.Renderer["fg", num + 3, false].DrawRectangleS(new RectangleF(num5, num6, width, 23f), Color.Black * 0.5f);
			}
			if (!parent.ShopMode)
			{
				base.core.Renderer["fg", num + 1, false].DrawRectangleS(new RectangleF(num5, num6 + (float)parent.panelEdge.Height - 2f, width - 2f, 5f), Color.Black * 0.3f);
			}
			base.core.Renderer["fg", num + 1, false].DrawSpriteS(parent.panelEdge, new Vector2(num5, num6));
			num5 += (float)(parent.panelEdge.Width - 1);
			base.core.Renderer["fg", num + 1, false].DrawSpriteS(parent.panelMiddle, new Vector2(num5, num6), null, new Vector2(width - (float)(parent.panelEdge.Width * 2), 1f));
			num5 = num4 + width - (float)parent.panelEdge.Width - 1f;
			base.core.Renderer["fg", num + 1, false].DrawSpriteS(parent.panelEdge, new Vector2(num5, num6), null, null, 0f, SpriteFlip.Horizontal);
			num5 = num4;
			if (mainIcon != null)
			{
				num5 += (float)(3 + (Ability.HideChargeBar ? 2 : 0));
				base.core.Renderer["fg", num + 1, false].DrawSpriteS(mainIcon, new Vector2(num5, num6));
				num5 += (float)mainIcon.Width;
				num5 += 3f;
			}
			else
			{
				num5 += 6f;
			}
			switch (Ability.Kind)
			{
			case AbilityKind.Consumable:
				if (!CompactMode)
				{
					for (int i = 0; i < MaxLevel; i++)
					{
						if (i == CurrentLevel - 1)
						{
							dropTokenX = num5;
						}
						Sprite sprite2 = itemSlot;
						if (i < CurrentLevel || (afterDropToken && i == CurrentLevel))
						{
							sprite2 = itemIcon;
						}
						base.core.Renderer["fg", num + 1, false].DrawSpriteS(sprite2, new Vector2(num5, num6));
						num5 += (float)(itemIcon.Width + 2);
					}
				}
				else
				{
					num5 = (dropTokenX = num5 - 1f);
					base.core.Renderer["fg", num + 1, false].DrawSpriteS(itemIcon, new Vector2(num5, num6));
					num5 += (float)(itemIcon.Width + 1);
					base.core.Renderer["fg", num + 1, false].DrawTextS("×" + CurrentLevel, new Vector2(num5, num6 + 3f), TextProfile.OrangeBoldText.Alter(textAlignment: Alignment2D.Left, boxAlignment: Alignment2D.Left, decoration: TextDecoration.Extrude1, color: default(Color).FromRgb(15967806), secondColor: default(Color).FromRgb(3939629)));
				}
				break;
			case AbilityKind.Rechargeable:
				if (parent.ShopMode && !Ability.HideChargeBar)
				{
					float num7 = (parent.ShopMode ? DemoCharge : CurrentCharge);
					Sprite sprite = _(SpriteName.skill_charge_track);
					base.core.Renderer["fg", num + 1, false].DrawSpriteS(sprite, new Vector2(num5, num6 + 6f));
					float num8 = (float)(sprite.Width - 2 - 2) * num7;
					Color white = Color.White;
					base.core.Renderer["fg", num + 1, false].DrawSpriteS(barLeft, new Vector2(num5 + 1f, num6 + 7f), white);
					base.core.Renderer["fg", num + 1, false].DrawSpriteS(barMiddle, new Vector2(num5 + 2f, num6 + 7f), scale: new Vector2(num8, 1f), tint: white);
					base.core.Renderer["fg", num + 1, false].DrawSpriteS(barRight, new Vector2(num5 + 2f + num8, num6 + 7f), white);
				}
				break;
			case AbilityKind.Permanent:
				switch (PanelSkill)
				{
				case Skill.CoinMagnetRadius:
					base.core.Renderer["fg", num + 1, false].DrawTextS(CurrentLevel + __(SId.MISC_meters), new Vector2(num5 - 10f, num6 + 3.5f), TextProfile.OrangeBoldText.Alter(font: Font.Thin, textAlignment: Alignment2D.Left, boxAlignment: Alignment2D.Left, decoration: TextDecoration.Extrude1, color: default(Color).FromRgb(15967806), secondColor: default(Color).FromRgb(3939629)));
					break;
				case Skill.TreasureHunt:
					if (!parent.ShopMode)
					{
						addedWidth = base.core.Renderer["fg", num + 1, false].DrawTextS(Text, new Vector2(num5 - 2f, num6 + 3.5f), TextProfile.OrangeBoldText.Alter(font: Font.Bold, textAlignment: Alignment2D.Left, boxAlignment: Alignment2D.Left, decoration: TextDecoration.Extrude1, width: 50, color: default(Color).FromRgb(15967806), secondColor: default(Color).FromRgb(3939629))).Width;
					}
					break;
				}
				break;
			}
			base.Draw();
		}

		public void Shake(string message = "")
		{
			if (shake == 0)
			{
				shake = 60;
			}
		}

		public bool ContainsPoint(Vector2 point)
		{
			return new RectangleF(Left, parent.Top, Width, 25f).Contains(point);
		}
	}

	public float Top;

	public float dLeft;

	public Skill? SelectedSkill;

	private readonly Dictionary<Skill, int> skillLevel;

	public readonly Dictionary<Skill, AbilityPanel> skillPanels;

	public Sprite panelEdge;

	public Sprite panelMiddle;

	public Sprite panelBottom;

	public Sprite panelOrnament;

	public readonly Skill ActiveSkill;

	public bool HasActiveSkill;

	public bool ShopMode { get; private set; }

	public AbilitiesHud(Dictionary<Skill, int> skillLevel, bool shopMode = false, float top = 18f)
	{
		ShopMode = shopMode;
		Top = top;
		this.skillLevel = skillLevel;
		panelEdge = _(SpriteName.keys_panel);
		panelEdge = panelEdge.Reduce(0, 0, 1, 0);
		panelMiddle = _(SpriteName.keys_panel);
		panelMiddle = panelMiddle.Reduce(panelMiddle.Width - 1, 0, 0, 0);
		panelOrnament = _(SpriteName.keys_panel_ornament);
		panelBottom = panelMiddle.Clone();
		panelBottom = panelBottom.Reduce(0, panelBottom.Height - 4, 0, 0);
		HasActiveSkill = false;
		skillPanels = new Dictionary<Skill, AbilityPanel>();
		foreach (KeyValuePair<Skill, int> item in this.skillLevel)
		{
			int value = item.Value;
			if (value != 0)
			{
				Skill key = item.Key;
				int maxLevel = (ShopMode ? value : base.core.CurrentCharDesc.Levels[base.core.ProfileData.CurrentCharLevel - 1].Abilities.SkillLevel[key]);
				skillPanels[key] = new AbilityPanel
				{
					parent = this,
					Ability = Abilities.SkillDesc[key],
					MaxLevel = maxLevel,
					CurrentLevel = value,
					Left = -200f
				};
				skillPanels[key].PanelSkill = key;
				skillPanels[key].Init();
				if (Abilities.SkillDesc[key].Kind == AbilityKind.Rechargeable)
				{
					ActiveSkill = key;
					HasActiveSkill = true;
				}
			}
		}
	}

	public override void Update()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		foreach (KeyValuePair<Skill, AbilityPanel> skillPanel in skillPanels)
		{
			AbilityPanel value = skillPanel.Value;
			if (!ShopMode)
			{
				value.UpdateCurrentLevel(base.core.CurrentPlayState.Player.Abilities.SkillLevel[skillPanel.Key]);
				value.UpdateCurrentCharge(base.core.CurrentPlayState.Player.Abilities.SkillCharge[skillPanel.Key]);
			}
			value.Update();
			if (!value.Hidden)
			{
				num += value.ModeWidth(compact: false);
				num2 += value.ModeWidth(compact: true);
				num3++;
			}
			else
			{
				value.TargetLeft = base.core.Renderer.ScreenWidth + 10;
			}
		}
		bool flag = num > (float)(base.core.Renderer.ScreenWidth - 8 - 2 * (num3 - 1));
		float num4 = (flag ? num2 : num) + (float)(2 * (num3 - 1));
		float num5 = ((float)base.core.Renderer.ScreenWidth - num4) / 2f;
		foreach (KeyValuePair<Skill, AbilityPanel> skillPanel2 in skillPanels)
		{
			AbilityPanel value2 = skillPanel2.Value;
			if (!value2.Hidden)
			{
				value2.CompactMode = flag;
				value2.TargetLeft = num5;
				num5 += value2.Width + 2f;
			}
		}
		base.Update();
	}

	public override void Draw()
	{
		foreach (KeyValuePair<Skill, AbilityPanel> skillPanel in skillPanels)
		{
			skillPanel.Value.Draw();
		}
		base.Draw();
	}
}
