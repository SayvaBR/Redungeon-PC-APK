using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's Store.cs (Google/Amazon IAP via
/// Plugin.InAppBilling). No purchase system on desktop for now - every
/// call is a safe no-op / "not available" so gameplay code that checks
/// before offering a purchase just skips that path gracefully.
/// </summary>
public class Store : Component, IStore
{
	public delegate void OnProductPurchase(Iap iap, bool succeed);

	public static readonly Dictionary<Iap, int> CoinsForOffer = new Dictionary<Iap, int>
	{
		{ Iap.Offer1, 3000 },
		{ Iap.Offer2, 12000 },
		{ Iap.Offer3, 50000 },
	};

	public void Initialize()
	{
	}

	public void RequestProducts()
	{
	}

	public void RestorePurchases()
	{
	}

	public bool CanMakePayments()
	{
		return false;
	}

	public bool PurchaseProduct(Iap iap)
	{
		return false;
	}

	public void PurchaseProduct(Iap iap, OnProductPurchase onProductPurchase)
	{
		onProductPurchase?.Invoke(iap, false);
	}

	public bool AllProductsAvailable()
	{
		return false;
	}

	public string GetPrice(Iap iap)
	{
		return string.Empty;
	}
}
