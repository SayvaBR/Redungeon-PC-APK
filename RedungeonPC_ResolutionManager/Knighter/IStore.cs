namespace Knighter;

public interface IStore
{
	void RestorePurchases();

	void Initialize();

	void RequestProducts();

	bool CanMakePayments();

	bool PurchaseProduct(Iap iap);

	bool AllProductsAvailable();

	string GetPrice(Iap iap);
}
