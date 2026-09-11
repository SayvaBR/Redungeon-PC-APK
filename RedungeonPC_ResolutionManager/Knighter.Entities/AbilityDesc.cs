using Knighter.Graphics;
using Knighter.Localization;

namespace Knighter.Entities;

public class AbilityDesc
{
	public SId Name;

	public SId Description;

	public AbilityKind Kind;

	public SpriteName? HudMainIcon;

	public SpriteName? HudItemIcon;

	public SpriteName? HudItemSlot;

	public SpriteName? HudChargeBar;

	public SpriteName? Illustration;

	public int Color1;

	public int Color2;

	public int Color3;

	public bool HideChargeBar;
}
