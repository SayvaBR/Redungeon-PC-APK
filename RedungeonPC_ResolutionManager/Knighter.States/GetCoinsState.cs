using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class GetCoinsState : State
{
	private enum Button
	{
		WatchAd,
		Offer1,
		Offer2,
		Offer3,
		FreeCoins,
		Doubler,
		Back
	}

	private readonly Sprite bg;

	private readonly float bgTop;

	private readonly TouchMenu<Button> touchMenu;

	private readonly TouchMenuNavigator<Button> menuNavigator;

	private Animation doubler;

	public GetCoinsState()
	{
		base.TransDuration = 25;
		IsOverlay = true;
		bg = _(SpriteName.coin_shop_bg);
		touchMenu = new TouchMenu<Button>(null, OnButtonRelease, "fg", 2000);
		doubler = new Animation();
		doubler.Add("double", "doubler_", "1111123425678999999abcdefgggggghij");
		doubler.Play("double");
		int num = 50;
		int num2 = 30;
		float num3 = (float)(base.core.Renderer.ScreenWidth - bg.Width) * 0.5f;
		bgTop = (float)(base.core.Renderer.ScreenHeight - bg.Height) * 0.5f;
		float x = num3 + 91f + 12f;
		float num4 = bgTop + 35f;
		if (base.core.ProfileData.AdsRemoved)
		{
			touchMenu.SetupButton(Button.FreeCoins, new RectangleF(x, num4, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.COINSHOP_free_coins));
			bool flag = true;
			if (base.core.ProfileData.FreeCoinsLastTime != string.Empty)
			{
				DateTime dateTime = DateTime.Parse(base.core.ProfileData.FreeCoinsLastTime);
				flag = DateTime.Now.Day != dateTime.Day;
			}
			touchMenu[Button.FreeCoins].Disabled = !flag;
		}
		else
		{
			touchMenu.SetupButton(Button.WatchAd, new RectangleF(x, num4, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, "", _(SpriteName.icon_ad));
			touchMenu[Button.WatchAd].Disabled = !base.core.AdsManager.CanShowUnityAds();
		}
		touchMenu.SetupButton(Button.Offer1, new RectangleF(x, num4 + 39f, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.COINSHOP_not_available));
		touchMenu.SetupButton(Button.Offer2, new RectangleF(x, num4 + 78f, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.COINSHOP_not_available));
		touchMenu.SetupButton(Button.Offer3, new RectangleF(x, num4 + 117f, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, __(SId.COINSHOP_not_available));
		touchMenu.SetupButton(Button.Doubler, new RectangleF(x, num4 + 156f, num, num2), _(SpriteName.button), _(SpriteName.button_pressed), _(SpriteName.button_disabled), stretch: true, SpriteFlip.None, ButtonColor.Orange, base.core.ProfileData.CoinDoublerEnabled ? __(SId.COINSHOP_purchased) : __(SId.COINSHOP_not_available));
		touchMenu.SetupButton(Button.Back, new RectangleF(base.core.Renderer.ScreenCenter.X - 35f, bgTop + (float)bg.Height, 70f, 30f), _(SpriteName.button_back), _(SpriteName.button_back_down));
		touchMenu[Button.Offer1].Disabled = true;
		touchMenu[Button.Offer2].Disabled = true;
		touchMenu[Button.Offer3].Disabled = true;
		menuNavigator = new TouchMenuNavigator<Button>(touchMenu, base.core.Input,
			base.core.ProfileData.AdsRemoved ? Button.FreeCoins : Button.WatchAd,
			Button.Offer1, Button.Offer2, Button.Offer3, Button.Doubler, Button.Back);
		touchMenu[Button.Doubler].Disabled = true;
		SendMessage(new PlaySoundMessage(SoundName.trans_2));
		if (!TryProcessReceivedProducts())
		{
			base.core.Store.RequestProducts();
		}
	}

	public override void Update()
	{
		IsOpaque = Transition == TransType.None;
		doubler.Update();
		base.Update();
	}

	public override void UpdateTransition()
	{
		touchMenu[base.core.ProfileData.AdsRemoved ? Button.FreeCoins : Button.WatchAd].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		touchMenu[Button.Offer1].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		touchMenu[Button.Offer2].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		touchMenu[Button.Offer3].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		touchMenu[Button.Doubler].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		touchMenu[Button.Back].Rectangle.Shift(0f, (float)Tween.CircEaseOut(TransD(0, 4), -200.0, 200.0, base.TransDuration - 4));
		base.UpdateTransition();
	}

	public override void Load()
	{
		Screen("get-coins");
		Subscribe(MessageType.ReceivedProducts);
		base.Load();
	}

	private bool TryProcessReceivedProducts()
	{
		if (!base.core.Store.AllProductsAvailable())
		{
			return false;
		}
		touchMenu[Button.Offer1].Label = base.core.Store.GetPrice(Iap.Offer1);
		touchMenu[Button.Offer1].Disabled = false;
		touchMenu[Button.Offer2].Label = base.core.Store.GetPrice(Iap.Offer2);
		touchMenu[Button.Offer2].Disabled = false;
		touchMenu[Button.Offer3].Label = base.core.Store.GetPrice(Iap.Offer3);
		touchMenu[Button.Offer3].Disabled = false;
		if (!base.core.ProfileData.CoinDoublerEnabled)
		{
			touchMenu[Button.Doubler].Label = base.core.Store.GetPrice(Iap.CoinDoubler);
			touchMenu[Button.Doubler].Disabled = false;
		}
		return true;
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
		base.core.Renderer["fg", 2000, false].FillScreen(Color.Black * 1f * ((float)base.Trans / (float)base.TransDuration));
		Vector2 vector = new Vector2((float)(base.core.Renderer.ScreenWidth - bg.Width) * 0.5f, touchMenu[base.core.ProfileData.AdsRemoved ? Button.FreeCoins : Button.WatchAd].Rectangle.Top - 38f);
		base.core.Renderer["fg", 2000, false].DrawSpriteS(bg, vector);
		float num = vector.Y + 37f;
		for (int i = 0; i < 5; i++)
		{
			string text = "^";
			switch (i)
			{
			case 0:
				text += base.core.AdsManager.GetOptimalWatchAdReward() * ((!base.core.ProfileData.AdsRemoved) ? 1 : 3);
				break;
			case 1:
				text += Store.CoinsForOffer[Iap.Offer1];
				break;
			case 2:
				text += Store.CoinsForOffer[Iap.Offer2];
				break;
			case 3:
				text += Store.CoinsForOffer[Iap.Offer3];
				break;
			case 4:
				text = __(SId.COINSHOP_coin_doubler);
				break;
			}
			base.core.Renderer["fg", 2000, false].DrawTextS(text, new Vector2(vector.X + 92f + 12f, num + (float)(39 * i)), TextProfile.OrangeBoldText.Alter(null, null, boxAlignment: Alignment2D.Right, textAlignment: Alignment2D.Middle, width: 50, height: 30, decoration: TextDecoration.None, font: null, scale: 0.7f));
		}
		base.core.Renderer["fg", 2000, false].DrawSpriteS(doubler.GetCurrentFrame(), vector.Shift(6f, 195f));
		touchMenu.Draw();
		base.Draw();
	}

	private void OnButtonRelease(Button button)
	{
		switch (button)
		{
		case Button.WatchAd:
			if (base.core.AdsManager.CanShowUnityAds())
			{
				base.core.AdsManager.ShowUnityAds(delegate(WatchAddStatus status)
				{
					if (status == WatchAddStatus.Watched)
					{
						int optimalWatchAdReward = base.core.AdsManager.GetOptimalWatchAdReward();
						base.core.ProfileData.AddCoins(optimalWatchAdReward);
						SendMessage(new PushStateMessage(new PopupState(optimalWatchAdReward)));
						touchMenu[Button.WatchAd].LabelSprite = null;
						touchMenu[Button.WatchAd].Label = __(SId.COINSHOP_not_available);
						touchMenu[Button.WatchAd].Disabled = true;
					}
				});
			}
			else
			{
				touchMenu[Button.WatchAd].LabelSprite = null;
				touchMenu[Button.WatchAd].Label = __(SId.COINSHOP_not_available);
				touchMenu[Button.WatchAd].Disabled = true;
			}
			break;
		case Button.Offer1:
		case Button.Offer2:
		case Button.Offer3:
			base.core.Store.PurchaseProduct(button switch
			{
				Button.Offer2 => Iap.Offer2, 
				Button.Offer1 => Iap.Offer1, 
				_ => Iap.Offer3, 
			}, delegate(Iap iap, bool succeed)
			{
				if (succeed)
				{
					SendMessage(new PushStateMessage(new PopupState(Store.CoinsForOffer[iap])));
				}
			});
			break;
		case Button.Doubler:
			base.core.Store.PurchaseProduct(Iap.CoinDoubler, delegate(Iap iap, bool succeed)
			{
				if (succeed)
				{
					touchMenu[Button.Doubler].Label = __(SId.COINSHOP_purchased);
					touchMenu[Button.Doubler].Disabled = true;
				}
			});
			break;
		case Button.FreeCoins:
		{
			int num = base.core.AdsManager.GetOptimalWatchAdReward() * 3;
			Event(AnalyticsCategory.Ux, "free-coins", num);
			base.core.ProfileData.FreeCoinsLastTime = DateTimeHelper.SafeNow();
			base.core.ProfileData.AddCoins(num);
			SendMessage(new PushStateMessage(new PopupState(num)));
			touchMenu[Button.FreeCoins].Disabled = true;
			break;
		}
		case Button.Back:
			OnBackButtonPressed();
			break;
		}
	}

	public override void OnMessage(Message message, object sender)
	{
		MessageType type = message.Type;
		if (type == MessageType.ReceivedProducts)
		{
			TryProcessReceivedProducts();
		}
		base.OnMessage(message, sender);
	}

	public override void OnBackButtonPressed()
	{
		SendMessage(new PlaySoundMessage(SoundName.trans_1), 7);
		TransitionOut(CoreEvent.HideGetCoins);
		base.OnBackButtonPressed();
	}
}
