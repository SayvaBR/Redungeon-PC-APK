using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Localization;

namespace Knighter.Entities;

public class Abilities
{
	public static Dictionary<Skill, AbilityDesc> SkillDesc = new Dictionary<Skill, AbilityDesc>
	{
		{ Skill.WolfBite, new AbilityDesc {
			Name = SId.SKILL_WOLFBITE_name, Description = SId.SKILL_WOLFBITE_desc,
			Kind = AbilityKind.Permanent, HudMainIcon = SpriteName.lykos_bite } },
		{ Skill.FullMoon, new AbilityDesc {
			Name = SId.SKILL_FULLMOON_name, Description = SId.SKILL_FULLMOON_desc,
			Kind = AbilityKind.Rechargeable, HudMainIcon = SpriteName.lykos_moon,
			HudChargeBar = SpriteName.skill_charge_brass,
			Color1 = 0x593427, Color2 = 0xA05C32, Color3 = 0xFFC66B } },
		{ Skill.WolfReinforcement, new AbilityDesc {
			Name = SId.SKILL_REINFORCEMENT_name, Description = SId.SKILL_REINFORCEMENT_desc,
			Kind = AbilityKind.Permanent, HudMainIcon = SpriteName.lykos_reinforce } },
		{
			Skill.Shield,
			new AbilityDesc
			{
				Name = SId.SKILL_SHIELD_name,
				Description = SId.SKILL_SHIELD_desc,
				HudItemIcon = SpriteName.skill_token_shield,
				HudItemSlot = SpriteName.skill_token_shield_slot,
				Kind = AbilityKind.Consumable
			}
		},
		{
			Skill.Thrust,
			new AbilityDesc
			{
				Name = SId.SKILL_THRUST_name,
				Description = SId.SKILL_THRUST_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_thrust,
				HudChargeBar = SpriteName.skill_charge_sword,
				Color1 = 4011378,
				Color2 = 4607102,
				Color3 = 11446484
			}
		},
		{
			Skill.Portals,
			new AbilityDesc
			{
				Name = SId.SKILL_PORTALS_name,
				Description = SId.SKILL_PORTALS_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_portal,
				Illustration = SpriteName.illustration_portals
			}
		},
		{
			Skill.SloMo,
			new AbilityDesc
			{
				Name = SId.SKILL_TIMEBEND_name,
				Description = SId.SKILL_TIMEBEND_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_sandclock,
				HudChargeBar = SpriteName.skill_charge_magic,
				Color1 = 1008705,
				Color2 = 2129698,
				Color3 = 7585298
			}
		},
		{
			Skill.Flowers,
			new AbilityDesc
			{
				Name = SId.SKILL_GREENPATH_name,
				Description = SId.SKILL_GREENPATH_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_flower,
				Illustration = SpriteName.illustration_green_path
			}
		},
		{
			Skill.ResistDarkness,
			new AbilityDesc
			{
				Name = SId.SKILL_LIGHTRITUAL_name,
				Description = SId.SKILL_LIGHTRITUAL_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_sun,
				HudChargeBar = SpriteName.skill_charge_sun,
				Color1 = 10172160,
				Color2 = 14119438,
				Color3 = 15907142
			}
		},
		{
			Skill.Pogo,
			new AbilityDesc
			{
				Name = SId.SKILL_COBWEB_name,
				Description = SId.SKILL_COBWEB_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_pogo,
				Illustration = SpriteName.illustration_pogo
			}
		},
		{
			Skill.ScareCreatures,
			new AbilityDesc
			{
				Name = SId.SKILL_BOO_name,
				Description = SId.SKILL_BOO_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_melon,
				HudChargeBar = SpriteName.skill_charge_melon,
				Color1 = 1980440,
				Color2 = 1988156,
				Color3 = 6070870
			}
		},
		{
			Skill.Bridger,
			new AbilityDesc
			{
				Name = SId.SKILL_BRIDGER_name,
				Description = SId.SKILL_BRIDGER_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_bridger,
				Illustration = SpriteName.illustration_bridger
			}
		},
		{
			Skill.CoinMagnetRadius,
			new AbilityDesc
			{
				Name = SId.SKILL_MAGNET_name,
				Description = SId.SKILL_MAGNET_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_magnet,
				Illustration = SpriteName.illustration_coin_magnet
			}
		},
		{
			Skill.Telekinesis,
			new AbilityDesc
			{
				Name = SId.SKILL_TELEKINESIS_name,
				Description = SId.SKILL_TELEKINESIS_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_telekinesis,
				HudChargeBar = SpriteName.skill_charge_sun,
				Color1 = 10172160,
				Color2 = 14119438,
				Color3 = 15907142
			}
		},
		{
			Skill.BreakTraps,
			new AbilityDesc
			{
				Name = SId.SKILL_TARGET_name,
				Description = SId.SKILL_TARGET_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_wrench,
				HudChargeBar = SpriteName.skill_charge_brass,
				Color1 = 5903378,
				Color2 = 10103594,
				Color3 = 16748624
			}
		},
		{
			Skill.Drone,
			new AbilityDesc
			{
				Name = SId.SKILL_MECHFRIEND_name,
				Description = SId.SKILL_MECHFRIEND_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_drone
			}
		},
		{
			Skill.Drones,
			new AbilityDesc
			{
				Name = SId.SKILL_MECHSQUAD_name,
				Description = SId.SKILL_MECHSQUAD_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_drones
			}
		},
		{
			Skill.BatFriend,
			new AbilityDesc
			{
				Name = SId.SKILL_BATFRIEND_name,
				Description = SId.SKILL_BATFRIEND_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_bat_love,
				Illustration = SpriteName.illustration_bat_friend
			}
		},
		{
			Skill.TurnIntoBat,
			new AbilityDesc
			{
				Name = SId.SKILL_ESCABYSS_name,
				Description = SId.SKILL_ESCABYSS_desc,
				HudItemIcon = SpriteName.skill_token_bat,
				HudItemSlot = SpriteName.skill_token_bat_slot,
				Kind = AbilityKind.Consumable,
				Illustration = SpriteName.illustration_bat_escape
			}
		},
		{
			Skill.Flight,
			new AbilityDesc
			{
				Name = SId.SKILL_DARKWING_name,
				Description = SId.SKILL_DARKWING_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_bat_up,
				HudChargeBar = SpriteName.skill_charge_bat,
				Color1 = 4076369,
				Color2 = 5591918,
				Color3 = 9080738
			}
		},
		{
			Skill.Undead,
			new AbilityDesc
			{
				Name = SId.SKILL_UNDEAD_name,
				Description = SId.SKILL_UNDEAD_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_undead,
				Illustration = SpriteName.illustration_undead
			}
		},
		{
			Skill.SpareSkull,
			new AbilityDesc
			{
				Name = SId.SKILL_SPAREHEAD_name,
				Description = SId.SKILL_SPAREHEAD_desc,
				HudItemIcon = SpriteName.skill_token_skull,
				HudItemSlot = SpriteName.skill_token_skull_slot,
				Kind = AbilityKind.Consumable
			}
		},
		{
			Skill.Electronic,
			new AbilityDesc
			{
				Name = SId.SKILL_ELECTRO_name,
				Description = SId.SKILL_ELECTRO_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_electronic,
				Illustration = SpriteName.illustration_electronic
			}
		},
		{
			Skill.PanicLaser,
			new AbilityDesc
			{
				Name = SId.SKILL_PANICLASER_name,
				Description = SId.SKILL_PANICLASER_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_lasers,
				HudChargeBar = SpriteName.skill_charge_brass,
				Color1 = 4656974,
				Color2 = 5579445,
				Color3 = 12039144,
				HideChargeBar = true
			}
		},
		{
			Skill.Petrification,
			new AbilityDesc
			{
				Name = SId.SKILL_PETRI_name,
				Description = SId.SKILL_PETRI_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_petri,
				Illustration = SpriteName.illustration_petri
			}
		},
		{
			Skill.SerpentsDexterity,
			new AbilityDesc
			{
				Name = SId.SKILL_DEXTERITY_name,
				Description = SId.SKILL_DEXTERITY_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_snake
			}
		},
		{
			Skill.TreasureHunt,
			new AbilityDesc
			{
				Name = SId.SKILL_TREASUREHUNT_name,
				Description = SId.SKILL_TREASUREHUNT_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_treasure_hunt,
				Illustration = SpriteName.illustration_treasure_hunt
			}
		},
		{
			Skill.Parrot,
			new AbilityDesc
			{
				Name = SId.SKILL_GEM_name,
				Description = SId.SKILL_GEM_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_parrot,
				Illustration = SpriteName.illustration_parrot
			}
		},
		{
			Skill.Gunshot,
			new AbilityDesc
			{
				Name = SId.SKILL_GUNSHOT_name,
				Description = SId.SKILL_GUNSHOT_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_gun,
				HudChargeBar = SpriteName.skill_charge_bat,
				Color1 = 4657675,
				Color2 = 7618598,
				Color3 = 12873008,
				HideChargeBar = true
			}
		},
		{
			Skill.Fireproof,
			new AbilityDesc
			{
				Name = SId.SKILL_FIREPROOF_name,
				Description = SId.SKILL_FIREPROOF_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_fireproof,
				Illustration = SpriteName.illustration_fireproof
			}
		},
		{
			Skill.FireShield,
			new AbilityDesc
			{
				Name = SId.SKILL_FSHIELD_name,
				Description = SId.SKILL_FSHIELD_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_fireshield
			}
		},
		{
			Skill.BetterFireShield,
			new AbilityDesc
			{
				Name = SId.SKILL_FSHIELD_name,
				Description = SId.SKILL_FSHIELD_desc,
				Kind = AbilityKind.Permanent,
				HudMainIcon = SpriteName.skill_hud_better_fireshield
			}
		},
		{
			Skill.Blaze,
			new AbilityDesc
			{
				Name = SId.SKILL_BLAZE_name,
				Description = SId.SKILL_BLAZE_desc,
				Kind = AbilityKind.Rechargeable,
				HudMainIcon = SpriteName.skill_hud_blaze,
				HudChargeBar = SpriteName.skill_charge_sun,
				Color1 = 10172160,
				Color2 = 14119438,
				Color3 = 15907142,
				HideChargeBar = true
			}
		}
	};

	public readonly Dictionary<Skill, int> SkillLevel;

	public readonly Dictionary<Skill, float> SkillCharge;

	public Abilities(Dictionary<Skill, int> skillLevel = null)
	{
		SkillLevel = new Dictionary<Skill, int>();
		SkillCharge = new Dictionary<Skill, float>();
		foreach (Skill value2 in Enum.GetValues(typeof(Skill)))
		{
			int value = ((skillLevel != null && skillLevel.ContainsKey(value2)) ? skillLevel[value2] : 0);
			SkillLevel.Add(value2, value);
			SkillCharge.Add(value2, 1f);
		}
	}

	public Abilities Clone()
	{
		return new Abilities(SkillLevel);
	}
}
