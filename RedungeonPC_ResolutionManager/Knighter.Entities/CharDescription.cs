using System;
using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class CharDescription
{
	public static Dictionary<Character, CharDescription> Get = new Dictionary<Character, CharDescription>
	{
		{
			Character.Knight,
			new CharDescription
			{
				EntityClass = typeof(KnightChar),
				Portrait = SpriteName.knight_portrait,
				Color1 = default(Color).FromRgb(3104695) * 0.7f,
				Color2 = default(Color).FromRgb(5593761) * 0.7f,
				BacklightDim = 0.4f,
				NameImage = "knight_name",
				Icon = SpriteName.knight_s1,
				ReviveSpriteName = "gylbard_revive_",
				ReviveShift = new Vector2(2f, -1f),
				AnimSequence = "knight_s|1234",
				Name = SId.CHAR_GYLBARD_name,
				Bio = SId.CHAR_GYLBARD_bio,
				RevealSound = SoundName.knight_reveal,
				SkullSprite = SpriteName.skull_gylbard,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(), 0),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Shield,
						1
					} }), 4, SId.CHAR_GYLBARD_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Shield,
							1
						},
						{
							Skill.Thrust,
							20
						}
					}), 32, SId.CHAR_GYLBARD_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Shield,
							2
						},
						{
							Skill.Thrust,
							15
						}
					}), 80, SId.CHAR_GYLBARD_l4, Skill.Shield),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Shield,
							3
						},
						{
							Skill.Thrust,
							10
						}
					}), 140, SId.CHAR_GYLBARD_l5)
				}
			}
		},
		{
			Character.Creep,
			new CharDescription
			{
				EntityClass = typeof(CreepChar),
				Portrait = SpriteName.creep_portrait,
				Color1 = default(Color).FromRgb(4026445) * 0.7f,
				Color2 = default(Color).FromRgb(548919) * 0.7f,
				BacklightDim = 0.7f,
				NameImage = "creep_name",
				Icon = SpriteName.creep_4,
				ReviveSpriteName = "creep_revive_",
				ReviveShift = new Vector2(5f, 3f),
				AnimSequence = "creep_|123345",
				AnimSpeed = 0.2f,
				Name = SId.CHAR_CREEP_name,
				Bio = SId.CHAR_CREEP_bio,
				RevealSound = SoundName.creep_reveal,
				SkullSprite = SpriteName.skull_creep,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Pogo,
						1
					} }), 12),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Pogo,
							1
						},
						{
							Skill.ScareCreatures,
							30
						}
					}), 10, SId.CHAR_CREEP_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Pogo,
							1
						},
						{
							Skill.ScareCreatures,
							15
						}
					}), 20, SId.CHAR_CREEP_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Pogo,
							1
						},
						{
							Skill.ScareCreatures,
							15
						},
						{
							Skill.Bridger,
							1
						}
					}), 40, SId.CHAR_CREEP_l4)
				}
			}
		},
		{
			Character.Vampire,
			new CharDescription
			{
				EntityClass = typeof(VampireChar),
				Portrait = SpriteName.vampire_portrait,
				Color1 = default(Color).FromRgb(15617214) * 0.4f,
				Color2 = default(Color).FromRgb(9054913) * 0.4f,
				BacklightDim = 1f,
				NameImage = "vampire_name",
				Icon = SpriteName.kazhan_s_4,
				ReviveSpriteName = "kazhan_revive_",
				ReviveShift = new Vector2(-1f, 3f),
				AnimSequence = "kazhan_s_|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_KAZHAN_name,
				Bio = SId.CHAR_KAZHAN_bio,
				RevealSound = SoundName.kazhan_reveal,
				RevealSoundDelay = 0,
				SkullSprite = SpriteName.skull_vampire,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.BatFriend,
						1
					} }), 28),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.BatFriend,
							1
						},
						{
							Skill.Flight,
							20
						}
					}), 16, SId.CHAR_KAZHAN_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.BatFriend,
							1
						},
						{
							Skill.Flight,
							20
						},
						{
							Skill.TurnIntoBat,
							1
						}
					}), 40, SId.CHAR_KAZHAN_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.BatFriend,
							1
						},
						{
							Skill.TurnIntoBat,
							2
						},
						{
							Skill.Flight,
							20
						}
					}), 60, SId.CHAR_KAZHAN_l4),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.BatFriend,
							1
						},
						{
							Skill.TurnIntoBat,
							2
						},
						{
							Skill.Flight,
							15
						}
					}), 80, SId.CHAR_KAZHAN_l5)
				}
			}
		},
		{
			Character.Nathan,
			new CharDescription
			{
				EntityClass = typeof(NathanChar),
				Portrait = SpriteName.nathan_portrait,
				Color1 = default(Color).FromRgb(3183740) * 0.6f,
				Color2 = default(Color).FromRgb(3789446) * 0.6f,
				BacklightDim = 0.7f,
				NameImage = "nathan_name",
				Icon = SpriteName.nathan_s_1,
				ReviveSpriteName = "nate_revive_",
				ReviveShift = new Vector2(2f, 1f),
				AnimSequence = "nathan_s_|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_NATE_name,
				Bio = SId.CHAR_NATE_bio,
				RevealSound = SoundName.nathan_reveal,
				RevealSoundDelay = -20,
				SkullSprite = SpriteName.skull_nathan,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Drone,
						1
					} }), 100),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Drones,
						1
					} }), 80, SId.CHAR_NATE_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Drones,
							1
						},
						{
							Skill.BreakTraps,
							45
						}
					}), 120, SId.CHAR_NATE_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Drones,
							1
						},
						{
							Skill.BreakTraps,
							30
						}
					}), 80, SId.CHAR_NATE_l4)
				}
			}
		},
		{
			Character.Ichitaka,
			new CharDescription
			{
				EntityClass = typeof(IchitakaChar),
				Portrait = SpriteName.ichitaka_portrait,
				Color1 = default(Color).FromRgb(15110181) * 0.4f,
				Color2 = default(Color).FromRgb(16751616) * 0.4f,
				BacklightDim = 0f,
				NameImage = "ichitaka_name",
				Icon = SpriteName.ichitaka_s_1,
				ReviveSpriteName = "ichi_revive_",
				AnimSequence = "ichitaka_s_|1213",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_ICHI_name,
				Bio = SId.CHAR_ICHI_bio,
				RevealSound = SoundName.ichitaka_reveal,
				RevealSoundDelay = -20,
				SkullSprite = SpriteName.skull_ichitaka,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.CoinMagnetRadius,
						1
					} }), 160),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.CoinMagnetRadius,
						2
					} }), 160, SId.CHAR_ICHI_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.CoinMagnetRadius,
						3
					} }), 200, SId.CHAR_ICHI_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.CoinMagnetRadius,
							3
						},
						{
							Skill.Telekinesis,
							10
						}
					}), 200, SId.CHAR_ICHI_l4)
				}
			}
		},
		{
			Character.Vesna,
			new CharDescription
			{
				EntityClass = typeof(VesnaChar),
				Portrait = SpriteName.vesna_portrait,
				Color1 = default(Color).FromRgb(4950763) * 0.4f,
				Color2 = default(Color).FromRgb(3563983) * 0.4f,
				BacklightDim = 1f,
				NameImage = "vesna_name",
				Icon = SpriteName.vesna_s1,
				ReviveSpriteName = "vesna_revive_",
				ReviveShift = new Vector2(4f, -1f),
				AnimSequence = "vesna_s|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_VESNA_name,
				Bio = SId.CHAR_VESNA_bio,
				RevealSound = SoundName.vesna_reveal,
				RevealSoundDelay = -30,
				SkullSprite = SpriteName.skull_vesna,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Flowers,
						1
					} }), 240),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Flowers,
							1
						},
						{
							Skill.ResistDarkness,
							30
						}
					}), 40, SId.CHAR_VESNA_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Flowers,
							1
						},
						{
							Skill.ResistDarkness,
							15
						}
					}), 40, SId.CHAR_VESNA_l3)
				}
			}
		},
		{
			Character.Mage,
			new CharDescription
			{
				EntityClass = typeof(MageChar),
				Portrait = SpriteName.mage_portrait,
				Color1 = default(Color).FromRgb(4513847) * 0.4f,
				Color2 = default(Color).FromRgb(12385345) * 0.4f,
				BacklightDim = 1f,
				NameImage = "mage_name",
				Icon = SpriteName.mage_s_3,
				ReviveSpriteName = "mage_revive_",
				ReviveShift = new Vector2(1f, -3f),
				AnimSequence = "mage_s_|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_AETHER_name,
				Bio = SId.CHAR_AETHER_bio,
				RevealSound = SoundName.mage_reveal,
				RevealSoundDelay = -20,
				SkullSprite = SpriteName.skull_mage,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Portals,
						1
					} }), 320),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Portals,
							1
						},
						{
							Skill.SloMo,
							25
						}
					}), 40, SId.CHAR_AETHER_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Portals,
							1
						},
						{
							Skill.SloMo,
							15
						}
					}), 40, SId.CHAR_AETHER_l3)
				}
			}
		},
		{
			Character.Rib,
			new CharDescription
			{
				EntityClass = typeof(RibChar),
				Portrait = SpriteName.rib_portrait,
				Color1 = default(Color).FromRgb(4513847) * 0.4f,
				Color2 = default(Color).FromRgb(16448577) * 0.4f,
				BacklightDim = 1f,
				NameImage = "rib_name",
				Icon = SpriteName.ribb_s_1,
				ReviveSpriteName = "ribb_revive_",
				ReviveShift = new Vector2(5f, 1f),
				AnimSequence = "ribb_s_|1122233444",
				AnimSpeed = 0.23f,
				Name = SId.CHAR_RIBB_name,
				Bio = SId.CHAR_RIBB_bio,
				RevealSound = SoundName.ribb_reveal,
				RevealSoundDelay = -20,
				SkullSprite = SpriteName.skull_ribb,
				CrossbowAnimation = false,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Undead,
							1
						},
						{
							Skill.SpareSkull,
							1
						}
					}), 400),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Undead,
							1
						},
						{
							Skill.SpareSkull,
							2
						}
					}), 60, SId.CHAR_RIBB_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Undead,
							1
						},
						{
							Skill.SpareSkull,
							3
						}
					}), 60, SId.CHAR_RIBB_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Undead,
							1
						},
						{
							Skill.SpareSkull,
							4
						}
					}), 60, SId.CHAR_RIBB_l4)
				}
			}
		},
		{
			Character.PanicBot,
			new CharDescription
			{
				EntityClass = typeof(PanicBotChar),
				Portrait = SpriteName.panicbot_portrait,
				Color1 = default(Color).FromRgb(7352817) * 0.8f,
				Color2 = default(Color).FromRgb(9375760) * 0.6f,
				BacklightDim = 1f,
				NameImage = "panicbot_name",
				Icon = SpriteName.panicbot_icon,
				ReviveSpriteName = "panicbot_revive_",
				ReviveShift = new Vector2(1f, 2f),
				AnimSequence = "panicbot_button_|12345678",
				AnimSpeed = 0.1f,
				ButtonAnimSpeedFactor = 2f,
				Name = SId.CHAR_PB_name,
				Bio = SId.CHAR_PB_bio,
				RevealSound = SoundName.panicbot_reveal,
				RevealSoundDelay = 20,
				SkullSprite = SpriteName.skull_panicbot,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Electronic,
						30
					} }), 400),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Electronic,
							30
						},
						{
							Skill.PanicLaser,
							15
						}
					}), 40, SId.CHAR_PB_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Electronic,
							30
						},
						{
							Skill.PanicLaser,
							10
						}
					}), 40, SId.CHAR_PB_l3, Skill.PanicLaser),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Electronic,
							40
						},
						{
							Skill.PanicLaser,
							10
						}
					}), 60, SId.CHAR_PB_l4, Skill.Electronic),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Electronic,
							40
						},
						{
							Skill.PanicLaser,
							5
						}
					}), 80, SId.CHAR_PB_l5, Skill.PanicLaser)
				}
			}
		},
		{
			Character.Medusa,
			new CharDescription
			{
				EntityClass = typeof(MedusaChar),
				Portrait = SpriteName.medusa_portrait_1,
				DrawPortraitUnderExtra = false,
				Color1 = default(Color).FromRgb(158087) * 0.7f,
				Color2 = default(Color).FromRgb(8774249) * 0.6f,
				BacklightDim = 0.7f,
				NameImage = "medusa_name",
				Icon = SpriteName.medusa_s_1,
				ReviveSpriteName = "medusa_revive_",
				ReviveShift = new Vector2(1f, -3f),
				AnimSequence = "medusa_s_|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_MEDUSA_name,
				Bio = SId.CHAR_MEDUSA_bio,
				RevealSound = SoundName.medousa_reveal,
				RevealSoundDelay = 10,
				SkullSprite = SpriteName.skull_medusa,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Petrification,
						1
					} }), 360),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Petrification,
							1
						},
						{
							Skill.SerpentsDexterity,
							1
						}
					}), 80, SId.CHAR_MEDUSA_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Petrification,
							2
						},
						{
							Skill.SerpentsDexterity,
							1
						}
					}), 100, SId.CHAR_MEDUSA_l3, Skill.Petrification)
				}
			}
		},
		{
			Character.Bragg,
			new CharDescription
			{
				EntityClass = typeof(BraggChar),
				Portrait = SpriteName.bragg_portrait_1,
				DrawPortraitUnderExtra = false,
				Color1 = default(Color).FromRgb(3169713) * 0.7f,
				Color2 = default(Color).FromRgb(5418213) * 0.6f,
				BacklightDim = 0.7f,
				NameImage = "bragg_name",
				Icon = SpriteName.bragg_s_1,
				ReviveSpriteName = "bragg_revive_",
				ReviveShift = new Vector2(2f, 4f),
				AnimSequence = "bragg_s_|1234",
				Name = SId.CHAR_BRAGG_name,
				Bio = SId.CHAR_BRAGG_bio,
				RevealSound = SoundName.bragg_reveal,
				RevealSoundDelay = 20,
				SkullSprite = SpriteName.skull_bragg,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.TreasureHunt,
						1
					} }), 240),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.TreasureHunt,
							1
						},
						{
							Skill.Parrot,
							1
						}
					}), 100, SId.CHAR_BRAGG_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.TreasureHunt,
							1
						},
						{
							Skill.Parrot,
							1
						},
						{
							Skill.Gunshot,
							1
						}
					}), 100, SId.CHAR_BRAGG_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.TreasureHunt,
							1
						},
						{
							Skill.Parrot,
							1
						},
						{
							Skill.Gunshot,
							2
						}
					}), 120, SId.CHAR_BRAGG_l4, Skill.Gunshot),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.TreasureHunt,
							1
						},
						{
							Skill.Parrot,
							1
						},
						{
							Skill.Gunshot,
							3
						}
					}), 120, SId.CHAR_BRAGG_l5, Skill.Gunshot)
				}
			}
		},
		{
			Character.Golem,
			new CharDescription
			{
				EntityClass = typeof(GolemChar),
				Portrait = SpriteName.golem_portrait,
				DrawPortraitUnderExtra = false,
				Color1 = default(Color).FromRgb(13123355) * 0.7f,
				Color2 = default(Color).FromRgb(16757760) * 0.6f,
				BacklightDim = 0.9f,
				NameImage = "golem_name",
				Icon = SpriteName.rik_s_1,
				ReviveSpriteName = "rik_revive_",
				ReviveShift = new Vector2(0f, 3f),
				AnimSequence = "rik_s_|1234",
				AnimSpeed = 0.1f,
				Name = SId.CHAR_RIK_name,
				Bio = SId.CHAR_RIK_bio,
				RevealSound = SoundName.rik_reveal,
				RevealSoundDelay = 10,
				SkullSprite = SpriteName.skull_rik,
				CrossbowAnimation = false,
				Levels = new List<CharLevel>
				{
					new CharLevel(new Abilities(new Dictionary<Skill, int> { 
					{
						Skill.Fireproof,
						1
					} }), 200),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Fireproof,
							1
						},
						{
							Skill.FireShield,
							1
						}
					}), 40, SId.CHAR_RIK_l2),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Fireproof,
							1
						},
						{
							Skill.FireShield,
							1
						},
						{
							Skill.Blaze,
							20
						}
					}), 60, SId.CHAR_RIK_l3),
					new CharLevel(new Abilities(new Dictionary<Skill, int>
					{
						{
							Skill.Fireproof,
							1
						},
						{
							Skill.BetterFireShield,
							1
						},
						{
							Skill.Blaze,
							20
						}
					}), 60, SId.CHAR_RIK_l4)
				}
			}
		}
		,
		{
			Character.Wolf,
			new CharDescription
			{
				EntityClass = typeof(WolfChar),
				Portrait = SpriteName.lykos_portrait,
				Color1 = default(Color).FromRgb(11737344) * 0.8f,
				Color2 = default(Color).FromRgb(3169713) * 0.7f,
				BacklightDim = 0.65f,
				NameImage = "lykos_name",
				Icon = SpriteName.lykos_icon,
				ReviveSpriteName = "lykos_revive_",
				AnimSequence = "lykos_human_s_|1234",
				Name = SId.CHAR_LYKOS_name,
				Bio = SId.CHAR_LYKOS_bio,
				RevealSound = SoundName.kazhan_turn,
				SkullSprite = SpriteName.lykos_skull,
				CrossbowAnimation = false,
				Levels = new List<CharLevel>
				{
					// CharLevel prices are in units of 25 coins: 4 means 100.
					new CharLevel(new Abilities(new Dictionary<Skill, int> { { Skill.WolfBite, 1 } }), 4),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { { Skill.WolfBite, 1 }, { Skill.FullMoon, 20 } }), 6, SId.CHAR_LYKOS_l2, Skill.FullMoon),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { { Skill.WolfBite, 1 }, { Skill.FullMoon, 15 } }), 10, SId.CHAR_LYKOS_l3, Skill.FullMoon),
					new CharLevel(new Abilities(new Dictionary<Skill, int> { { Skill.WolfBite, 1 }, { Skill.FullMoon, 15 }, { Skill.WolfReinforcement, 1 } }), 16, SId.CHAR_LYKOS_l4, Skill.WolfReinforcement)
				}
			}
		}
	};

	public Type EntityClass;

	public SpriteName Portrait;

	public bool DrawPortraitUnderExtra = true;

	public string NameImage;

	public SpriteName Icon;

	public string ReviveSpriteName;

	public Vector2 ReviveShift;

	public string AnimSequence;

	public float AnimSpeed = 0.095f;

	public float ButtonAnimSpeedFactor = 1f;

	public SId Name;

	public SId Bio;

	public Color Color1;

	public Color Color2;

	public float BacklightDim;

	public SpriteName SkullSprite;

	public SoundName RevealSound;

	public int RevealSoundDelay;

	public bool CrossbowAnimation = true;

	public bool FallAnimation = true;

	public List<CharLevel> Levels;

	public int UnlockPrice => Levels[0].Price;

	public static int GetOverallPrice(bool withUnlocks = true, bool withUpgrades = true)
	{
		int num = 0;
		foreach (Character value in Enum.GetValues(typeof(Character)))
		{
			CharDescription charDescription = Get[value];
			if (withUnlocks)
			{
				num += charDescription.UnlockPrice;
			}
			if (withUpgrades)
			{
				for (int i = 1; i < charDescription.Levels.Count; i++)
				{
					num += charDescription.Levels[i].Price;
				}
			}
		}
		return num;
	}
}
