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

public class NewCharState : State
{
	private enum Button
	{
		Share,
		Back
	}

	private Character character;

	private readonly CharDescription charDesc;

	private readonly int charLevel;

	private Sprite charPortrait;

	private Sprite charNamePic;

	private Sprite ray;

	private Sprite glow;

	private ParticleEmitter emitter;

	private bool reveal;

	private int seqN;

	private readonly int[] seqL = new int[5] { 30, 100, 20, 10, 40 };

	private int seqT;

	private float seqA;

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	public NewCharState(Character character, bool reveal = true)
	{
		base.TransDuration = 10;
		IsOpaque = true;
		IsOverlay = true;
		ShowCoins = false;
		this.character = character;
		this.reveal = reveal;
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 4000);
		int num = (int)base.core.Renderer.ScreenCenter.Shift(0f, 70f).Y + base.topSafeArea;
		int num2 = (base.core.Renderer.ScreenWidth - 20) / 4;
		touchMenu.SetupButton(Button.Back, new RectangleF(10 + 3 * num2 / 2 + 1, num, num2 * 2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.MISC_btn_back));
		touchMenu[Button.Back].Rectangle.Shift(-200f);
		touchMenu.SetupButton(Button.Share, new RectangleF(10 + num2 / 2, num, num2, 30f), base.core.SpriteManager.GetSprite(SpriteName.button), base.core.SpriteManager.GetSprite(SpriteName.button_pressed), null, stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(Settings.ShareIcon), icon: true, iconIsPicture: false, blink: true);
		touchMenu[Button.Share].Rectangle.Shift(-200f);
		touchMenu[Button.Share].Hidden = true;
		touchMenu[Button.Back].Rectangle = new RectangleF(core.Renderer.ScreenCenter.X - 70f, num, 140f, 30f);
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input, Button.Back);
		charDesc = CharDescription.Get[character];
		charPortrait = _(charDesc.Portrait);
		charNamePic = _(charDesc.NameImage + "_" + Locale.ShortName[base.core.LocaleManager.CurrentLocale], charDesc.NameImage);
		charLevel = base.core.ProfileData.Characters[character].Level;
		ray = _(SpriteName.ray_huge_soft);
		glow = _(SpriteName.glow_big);
		if (reveal)
		{
			SendMessage(new PlaySoundMessage(SoundName.unlock_lock));
			SendMessage(new PlaySoundMessage(SoundName.unlock_noise));
			SendMessage(new PlaySoundMessage(charDesc.RevealSound), 110 + charDesc.RevealSoundDelay);
			base.core.AudioManager.MusicVolumeBox.Set("unlock", 0.2f, inWorld: false, 0.4f, 0.05f, 200);
		}
		else
		{
			SendMessage(new PlaySoundMessage(charDesc.RevealSound), 20 + charDesc.RevealSoundDelay);
			base.core.AudioManager.MusicVolumeBox.Set("unlock", 0.2f, inWorld: false, 0.4f, 0.05f, 130);
		}
		if (!reveal)
		{
			seqN = 2;
			return;
		}
		Vector2 v = base.core.Renderer.ScreenCenter.Shift(0f, (float)base.core.Renderer.ScreenHeight * 0.05f + (float)base.topSafeArea);
		emitter = base.core.ParticleManager.AddEmitter(inWorld: false, v.Shift(0f, -10f), 5f).OnSpawn(delegate(Particle p)
		{
			if (p.Offset.Y > 0f)
			{
				p.Position.Y -= p.Offset.Y * 3f;
				p.Offset.Y = 0f - p.Offset.Y;
			}
			p.Position += p.Offset * 60f;
			p.Velocity.X = (float)Math.Atan2(p.Offset.Y, p.Offset.X);
		}).OnUpdate(delegate(Particle p)
		{
			p.Position -= p.Offset * 1.8f;
			p.Offset *= 0.98f;
			p.Dead = p.Age == 60;
		})
			.OnDraw(delegate(Particle p)
			{
				float num3 = (float)p.Age / 60f;
				base.core.Renderer["fg", 3000, false].DrawSpriteS(glow, p.Position, Color.Lerp(charDesc.Color2, charDesc.Color1, num3) * (num3 * num3) * 3f, new Vector2(1.2f * (1f - num3), 0.8f * (1f - num3) * num3) * 0.7f, p.Velocity.X, SpriteFlip.None, SpriteOrigin.Center);
			});
		emitter.Start(10, 10);
	}

	public override void Load()
	{
		Screen("new-character");
		base.Load();
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

	public override void Update()
	{
		if (seqT >= 0)
		{
			int num = seqL[seqN];
			seqT++;
			if (seqN == 1 && seqT == 50 && emitter != null)
			{
				emitter.Stop();
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

	public override void UpdateTransition()
	{
		if (Transition == TransType.Out)
		{
			touchMenu[Button.Share].Rectangle.Shift(-200f);
			touchMenu[Button.Back].Rectangle.Shift(-200f);
		}
		base.UpdateTransition();
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
		base.core.Renderer["fg", 2999, false].FillScreen(Color.Black);
		Vector2 vector = base.core.Renderer.ScreenCenter.Shift(0f, (float)base.core.Renderer.ScreenHeight * 0.05f + (float)base.topSafeArea);
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		Color value = Color.White;
		bool flag4 = false;
		Vector2 vector2 = Vector2.Zero;
		float num2 = 1f;
		float num3 = 1f;
		float value2 = 1f;
		bool flag5 = false;
		switch (seqN)
		{
		case 0:
			value = Color.Black;
			flag3 = true;
			break;
		case 1:
			value = Color.Lerp(Color.Black, charDesc.Color1, seqA * seqA * seqA);
			vector2 = SciHelper.GetRandomVectorInCircle(seqA * 5f);
			num2 = 1f - seqA * 0.3f;
			break;
		case 2:
			num2 = (float)Tween.CubicEaseOut(seqT, 0.699999988079071, 0.7, seqL[seqN]);
			flag2 = true;
			num3 = seqA;
			flag4 = true;
			value2 = Component._M(0f, (float)Tween.CubicEaseOut(seqT - 7, 0.699999988079071, 0.5, seqL[seqN]));
			break;
		case 3:
			flag2 = true;
			num2 = 1.4f - 0.4f * seqA;
			flag4 = true;
			value2 = 1.2f - 0.2f * seqA;
			break;
		case 4:
			flag2 = true;
			flag4 = true;
			flag5 = true;
			touchMenu[Button.Share].Rectangle.Shift((float)Tween.CubicEaseOut(seqT, -200.0, 200.0, seqL[seqN]));
			touchMenu[Button.Back].Rectangle.Shift((float)Tween.CubicEaseOut(seqT, -200.0, 200.0, seqL[seqN]));
			break;
		case 5:
			flag2 = true;
			flag4 = true;
			flag5 = true;
			break;
		}
		if (flag)
		{
			Vector2 vector3 = vector.Shift(0f, -30f);
			base.core.Renderer["fg", 2999, false].DrawSpriteS(_(SpriteName.shop_wall), vector3 - vector2 * 0.5f, Color.White, null, 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (flag2)
		{
			for (int i = 0; i < 8; i++)
			{
				base.core.Renderer["fg", 2999, false].DrawSpriteS(_(SpriteName.ray_huge), vector.Shift(0f, -20f), charDesc.Color1 * 0.5f * (0.3f + Component._sin((float)base.ticks * 0.014f + (float)(i - 2) * (float)Math.PI * 2f / 8f) * 0.7f), rotation: (float)i * (float)Math.PI * 2f / 8f + (float)base.ticks * 0.014f, scale: Vector2.One * num3, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
			}
			for (int j = 0; j < 8; j++)
			{
				base.core.Renderer["fg", 2999, false].DrawSpriteS(ray, vector.Shift(0f, -20f), charDesc.Color1 * 0.4f * (0.3f + Component._sin((float)base.ticks * 0.01f + (float)(j - 2) * (float)Math.PI * 2f / 8f) * 0.7f), rotation: (float)j * (float)Math.PI * 2f / 8f + (float)base.ticks * 0.01f, scale: Vector2.One * num3 * (0.9f + 0.1f * Component._sin((float)base.ticks * 0.05f + (float)(j * 2))) * 0.7f, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
			}
			for (int k = 0; k < 7; k++)
			{
				base.core.Renderer["fg", 2999, false].DrawSpriteS(ray, vector.Shift(0f, -20f), charDesc.Color1 * 0.4f * (0.3f + Component._sin((float)base.ticks * 0.01f + (float)(k - 2) * (float)Math.PI * 2f / 7f) * 0.7f), rotation: (float)(-k) * (float)Math.PI * 2f / 7f - (float)base.ticks * 0.01f, scale: Vector2.One * num3 * (0.7f + 0.2f * Component._sin((float)base.ticks * 0.04f + (float)(k * 3))) * 0.7f, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
			}
			for (int l = 0; l < 5; l++)
			{
				base.core.Renderer["fg", 2999, false].DrawSpriteS(ray, vector.Shift(0f, -20f), charDesc.Color2 * 0.4f * (0.3f + Component._sin((float)base.ticks * 0.015f + (float)(l - 2) * (float)Math.PI * 2f / 5f) * 0.7f), rotation: (float)l * (float)Math.PI * 2f / 5f + (float)base.ticks * 0.015f, scale: Vector2.One * num3 * (0.8f + 0.1f * Component._cos((float)base.ticks * 0.055f + (float)(l * 4))) * 0.7f, flip: SpriteFlip.None, origin: SpriteOrigin.TopCenter);
			}
		}
		base.core.Renderer["fg", 2999, false].DrawSpriteS(_(SpriteName.glow_huge), vector.Shift(0f, -18f).Shift(Component._cos((float)base.ticks * 0.07f) * 2.5f, Component._sin((float)base.ticks * 0.08f)), charDesc.Color1 * (0.8f + Component._sin((float)base.ticks * 0.035f) * 0.2f), Vector2.One * 1f, 0f, SpriteFlip.None, SpriteOrigin.Center);
		base.core.Renderer["fg", 3000, false].DrawSpriteS(_(SpriteName.glow_huge), vector.Shift(0f, -18f).Shift(Component._sin((float)base.ticks * 0.05f) * 2.5f, Component._cos((float)base.ticks * 0.06f)), charDesc.Color2 * (0.85f + Component._cos((float)base.ticks * 0.05f) * 0.15f), Vector2.One * 1f, 0f, SpriteFlip.None, SpriteOrigin.Center);
		if (charDesc.DrawPortraitUnderExtra || !flag5 || !num2.IsEqualTo(1f))
		{
			base.core.Renderer["fg", 3001, false].DrawSpriteS(charPortrait, vector + (new Vector2(charPortrait.Width, charPortrait.Height) * 0.5f - charPortrait.Link) * num2 + vector2, value, new Vector2(num2), 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (flag5)
		{
			base.core.Renderer.DrawPortraitExtra(character, charUnlocked: true, vector, charDesc, charLevel, 3001, (float)Tween.BackEaseOut(seqA, 0.0, 1.0, 1.0));
		}
		if (flag3)
		{
			base.core.Renderer["fg", 3001, false].DrawSpriteS(_(SpriteName.icon_lock_open), vector.Shift(0f, (float)(-charPortrait.Height) * 0.4f).Shift(0f, seqA * seqA * 80f - seqA * 30f), Color.White * (1f - seqA * seqA), new Vector2(1f + 1f * seqA * seqA), seqA * (float)Math.PI * 2f * 1.5f, SpriteFlip.None, SpriteOrigin.Center);
		}
		if (flag4)
		{
			base.core.Renderer["fg", 3001, false].DrawSpriteS(charNamePic, vector.Shift(0f, 23f * num2), null, new Vector2(value2), 0f, SpriteFlip.None, SpriteOrigin.Center);
		}
		base.Draw();
	}

	public override void OnBackButtonPressed()
	{
		IsOpaque = false;
		TransitionOut(CoreEvent.PopState);
		if (emitter != null)
		{
			emitter.Stop();
		}
		base.OnBackButtonPressed();
	}
}
