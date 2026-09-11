using System;
using System.Linq;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

public sealed class AchievementBrowserWidget : Component, IMenuWidget
{
	private readonly Achievement[] entries = Achievements.Metas.Keys.ToArray();
	private int index;
	public RectangleF Bounds { get; set; }
	public bool IsFocusable => true;
	public bool ConsumesHorizontalInput => true;
	public void Update(float dt) { }
	public void Activate() => Adjust(1);
	public void Adjust(int direction) => index = (index + direction + entries.Length) % entries.Length;
	public void HandleClick(Vector2 p) => Adjust(p.X < Bounds.Center.X ? -1 : 1);
	public void Draw(bool focused, int depth)
	{
		var key = entries[index];
		var meta = Achievements.Metas[key];
		bool unlocked = core.ProfileData.IsAchievementUnlocked(key);
		MenuTheme.Panel(R, Bounds, depth);
		var text = TextProfile.OrangeBoldText.Alter(width: (int)Bounds.Width - 20, height: 18, scale: 0.55f,
			boxAlignment: Alignment2D.Center, textAlignment: Alignment2D.Middle);
		R["fg", depth + 3, false].DrawTextS($"<  {index + 1} / {entries.Length}  >", Bounds.TopLeft + new Vector2(Bounds.Width / 2, 4), text);
		if (!meta.Hidden || unlocked)
		{
			Vector2 badge = Bounds.TopLeft + new Vector2(12, 32);
			R["fg", depth + 1, false].DrawSpriteS(_(SpriteName.facts_achievement_bg), badge + new Vector2(1, .5f), meta.ColorBG);
			R["fg", depth + 2, false].DrawSpriteS(_(SpriteName.facts_achievement_glow), badge, meta.ColorFG);
			R["fg", depth + 3, false].DrawSpriteS(_(SpriteName.facts_achievement_frame), badge, meta.ColorFrame);
			R["fg", depth + 4, false].DrawSpriteS(_(meta.Icon), badge + new Vector2(20 + meta.IconDx * .2f, 21 + meta.IconDy * .2f), Color.White,
				Vector2.One, 0, SpriteFlip.None, SpriteOrigin.Center);
		}
		float detailsX = Bounds.Width / 2 + 62;
		text = text.Alter(width: Math.Max(40, (int)Bounds.Width - 150));
		R["fg", depth + 3, false].DrawTextS(meta.Hidden && !unlocked ? "???" : __(meta.Name), Bounds.TopLeft + new Vector2(detailsX, 24), text);
		R["fg", depth + 3, false].DrawTextS(meta.Hidden && !unlocked ? "???" : __(meta.Briefing), Bounds.TopLeft + new Vector2(detailsX, 46),
			text.Alter(height: 44, scale: 0.48f, color: Color.White));
		string progress = unlocked ? "100%" : Achievements.Targets.TryGetValue(key, out int target)
			? $"{Math.Clamp(core.Achievments.GetProgress(key), 0, target)} / {target}" : "0%";
		R["fg", depth + 3, false].DrawTextS(progress, Bounds.TopLeft + new Vector2(Bounds.Width / 2, 96), text);
	}
}
