namespace Knighter.Entities;

public class ChestContents
{
	public ItemType Item;

	public int Count;

	public ChestContents(ItemType item, int count)
	{
		Item = item;
		Count = count;
	}
}
