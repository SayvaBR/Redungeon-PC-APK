using Knighter.Graphics;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter;

public class AchievementMeta
{
	public SId Name;

	public SId Briefing;

	public SId Debriefing;

	public bool Hidden;

	public SpriteName Icon;

	public Color ColorBG;

	public Color ColorFG;

	public Color ColorFrame;

	public float IconDx;

	public float IconDy;

	public AchievementMeta(SId name, SId briefing, SId debriefing, SpriteName icon, Color colorBG, Color colorFG, Color colorFrame, bool hidden = false, float iconDx = 0f, float iconDy = 0f)
	{
		Name = name;
		Briefing = briefing;
		Debriefing = debriefing;
		Icon = icon;
		ColorBG = colorBG;
		ColorFG = colorFG;
		ColorFrame = colorFrame;
		Hidden = hidden;
		IconDx = iconDx;
		IconDy = iconDy;
	}
}
