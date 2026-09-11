using System;
using System.Collections.Generic;
using System.Linq;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.UI;

/// <summary>Local profile statistics, paginated into a landscape-friendly grid.</summary>
public sealed class StatisticsBrowserWidget : Component, IMenuWidget
{
	private record Row(SId Label, string Icon, string Value);
	private readonly List<Row> rows = new();
	private int page;
	public RectangleF Bounds { get; set; }
	public bool IsFocusable => true;
	public bool ConsumesHorizontalInput => true;
	private int Columns => Bounds.Width >= 340f ? 2 : 1;
	private int RowsPerColumn => Math.Max(1, Math.Min(4, (int)((Bounds.Height - 24f) / 34f)));
	private int Capacity => Columns * RowsPerColumn;
	private int Pages => Math.Max(1, (rows.Count + Capacity - 1) / Capacity);

	public StatisticsBrowserWidget(bool deaths)
	{
		if (deaths)
		{
			Stat[] causes = Enum.GetValues<Stat>().Where(s => s.ToString().StartsWith("KilledBy", StringComparison.Ordinal)).ToArray();
			int total = causes.Sum(s => core.ProfileData.GetStat(s));
			rows.Add(new Row(SId.FACTS_h_total, "facts_field_death", total.ToString()));
			foreach (Stat cause in causes.OrderByDescending(s => core.ProfileData.GetStat(s)))
			{
				int count = core.ProfileData.GetStat(cause);
				rows.Add(new Row(SId.FACTS_h_causes, DeathIcon(cause), $"{count}   {(total == 0 ? 0 : count * 100 / total)}%"));
			}
			return;
		}
		int ticks = core.ProfileData.GetStat(Stat.TicksInGame);
		rows.Add(new Row(SId.FACTS_f_time_in_game, "facts_field_time", Duration(ticks)));
		Add(SId.FACTS_f_runs, "icon_play", Stat.Attempts);
		rows.Add(new Row(SId.FACTS_f_avg_run_duration, "icon_play", Duration(ticks / Math.Max(1, core.ProfileData.GetStat(Stat.Attempts)))));
		Add(SId.FACTS_f_distance_walked, "facts_field_steps_1", Stat.MetersWalked, "m");
		Add(SId.FACTS_f_distance_slid, "facts_field_slide_1", Stat.MetersSlided, "m");
		Add(SId.FACTS_f_jumps, "facts_field_pusher_1", Stat.JumpersUsed);
		Add(SId.FACTS_f_webs, "facts_field_web", Stat.WebsBroken);
		Add(SId.FACTS_f_pots, "pot_hit_0", Stat.PotsBroken);
		Add(SId.FACTS_f_chests, "chest_1", Stat.ChestsLooted);
		Add(SId.FACTS_f_coins, "coin_gold_1", Stat.CoinsCollected);
		Add(SId.SKILL_WOLFBITE_name, "lykos_bite", Stat.LykosBonesEaten);
		foreach (var item in new[] { (Stat.SlimesKilled, "slime_1"), (Stat.BatsKilled, "bat_1"),
			(Stat.WispsKilled, "facts_field_wisp_1"), (Stat.FollowersKilled, "facts_field_follower_1"), (Stat.SerpentsKilled, "facts_field_serpent_1") })
			Add(SId.FACTS_h_monsters_killed, item.Item2, item.Item1);
		foreach (var item in new[] { (Stat.SpikesBroken, "spikes_5"), (Stat.SawsBroken, "saw_1"),
			(Stat.CrossbowsBroken, "crossbow_1"), (Stat.RotobladesBroken, "facts_field_blade_1"),
			(Stat.PistonsBroken, "facts_field_piston_1"), (Stat.ZappersBroken, "facts_field_zapper_1") })
			Add(SId.FACTS_h_traps_destroyed, item.Item2, item.Item1);
	}
	private void Add(SId label, string icon, Stat stat, string suffix = "") =>
		rows.Add(new Row(label, icon, core.ProfileData.GetStat(stat) + suffix));
	private static string Duration(int ticks) { var t = TimeSpan.FromSeconds(ticks / 60); return $"{(int)t.TotalHours}h {t.Minutes}m {t.Seconds}s"; }
	private static string DeathIcon(Stat stat) => stat switch
	{
		Stat.KilledByGravity => "facts_field_fall_5", Stat.KilledBySlime => "slime_1",
		Stat.KilledByBat => "bat_1", Stat.KilledByCrossbow => "crossbow_1",
		Stat.KilledByRotoblade => "facts_field_blade_1", Stat.KilledByPiston => "facts_field_piston_1",
		Stat.KilledBySaw => "saw_1", Stat.KilledBySpikes => "spikes_5",
		Stat.KilledByZapper => "zapball_1", Stat.KilledByDarkness => "facts_field_grue_1",
		Stat.KilledByFlame => "grill_burn_1", Stat.KilledByAxe => "facts_field_axe_1",
		Stat.KilledByFollower => "facts_field_follower_1", Stat.KilledBySerpent => "facts_field_serpent_1",
		_ => "facts_field_death"
	};
	public void Update(float dt) { page = Math.Min(page, Pages - 1); }
	public void Adjust(int direction) => page = (page + direction + Pages) % Pages;
	public void Activate() => Adjust(1);
	public void HandleClick(Vector2 p) => Adjust(p.X < Bounds.Center.X ? -1 : 1);
	public void Draw(bool focused, int depth)
	{
		float width = (Bounds.Width - (Columns - 1) * 8f) / Columns;
		float height = (Bounds.Height - 24f) / RowsPerColumn;
		for (int i = 0; i < Capacity && page * Capacity + i < rows.Count; i++)
		{
			Row row = rows[page * Capacity + i];
			var box = new RectangleF(Bounds.X + (i % Columns) * (width + 8f), Bounds.Y + (i / Columns) * height, width, height - 4f);
			MenuTheme.Panel(R, box, depth);
			Sprite icon = _(row.Icon, "facts_field_death");
			float fit = Math.Min(1f, 24f / Math.Max(icon.Width, icon.Height));
			R["fg", depth + 2, false].DrawSpriteS(icon, box.TopLeft + new Vector2(18f, box.Height / 2f), Color.White, new Vector2(fit), origin: SpriteOrigin.Center);
			var text = TextProfile.OrangeBoldText.Alter(width: (int)box.Width - 44, height: 13, scale: 0.48f,
				boxAlignment: Alignment2D.Left, textAlignment: Alignment2D.Left);
			R["fg", depth + 3, false].DrawTextS(__(row.Label), box.TopLeft + new Vector2(36, 3), text);
			R["fg", depth + 3, false].DrawTextS(row.Value, box.TopLeft + new Vector2(36, 17), text.Alter(color: Color.White, scale: 0.65f));
		}
		R["fg", depth + 3, false].DrawTextS($"<   {__(SId.PC_page)} {page + 1} / {Pages}   >", new Vector2(Bounds.Center.X, Bounds.Bottom - 10),
			TextProfile.OrangeBoldText.Alter(width: (int)Bounds.Width, height: 16, scale: 0.5f, boxAlignment: Alignment2D.Middle, textAlignment: Alignment2D.Middle));
	}
}
