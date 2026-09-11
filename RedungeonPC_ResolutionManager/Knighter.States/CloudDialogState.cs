using System;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class CloudDialogState : State
{
	private enum Button
	{
		Yes,
		No
	}

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private RectangleF menuRect;

	private bool merging;

	private int refund;

	private Sprite bigCloud;

	private Sprite icon;

	private Sprite cover;

	private Sprite block;

	private Sprite chain;

	private bool otherDevice;

	public CloudDialogState(bool merging = false, bool otherDevice = true, int refund = 0)
	{
		base.TransDuration = 25;
		IsOverlay = true;
		ShowCoins = false;
		this.merging = merging;
		this.refund = refund;
		this.otherDevice = otherDevice;
		block = _(merging ? SpriteName.options_block : SpriteName.cloud_block);
		cover = _(SpriteName.cloud_icon_cover);
		icon = _(SpriteName.cloud_icon);
		icon = _(SpriteName.cloud_icon_android);
		bigCloud = _(SpriteName.cloud_decoration);
		chain = _(SpriteName.gui_chain);
		menuRect = new RectangleF((float)(base.core.Renderer.ScreenWidth - block.Width) * 0.5f, (float)(base.core.Renderer.ScreenHeight - block.Height) * 0.5f, block.Width, block.Height);
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 15000);
		if (!merging)
		{
			touchMenu.SetupButton(Button.Yes, new RectangleF(menuRect.Left + 7f, menuRect.Bottom - 8f - 30f, 60f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "Yes");
			touchMenu.SetupButton(Button.No, new RectangleF(menuRect.Right - 7f - 60f, menuRect.Bottom - 8f - 30f, 60f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "No");
		}
		else
		{
			touchMenu.SetupButton(Button.Yes, new RectangleF(menuRect.Center.X - 30f, menuRect.Bottom - 8f - 30f, 60f, 30f), _(SpriteName.button), _(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "Cool!");
		}
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, Button.Yes, Button.No);
	}

	public override void UpdateTransition()
	{
		float y = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
		touchMenu[Button.Yes].Rectangle.Shift(0f, y);
		if (!merging)
		{
			touchMenu[Button.No].Rectangle.Shift(0f, y);
		}
		base.UpdateTransition();
	}

	public override void Update()
	{
		touchMenu.Update();
		base.Update();
	}

	public override void HandleInput()
	{
		if (Transition == TransType.None)
		{
			touchMenu.HandleInput();
			menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds);
			base.HandleInput();
		}
	}

	public override void Draw()
	{
		float num = 1f - (float)base.Trans / (float)base.TransDuration;
		base.core.Renderer["fg", 9998, false].FillScreen(Color.Black * (1f - num * num * num));
		float num2 = (float)Tween.BackEaseOut(TransD(0, 4), -250.0, 250.0, base.TransDuration - 4);
		for (int i = chain.Height; menuRect.Top + 21f + num2 - (float)i > (float)(-chain.Height); i += chain.Height)
		{
			base.core.Renderer["fg", 10000, false].DrawSpriteS(chain, new Vector2(menuRect.Left + 20f, menuRect.Top + 21f + num2 - (float)i));
			base.core.Renderer["fg", 10000, false].DrawSpriteS(chain, new Vector2(menuRect.Right - 19f - (float)chain.Width, menuRect.Top + 21f + num2 - (float)i));
		}
		Vector2 vector = menuRect.TopLeft.Shift(0f, num2);
		base.core.Renderer["fg", 10000, false].DrawSpriteS(block, vector);
		if (merging)
		{
			base.core.Renderer["fg", 10000, false].DrawSpriteS(cover, vector);
		}
		base.core.Renderer["fg", 10000, false].DrawSpriteS(icon, new Vector2(menuRect.Center.X - (float)(icon.Width / 2), vector.Y + (float)(merging ? 11 : (-7))));
		base.core.Renderer["fg", 10010, false].DrawSpriteS(bigCloud, menuRect.TopLeft.Shift(-60f + num2 / 2f + Component._sin((float)(base.ticks + 300) * 0.012f) * 5f, 32f), Color.White * 0.2f);
		base.core.Renderer["fg", 9999, false].DrawSpriteS(bigCloud, menuRect.TopLeft.Shift(-5f + num2 / 3f + Component._sin((float)base.ticks * 0.009f) * 7f, -30f), Color.White * 0.2f);
		base.core.Renderer["fg", 10010, false].DrawSpriteS(bigCloud, menuRect.TopRight.Shift(-10f - num2 / 2f + Component._sin((float)base.ticks * 0.01f) * 5f, 20f), Color.White * 0.2f, null, 0f, SpriteFlip.Horizontal);
		string text = "iCloud";
		text = "Saved Games";
		if (merging)
		{
			TextProfile textProfile = new TextProfile
			{
				Width = (int)menuRect.Width - 26,
				BoxAlignment = Alignment2D.Left,
				TextAlignment = Alignment2D.Center,
				Color = default(Color).FromRgb(9212825),
				SecondColor = default(Color).FromRgb(1645605),
				Decoration = TextDecoration.Extrude2,
				Font = Font.Thin,
				Scale = 0.8f
			};
			vector.Y += 45f;
			vector.X += 11f;
			base.core.Renderer["fg", 10000, false].DrawTextS(text, vector, textProfile.Alter(null, null, null, null, null, null, null, Font.Bold, 1f));
			vector.Y += 15f;
			vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS(otherDevice ? "You've been playing Redungeon on another device." : "You've been playing Redungeon on this device earlier.", vector, textProfile).Height + (float)((refund > 0) ? 10 : 20);
			vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS("Your progress was combined!", vector, textProfile.Alter(null, null, null, null, null, null, null, Font.Bold)).Height + 10f;
			int num3 = 0;
			foreach (Character value in Enum.GetValues(typeof(Character)))
			{
				CharDescription charDescription = CharDescription.Get[value];
				base.core.Renderer["fg", 10000, false].DrawSpriteS(_(charDescription.Icon), vector.Shift(5 + 15 * num3, 0f), base.core.ProfileData.Characters[value].Unlocked ? Color.White : Color.Black, Vector2.One * 0.65f);
				num3++;
				if (num3 >= 8)
				{
					break;
				}
			}
			vector.Y += 18f;
			vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS("^" + base.core.ProfileData.Coins, vector, textProfile.Alter(TextProfile.OrangeLight)).Height + 10f;
			if (refund > 0)
			{
				vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS("This includes refund for things you've unlocked twice:", vector, textProfile).Height + 5f;
				vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS("+^" + refund, vector, textProfile.Alter(TextProfile.OrangeLight)).Height + 10f;
			}
			else
			{
				vector.Y += 5f;
				vector.Y += base.core.Renderer["fg", 10000, false].DrawTextS("Enjoy!", vector, textProfile.Alter(null, null, null, null, null, null, null, Font.Bold)).Height + 5f;
			}
		}
		else
		{
			base.core.Renderer["fg", 10000, false].DrawTextS($"Should we use {text} to sync your progress?", vector.Shift(10f, 35f), new TextProfile
			{
				Width = (int)menuRect.Width - 20,
				BoxAlignment = Alignment2D.Left,
				TextAlignment = Alignment2D.Center,
				Color = default(Color).FromRgb(9212825),
				SecondColor = default(Color).FromRgb(1645605),
				Decoration = TextDecoration.Extrude2,
				Font = Font.Thin
			});
		}
		touchMenu.Draw();
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		if ((uint)button <= 1u)
		{
			base.core.ProfileData.UseCloud = button == Button.Yes;
			base.core.ProfileData.SaveIntoStorage();
		}
		TransitionOut(CoreEvent.PopState);
	}
}
