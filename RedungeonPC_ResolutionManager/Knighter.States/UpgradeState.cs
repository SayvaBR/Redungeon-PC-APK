using System;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class UpgradeState : State
{
	private enum Button
	{
		Share,
		Back
	}

	private int seqN;

	private int[] seqL = new int[3] { 50, 30, 30 };

	private int seqT;

	private float seqA;

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private ParticleEmitter buildupEmitter;

	private ParticleEmitter emitter;

	private ShopState shop;

	private readonly CharDescription charDesc;

	private int level;

	private string lvlText;

	private string lvlDesc;

	public UpgradeState(ShopState shop)
	{
		this.shop = shop;
		charDesc = CharDescription.Get[shop.CurrentCharacter];
		base.TransDuration = 15;
		IsOverlay = true;
		ShowCoins = false;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 5000);
		SendMessage(new PlaySoundMessage(SoundName.upgrade));
		base.core.AudioManager.MusicVolumeBox.Set("upgrade", 0.2f, inWorld: false, 0.4f, 0.05f, 130);
		int num = base.core.Renderer.ScreenHeight - 45;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.Back, new RectangleF(10 + 3 * num2 / 2 + 1, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.MISC_btn_back));
		touchMenu[Button.Back].Rectangle.Shift(-200f);
		touchMenu.SetupButton(Button.Share, new RectangleF(10 + num2 / 2, num, num2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(Settings.ShareIcon), icon: true, iconIsPicture: false, blink: true);
		touchMenu[Button.Share].Rectangle.Shift(-200f);
		touchMenu[Button.Share].Hidden = true;
		touchMenu[Button.Back].Rectangle = new RectangleF(core.Renderer.ScreenCenter.X - 70f, num, 140f, 30f);
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, Button.Back);
		level = base.core.ProfileData.Characters[shop.CurrentCharacter].Level;
		lvlText = ((level == CharDescription.Get[shop.CurrentCharacter].Levels.Count) ? __(SId.SHOP_max_level) : (__(SId.SHOP_level) + " " + level));
		lvlDesc = __(charDesc.Levels[level - 1].Description);
		buildupEmitter = base.core.ParticleManager.AddEmitter(inWorld: false, base.core.Renderer.ScreenCenter.Shift(0f, (float)base.core.Renderer.ScreenHeight * 0.5f + (float)base.topSafeArea), 1f).OnSpawn(delegate
		{
		}).OnUpdate(delegate(Particle p)
		{
			p.Velocity.X = Component._cos((float)p.Age * 0.25f) * 9f;
			p.Velocity.Y = (float)(-base.core.Renderer.ScreenHeight) * 1.05f / (40f + (float)p.Age * 2f);
			p.Velocity.Y -= (1f - Math.Abs(p.Velocity.X)) * 0.2f;
			p.Position += p.Velocity;
			p.Dead = p.Age == 90;
		})
			.OnDraw(delegate(Particle p)
			{
				float num3 = (float)p.Age / 90f;
				float num4 = 1f - num3;
				int num5 = ((p.Velocity.X < 0f) ? (-3) : 5);
				base.core.Renderer["fg", num5, false].DrawSpriteS(_(SpriteName.glow_big), p.Position, charDesc.Color1 * (1f - num3 * num3 * num3) * 3f, new Vector2(0.5f, p.Velocity.X * 0.2f) * 3.5f * num3 * num3 * num3, (float)Math.Atan2(p.Velocity.X, 0f - p.Velocity.Y), SpriteFlip.None, SpriteOrigin.Center);
				base.core.Renderer["fg", num5 + 1, false].DrawSpriteS(_(SpriteName.glow_big), p.Position, Color.Lerp(charDesc.Color2, Color.White, num3 * num3 * num3) * (1f - num3 * num3 * num3 * num3) * 3f, new Vector2(0.6f, p.Velocity.Length() * (0.5f + 0.75f * num4 * num4)) * 0.5f * num3, (float)Math.Atan2(p.Velocity.X, 0f - p.Velocity.Y), SpriteFlip.None, SpriteOrigin.Center);
			});
		buildupEmitter.Emit(10, 1);
		emitter = base.core.ParticleManager.AddEmitter(inWorld: false, new Vector2(0f, base.core.Renderer.ScreenHeight + 40), base.core.Renderer.ScreenWidth, 1f).OnSpawn(delegate(Particle p)
		{
			p.Velocity = new Vector2(0f, -0.17f);
		}).OnUpdate(delegate(Particle p)
		{
			p.Position += p.Velocity;
			p.Velocity *= 1.007f;
			p.Dead = p.Age == 60;
		})
			.OnDraw(delegate(Particle p)
			{
				float num3 = 1f - (float)p.Age / 60f;
				float num4 = 1f - num3;
				base.core.Renderer["fg", -2, false].DrawSpriteS(_(SpriteName.ray_huge_soft), p.Position, charDesc.Color1 * num3 * num3 * num4 * 5f, new Vector2(0.5f + num3, 1.5f - num3) * 3.3f * (((float)base.core.Renderer.ScreenWidth * 0.5f - Math.Abs(p.Offset.X)) / (float)base.core.Renderer.ScreenWidth), 0f, SpriteFlip.None, SpriteOrigin.BottomCenter);
			});
	}

	public override void Load()
	{
		Screen("upgrade");
		base.Load();
	}

	public override void UpdateTransition()
	{
		if (Transition == TransType.Out)
		{
			touchMenu[Button.Share].Rectangle.Shift(-200f);
			touchMenu[Button.Back].Rectangle.Shift(-200f);
		}
		base.UpdateTransition();
	}

	public override void Update()
	{
		if (seqT >= 0)
		{
			int num = seqL[seqN];
			seqT++;
			if (seqN == 0 && seqT == 30)
			{
				emitter.Emit(20, 2, once: true, 2);
			}
			if (seqN == 1 && seqT == 7)
			{
				shop.ChangeCurrentCharacter(shop.CurrentCharacter);
			}
			if (seqT == num)
			{
				seqN++;
				if (seqN <= seqL.Length - 1)
				{
					seqT = 0;
					seqA = 0f;
				}
				else
				{
					seqT = -1;
					seqA = 1f;
				}
			}
			else
			{
				seqA = (float)seqT / (float)num;
			}
		}
		base.Update();
	}

	public override void HandleInput()
	{
		if (seqT < 0 && Transition == TransType.None
			&& !touchMenu.HandleInput()
			&& !menuNavigator.HandleInput((float)base.core.GameTime.ElapsedGameTime.TotalSeconds))
		{
			base.HandleInput();
		}
	}

	public override void Draw()
	{
		if (!base.core.TakingScreenshot)
		{
			touchMenu.Draw();
		}
		float num = (float)base.Trans / (float)base.TransDuration;
		switch (Transition)
		{
		case TransType.In:
			base.core.Renderer["fg", 4000, false].FillScreen(charDesc.Color1 * (1f - num));
			break;
		case TransType.Out:
			base.core.Renderer["fg", 4000, false].FillScreen(charDesc.Color1 * num);
			return;
		}
		bool flag = true;
		float num2 = 0f;
		switch (seqN)
		{
		case 0:
			flag = false;
			break;
		case 1:
			base.core.Renderer["fg", 30, false].FillScreen(charDesc.Color1 * seqA * (1f - seqA) * 5f);
			num2 = (float)Tween.BackEaseOut(seqA, 0.0, 1.0, 1.0);
			break;
		case 2:
			touchMenu[Button.Share].Rectangle.Shift((float)Tween.CubicEaseOut(seqT, -200.0, 200.0, seqL[seqN]));
			touchMenu[Button.Back].Rectangle.Shift((float)Tween.CubicEaseOut(seqT, -200.0, 200.0, seqL[seqN]));
			num2 = 1f;
			break;
		case 3:
			num2 = 1f;
			break;
		}
		if (flag)
		{
			base.core.Renderer["fg", 3000, false].DrawTextS(lvlText, new Vector2(base.core.Renderer.ScreenCenter.X, base.core.Renderer.ScreenCenter.Y * 1.1f + 25f * num2 + (float)base.topSafeArea), TextProfile.OrangeBoldText.Alter(charDesc.Color1 * 3f, null, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, width: 200, height: 25, decoration: TextDecoration.None, font: null, scale: num2 / Settings.GuiScale));
			base.core.Renderer["fg", 3000, false].DrawTextS(lvlDesc, new Vector2(base.core.Renderer.ScreenCenter.X, base.core.Renderer.ScreenCenter.Y * 1.1f + 40f * num2 + (float)base.topSafeArea), TextProfile.OrangeBoldText.Alter(font: Font.Thin, color: charDesc.Color1 * 2f * num2, secondColor: null, boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Center, width: base.core.Renderer.ScreenWidth - 20, height: 25, decoration: TextDecoration.None, scale: 0.75f / Settings.GuiScale));
		}
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
	}

	public override void OnBackButtonPressed()
	{
		TransitionOut(CoreEvent.HideOptions);
		base.OnBackButtonPressed();
	}
}
