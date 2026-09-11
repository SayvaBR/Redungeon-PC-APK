using Knighter.Localization;

namespace Knighter.Entities;

public class CharLevel
{
	public Abilities Abilities;

	private readonly int priceFactor;

	public SId Description;

	public Skill? Highlight;

	public int Price => priceFactor * 25;

	public CharLevel(Abilities abilities, int price, SId description = SId.MISC_empty, Skill? highlight = null)
	{
		Abilities = abilities;
		priceFactor = price;
		Description = description;
		Highlight = highlight;
	}
}
