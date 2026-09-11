using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.States;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Gameplay;

public class LevelGenerator : Component
{
	public class GeneratedModule
	{
		public LevelModule Module;

		public int WorldEnterX;

		public int WorldEnterY;

		public readonly List<Tile> Tiles;

		public bool Flipped;

		public bool Released;

		public string Id;

		public List<TeleportEntity> Teleports;

		public List<int> TeleportColors;

		public int RandomAssociatedNumber { get; private set; }

		public GeneratedModule()
		{
			Tiles = new List<Tile>();
			RandomAssociatedNumber = SciHelper.GetRandom();
			Teleports = new List<TeleportEntity>();
			TeleportColors = new List<int>();
			TeleportColors.Add(0);
			BagOf<int> bagOf = new BagOf<int>().Put(1).Put(2).Put(3)
				.Put(4);
			for (int i = 1; i < 5; i++)
			{
				TeleportColors.Add(bagOf.DrawAndRemove());
			}
		}
	}

	public readonly int BaseDistance;

	private readonly PlayState playState;

	private readonly TileMap tileMap;

	private BagOf<LevelModule> BagOfModules;

	private LevelModule FirstModule;

	private int currentX;

	private int currentY;

	private int lastDistance;

	private readonly List<GeneratedModule> generatedModules;

	private int currentMilestone;

	private int nextMilestoneY;

	private const int BONUS_GROUP = 30;

	private int skullKeysSpawned;

	private int skullChestsSpawned;

	private int skullChestsLevel = 1;

	private bool makeLockedChest;

	private bool makeParrotCage;

	private int nextParrotCageDistance = -1;

	public int SpawnY { get; private set; }

	private int distance => BaseDistance - (currentY + FirstModule.Height - FirstModule.SpawnY - 1);

	private int nextSkullLock
	{
		get
		{
			if (skullChestsLevel != 1)
			{
				if (skullChestsLevel != 2)
				{
					return 13;
				}
				return 7;
			}
			return 3;
		}
	}

	public GeneratedModule CurrentModule { get; private set; }

	public LevelGenerator(PlayState playState, int baseDistance = 0)
	{
		BaseDistance = baseDistance;
		currentY = -baseDistance;
		this.playState = playState;
		tileMap = playState.TileMap;
		switch (base.core.Holiday)
		{
		case Holiday.Xmas:
		{
			List<LevelModule> list2 = base.core.LevelModules[LevelModuleType.Starting].FindAll((LevelModule m) => m.Name.Contains("xmas"));
			FirstModule = list2[Component._rnd(0, list2.Count - 1)];
			break;
		}
		case Holiday.ChunJie:
		{
			List<LevelModule> list3 = base.core.LevelModules[LevelModuleType.Starting].FindAll((LevelModule m) => m.Group == 12);
			FirstModule = list3[Component._rnd(0, list3.Count - 1)];
			break;
		}
		default:
		{
			List<LevelModule> list = base.core.LevelModules[LevelModuleType.Starting].FindAll((LevelModule m) => m.Group == 1);
			FirstModule = list[Component._rnd(0, list.Count - 1)];
			break;
		}
		}
		BagOfModules = new BagOf<LevelModule>();
		foreach (LevelModule item in base.core.LevelModules[LevelModuleType.Corridor])
		{
			BagOfModules.Put(item);
		}
		generatedModules = new List<GeneratedModule>();
	}

	public void Begin()
	{
		StartLevel();
	}

	private void StartLevel()
	{
		SpawnModule(FirstModule, LevelModuleType.Starting);
		SpawnY = -FirstModule.Height + FirstModule.SpawnY + 1;
		SpawnPlayer(0);
		lastDistance = -1;
		SetNextMilestone();
		Update();
	}

	private void SetNextMilestone()
	{
		currentMilestone++;
		nextMilestoneY = currentY - Component._rnd(75, 350);
	}

	public override void Unload()
	{
		for (int i = 0; i < generatedModules.Count; i++)
		{
			ReleaseGeneratedModule(i);
		}
		base.Unload();
	}

	private bool JustPassed(int milestone)
	{
		if (lastDistance < milestone)
		{
			return distance >= milestone;
		}
		return false;
	}

	private int GetModuleDifficulty()
	{
		int result = 1;
		if (SciHelper.ChanceRoll(0.35f))
		{
			return result;
		}
		float num = Component._M(Component._m((float)(distance + 50) / 200f, 1f), 0f);
		if (SciHelper.ChanceRoll(num * (num * num)))
		{
			result = 2;
		}
		if (SciHelper.ChanceRoll(0.35f))
		{
			return result;
		}
		float num2 = Component._M(Component._m((float)(distance + 50) / 300f, 1f), 0f);
		if (SciHelper.ChanceRoll(num2 * (num2 * num2)))
		{
			result = 3;
		}
		if (SciHelper.ChanceRoll(0.3f))
		{
			return result;
		}
		float num3 = Component._M(Component._m((float)(distance - 30) / 300f, 1f), 0f);
		if (SciHelper.ChanceRoll(num3 * (num3 * num3)))
		{
			result = 4;
		}
		if (SciHelper.ChanceRoll(0.8f))
		{
			return result;
		}
		float num4 = Component._M(Component._m((float)(distance - 100) / 300f, 1f), 0f);
		if (SciHelper.ChanceRoll(num4 * (num4 * num4)))
		{
			result = 5;
		}
		return result;
	}

	private void ProgressLevel()
	{
		if (distance > lastDistance)
		{
			lastDistance = distance;
		}
	}

	public override void Update()
	{
		bool flag = playState.Player != null && playState.Player.Tile != null;
		int num = (flag ? playState.Player.Tile.Y : 0) - base.core.Renderer.ScreenHeight / 16 - 5;
		bool flag2 = true;
		while (currentY > num && flag2)
		{
			ProgressLevel();
			int difficulty = GetModuleDifficulty();
			flag2 = SpawnModule(BagOfModules.Draw((LevelModule m) => m.Group == difficulty), LevelModuleType.Corridor);
			if (currentY <= nextMilestoneY)
			{
				SpawnModule(BagOfModules.Draw((LevelModule m) => m.Group == 30), LevelModuleType.Corridor);
				SetNextMilestone();
			}
			if (base.core.ProfileData.Character == Character.PanicBot && generatedModules.Count % 10 == 0)
			{
				SpawnModule(BagOfModules.Draw((LevelModule m) => m.Group == 26), LevelModuleType.Corridor);
			}
			if (base.core.ProfileData.Character == Character.Bragg)
			{
				if ((float)skullKeysSpawned >= (float)nextSkullLock * ((skullChestsLevel == 1) ? 1f : ((skullChestsLevel == 2) ? 0.75f : 0.5f)))
				{
					makeLockedChest = true;
					skullKeysSpawned = 0;
					skullChestsSpawned++;
					skullChestsLevel = ((skullKeysSpawned > 5) ? 3 : ((skullChestsSpawned <= 2) ? 1 : 2));
					SpawnModule(BagOfModules.Draw((LevelModule m) => m.Group == difficulty && m.HasElement(ElementType.Chest)), LevelModuleType.Corridor);
				}
				if (base.core.ProfileData.CurrentCharLevel > 1)
				{
					if (nextParrotCageDistance > 0 && distance > nextParrotCageDistance)
					{
						makeParrotCage = true;
					}
					if (nextParrotCageDistance < 0 || distance > nextParrotCageDistance)
					{
						nextParrotCageDistance = distance + Component._rnd(50, 200);
					}
				}
			}
			if (base.core.Holiday == Holiday.Xmas && generatedModules.Count % 25 == 0)
			{
				List<LevelModule> list = base.core.LevelModules[LevelModuleType.Corridor].FindAll((LevelModule m) => m.Name.Contains("xmas"));
				LevelModule module = list[Component._rnd(0, list.Count - 1)];
				SpawnModule(module, LevelModuleType.Corridor);
			}
			if (base.core.Holiday == Holiday.ChunJie && generatedModules.Count % 25 == 0)
			{
				List<LevelModule> list2 = base.core.LevelModules[LevelModuleType.Corridor].FindAll((LevelModule m) => m.Group == 12);
				LevelModule module2 = list2[Component._rnd(0, list2.Count - 1)];
				SpawnModule(module2, LevelModuleType.Corridor);
			}
		}
		if (flag)
		{
			int num2 = FindGeneratedModuleIndex(playState.Player.Tile.Y);
			if (num2 >= 5)
			{
				ReleaseGeneratedModule(num2 - 5);
			}
		}
	}

	private void ReleaseGeneratedModule(int index)
	{
		GeneratedModule generatedModule = generatedModules[index];
		if (generatedModule.Released)
		{
			return;
		}
		foreach (Tile tile in generatedModule.Tiles)
		{
			playState.TileMap.RemoveTile(tile);
		}
		generatedModule.Tiles.Clear();
		generatedModule.Released = true;
	}

	private bool SpawnModule(LevelModule module, LevelModuleType lmt, int replaceIndex = -1, bool? forceFlipped = null)
	{
		if (module == null)
		{
			return false;
		}
		bool flag = forceFlipped ?? SciHelper.ChanceRoll();
		int num = ((!flag) ? (currentX - module.EnterX) : (currentX - (module.Width - module.EnterX - 1)));
		int num2 = currentY - module.Height + 1;
		CurrentModule = new GeneratedModule();
		CurrentModule.Module = module;
		CurrentModule.Flipped = flag;
		CurrentModule.Id = module.Id;
		if (lmt == LevelModuleType.Corridor || lmt == LevelModuleType.Ending)
		{
			CurrentModule.WorldEnterX = currentX;
			CurrentModule.WorldEnterY = currentY;
		}
		for (int i = 0; i < module.Height; i++)
		{
			int y = num2 + i;
			for (int j = 0; j < 7; j++)
			{
				int x = num - 1 - j;
				SpawnDungeonTile(x, y, TileType.Pit, CurrentModule, sidePit: true);
				x = num + module.Width + j;
				SpawnDungeonTile(x, y, TileType.Pit, CurrentModule, sidePit: true);
			}
			for (int k = 0; k < module.Width; k++)
			{
				int x2 = ((!flag) ? (num + k) : (num + module.Width - k - 1));
				TileType tileType = ((module[k, i].ElementType == ElementType.Fragile) ? TileType.Fragile : module[k, i].TileType);
				SpawnDungeonTile(x2, y, tileType, CurrentModule);
				PopulateTile(module[k, i].Flip(flag), x2, y);
			}
		}
		if (lmt == LevelModuleType.Starting)
		{
			for (int l = 0; l < module.Width; l++)
			{
				int x3 = num + l;
				int y2 = num2 + module.Height;
				SpawnDungeonTile(x3, y2, TileType.Pit, CurrentModule);
			}
		}
		if (lmt == LevelModuleType.Starting)
		{
			CurrentModule.WorldEnterX = num + ((!flag) ? module.SpawnX : (module.Width - module.SpawnX - 1));
			CurrentModule.WorldEnterY = num2 + module.SpawnY;
		}
		currentX = num + ((!flag) ? module.ExitX : (module.Width - module.ExitX - 1));
		currentY = num2 - 1;
		if (replaceIndex < 0)
		{
			generatedModules.Add(CurrentModule);
		}
		else
		{
			generatedModules[replaceIndex] = CurrentModule;
		}
		return true;
	}

	public void ResetFirstModule()
	{
		int num = currentX;
		int num2 = currentY;
		currentX = 0;
		currentY = 0;
		bool flipped = generatedModules[0].Flipped;
		ReleaseGeneratedModule(0);
		SpawnModule(FirstModule, LevelModuleType.Starting, 0, flipped);
		currentX = num;
		currentY = num2;
	}

	private void SpawnDungeonTile(int x, int y, TileType tileType, GeneratedModule currentModule, bool sidePit = false)
	{
		DungeonTile dungeonTile = new DungeonTile(x, y, tileType);
		tileMap.AddTile(dungeonTile);
		currentModule.Tiles.Add(dungeonTile);
		if (base.core.Holiday == Holiday.ChunJie && sidePit && SciHelper.ChanceRoll(0.07f))
		{
			SpawnEntity(new ChunJieLanternEntity(x, y), null);
		}
	}

	public void PopulateTile(TileDesc desc, int x, int y, PlatformEntity platform = null)
	{
		bool flag = desc.TileType == TileType.Floor;
		TileType tileType = desc.TileType;
		if (tileType == TileType.Wall)
		{
			SpawnEntity(new WallEntity(x, y), platform);
			flag = false;
		}
		switch (desc.ElementType)
		{
		case ElementType.Chest:
			if (desc["is-mimic"] > 0 && !desc.ParentModule.Name.Contains("xmas"))
			{
				bool flag3 = true;
				if (desc["is-mimic"] == 1)
				{
					flag3 = SciHelper.ChanceRoll((distance < 50) ? 0.1f : ((distance < 200) ? 0.2f : ((distance < 300) ? 0.3f : 0.4f)));
				}
				if (flag3)
				{
					SpawnEntity(new FollowerEntity(x, y, null, FollowerKind.Red, distance), platform);
					flag = false;
					break;
				}
			}
			if (desc["kind"] == 0)
			{
				SpawnEntity(MakeChest(x, y, desc), platform);
			}
			else
			{
				SpawnEntity(new ChestEntity(x, y, new ChestContents(ItemType.GoldCoin, 100), ChestEntity.ChestTier.Treasure), platform);
			}
			flag = false;
			break;
		case ElementType.SawRail:
			SpawnEntity(new SawRailEntity(x, y), platform);
			break;
		case ElementType.Saw:
			SpawnEntity(new SawRailEntity(x, y), platform);
			SpawnEntity(new SawEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Spikes:
		{
			int num2 = desc["kind"];
			if (num2 == -1 || (num2 > 0 && CurrentModule.RandomAssociatedNumber % 2 == num2 % 2))
			{
				SpawnEntity(new SpikesEntity(x, y, desc), platform);
			}
			else
			{
				SpawnEntity(new GrillEntity(x, y, desc), platform);
			}
			break;
		}
		case ElementType.Torch:
			SpawnEntity(new TorchEntity(x, y, desc), platform);
			break;
		case ElementType.Piston:
			SpawnEntity(new PistonEntity(x, y, desc), platform);
			break;
		case ElementType.Platform:
			SpawnEntity(new PlatformEntity(x, y, desc), platform);
			break;
		case ElementType.Bat:
			SpawnEntity(new BatEntity(x, y, desc), platform);
			break;
		case ElementType.Loot:
			SpawnEntity(MakeLoot(x, y, desc["value"]), platform);
			flag = false;
			break;
		case ElementType.Crossbow:
			SpawnEntity(new CrossbowEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Obstacle:
			SpawnEntity(new ObstacleEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Ghost:
			SpawnEntity(new GhostEntity(x, y), platform);
			flag = false;
			break;
		case ElementType.Rotoblade:
			SpawnEntity(new RotobladeEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Empty:
			flag = false;
			break;
		case ElementType.Pusher:
			SpawnEntity(new PusherEntity(x, y, desc), platform);
			flag = true;
			break;
		case ElementType.Slime:
			SpawnEntity(new SlimeEntity(x, y, desc), platform);
			flag = true;
			break;
		case ElementType.Fragile:
			switch (desc["coin"])
			{
			case -1:
				flag = false;
				break;
			case 0:
				flag = true;
				break;
			case 1:
				SpawnEntity(MakeLoot(x, y), platform);
				flag = false;
				break;
			}
			break;
		case ElementType.Spider:
			SpawnEntity(new SpiderEntity(x, y, desc), platform);
			flag = true;
			break;
		case ElementType.Web:
			SpawnEntity(new WebEntity(x, y, desc, distance), platform);
			flag = true;
			break;
		case ElementType.Pot:
			if (SciHelper.ChanceRoll((float)desc["chance"] / 10f))
			{
				SpawnEntity(new PotEntity(x, y, desc), platform);
				flag = false;
			}
			else
			{
				flag = true;
			}
			break;
		case ElementType.Fountain:
			SpawnEntity(new FountainEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Door:
			SpawnEntity(new DoorEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Object:
			switch ((LevelObjectKind)desc["kind"])
			{
			case LevelObjectKind.ChestLottery:
				SpawnEntity(new ChestLotteryEntity(x, y), platform);
				flag = false;
				break;
			case LevelObjectKind.XmasTree:
				SpawnEntity(new XmasTreeEntity(x, y), platform);
				flag = false;
				break;
			case LevelObjectKind.ChunJieSoundEmitter:
				SpawnEntity(new SoundEmitterEntity(x, y), platform);
				flag = true;
				break;
			}
			break;
		case ElementType.Zapper:
			SpawnEntity(new ZapperEntity(x, y, desc).SetModule(CurrentModule), platform);
			flag = false;
			break;
		case ElementType.Text:
			SpawnEntity(new TextEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Statue:
			SpawnEntity(new StatueEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Firewall:
		{
			int num4 = desc["kind"];
			bool flag4 = num4 == -1 || (num4 > 0 && (CurrentModule.RandomAssociatedNumber % 2 == num4 % 2 || (base.core.ProfileData.Character == Character.Golem && SciHelper.ChanceRoll(0.3f))));
			SpawnEntity(new FirewallEntity(x, y, desc, (!flag4) ? BallType.Zap : BallType.Fire), platform);
			flag = false;
			break;
		}
		case ElementType.Follower:
			if (desc["is-pad"] == 1)
			{
				SpawnEntity(new FollowerPadEntity(x, y, chestBase: false), platform);
			}
			else
			{
				SpawnEntity(new FollowerEntity(x, y, desc), platform);
			}
			flag = false;
			break;
		case ElementType.Wisp:
			SpawnEntity(new WispEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.Cannon:
		{
			int num3 = desc["kind"];
			bool flag2 = num3 == -1 || (num3 > 0 && (CurrentModule.RandomAssociatedNumber % 2 == num3 % 2 || (base.core.ProfileData.Character == Character.Golem && SciHelper.ChanceRoll(0.2f))));
			SpawnEntity(new CannonEntity(x, y, desc, (!flag2) ? BallType.Zap : BallType.Fire), platform);
			flag = false;
			break;
		}
		case ElementType.Teleport:
		{
			TeleportEntity teleportEntity = new TeleportEntity(x, y, desc, CurrentModule.Teleports, CurrentModule.TeleportColors);
			SpawnEntity(teleportEntity, platform);
			CurrentModule.Teleports.Add(teleportEntity);
			flag = false;
			break;
		}
		case ElementType.Box:
			SpawnEntity(new BoxEntity(x, y, desc), platform);
			flag = false;
			break;
		case ElementType.ItemRail:
			SpawnEntity(new ItemRailEntity(x, y, desc, CurrentModule.RandomAssociatedNumber), platform);
			flag = false;
			break;
		case ElementType.Blocker:
		{
			string text = desc.Str("blockers");
			int trigger = int.Parse(desc.Str("trigger"));
			string[] array = desc.Str("action").Split(new char[1] { ',' });
			int num = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '+')
				{
					SpawnEntity(new BlockerEntity(x, y, i - 2, array[num], trigger, desc.Flipped), platform);
					num++;
				}
			}
			flag = false;
			break;
		}
		case ElementType.Button:
			SpawnEntity(new ButtonEntity(x, y, desc), platform);
			flag = true;
			break;
		}
		float chance = 0.07f;
		if (CurrentModule.Module.HasElement(ElementType.ItemRail))
		{
			flag = false;
		}
		if (currentY < 0 && flag && SciHelper.ChanceRoll(chance))
		{
			SpawnEntity(MakeLoot(x, y), platform);
		}
	}

	private void SpawnEntity(Entity newEntity, PlatformEntity platform)
	{
		SendMessage(new SpawnEntityMessage(newEntity, platform));
	}

	public void SpawnPlayer(int generatedModuleIndex)
	{
		GeneratedModule generatedModule = generatedModules[generatedModuleIndex];
		if (playState.Player == null || playState.Player.Dead)
		{
			playState.Player = (PlayerEntity)Activator.CreateInstance(base.core.CurrentCharDesc.EntityClass, generatedModule.WorldEnterX, generatedModule.WorldEnterY);
			playState.Player.InitAbilities(null);
			SendMessage(new SpawnEntityMessage(playState.Player, null));
		}
		else
		{
			playState.Player.TryMoveToCoordinates(playState.TileMap, new Vector2(generatedModule.WorldEnterX, generatedModule.WorldEnterY));
		}
	}

	public void RespawnPlayer()
	{
		int num = FindGeneratedModuleIndex(playState.Session.MaxPlayerY);
		if (num > 0 && num + 1 < generatedModules.Count)
		{
			num++;
		}
		if (num >= 0 && num < generatedModules.Count)
		{
			Abilities abilities = playState.Player.Abilities;
			SpawnPlayer(num);
			playState.Player.Abilities = abilities;
		}
	}

	public Vector2 GetRespawnPointCoordinates()
	{
		int num = FindGeneratedModuleIndex(playState.Session.MaxPlayerY);
		if (num > 0 && num + 1 < generatedModules.Count)
		{
			num++;
		}
		if (num >= 0 && num < generatedModules.Count)
		{
			GeneratedModule generatedModule = generatedModules[num];
			return new Vector2(generatedModule.WorldEnterX, generatedModule.WorldEnterY);
		}
		return Vector2.Zero;
	}

	public int FindGeneratedModuleIndex(int worldY)
	{
		for (int num = generatedModules.Count - 1; num >= 0; num--)
		{
			if (worldY <= generatedModules[num].WorldEnterY || num == 0)
			{
				return num;
			}
		}
		return -1;
	}

	private GeneratedModule GeneratedModuleAt(int worldY)
	{
		int num = FindGeneratedModuleIndex(worldY);
		if (num != -1)
		{
			return generatedModules[num];
		}
		return null;
	}

	public LevelModule ModuleAt(int worldY)
	{
		return GeneratedModuleAt(worldY)?.Module;
	}

	public Vector2 ClosestSafePoint(Vector2 coordinates)
	{
		GeneratedModule generatedModule = GeneratedModuleAt((int)coordinates.Y);
		return new Vector2(generatedModule.WorldEnterX, generatedModule.WorldEnterY);
	}

	public Vector2 NextSafePoint(Vector2 coordinates)
	{
		int num = FindGeneratedModuleIndex((int)coordinates.Y);
		if (num + 1 < generatedModules.Count)
		{
			num++;
		}
		GeneratedModule generatedModule = ((num == -1 || num >= generatedModules.Count) ? null : generatedModules[num]);
		if (generatedModule != null)
		{
			return new Vector2(generatedModule.WorldEnterX, generatedModule.WorldEnterY);
		}
		return Vector2.Zero;
	}

	public int AvgCoinValue()
	{
		if (distance >= 100)
		{
			if (distance >= 200)
			{
				if (distance >= 300)
				{
					return 4;
				}
				return 3;
			}
			return 2;
		}
		return 1;
	}

	public Entity MakeChest(int x, int y, TileDesc desc)
	{
		int num = AvgCoinValue();
		int num2 = 0;
		bool flag = base.core.Holiday == Holiday.Xmas && (SciHelper.ChanceRoll(0.4f) || desc.ParentModule.Name.Contains("xmas"));
		Entity result;
		if (SciHelper.ChanceRoll(0.1f))
		{
			num2 = 10 * num;
			result = new ChestEntity(x, y, new ChestContents(ItemType.GoldCoin, num2), (!flag) ? ChestEntity.ChestTier.Gold : ChestEntity.ChestTier.Present);
		}
		else
		{
			num2 = 5 * num;
			result = new ChestEntity(x, y, new ChestContents(ItemType.GoldCoin, num2), flag ? ChestEntity.ChestTier.Present : ChestEntity.ChestTier.Wood);
		}
		if (base.core.ProfileData.Character == Character.Bragg)
		{
			if (makeLockedChest)
			{
				makeLockedChest = false;
				result = new ChestEntity(x, y, new ChestContents(ItemType.GoldCoin, (((skullChestsLevel == 1) ? 20 : ((skullChestsLevel == 2) ? 25 : 30)) + Component._rnd(-3, 3)) * num)).Lock(nextSkullLock + ((skullChestsLevel > 1) ? Component._rnd(0, 5 * (skullChestsLevel - 1)) : 0));
			}
			else if (makeParrotCage)
			{
				makeParrotCage = false;
				result = new ParrotCageEntity(x, y);
			}
		}
		return result;
	}

	public Entity MakeLoot(int x, int y, int value = 0)
	{
		ItemType type = ItemType.GoldCoin;
		int num = distance;
		if (value == 0)
		{
			float f = (float)(num - 70) / 60f;
			f = Component._m(f, 1f);
			f = Component._M(f, 0f);
			f *= f * f;
			if (SciHelper.ChanceRoll(f))
			{
				type = ItemType.GreenCoin;
			}
			f = (float)(num - 90) / 150f;
			f = Component._m(f, 1f);
			f = Component._M(f, 0f);
			f *= f * f * f * f;
			if (SciHelper.ChanceRoll(f))
			{
				type = ItemType.BlueCoin;
			}
			f = (float)(num - 90) / 280f;
			f = Component._m(f, 1f);
			f = Component._M(f, 0f);
			f *= f * f * f * f;
			if (SciHelper.ChanceRoll(f))
			{
				type = ItemType.RedCoin;
			}
		}
		else
		{
			type = ItemEntity.ValueToType(value);
		}
		if (base.core.Holiday == Holiday.Xmas && SciHelper.ChanceRoll(0.1f))
		{
			type = (SciHelper.ChanceRoll() ? ItemType.Ginger : ItemType.CandyCane);
		}
		if (base.core.Holiday == Holiday.ChunJie && SciHelper.ChanceRoll(0.1f))
		{
			type = ItemType.Tangerine;
		}
		if (base.core.ProfileData.Character == Character.Bragg && SciHelper.ChanceRoll(0.25f))
		{
			type = ItemType.SkullKey;
			skullKeysSpawned++;
		}
		return new ItemEntity(x, y, type);
	}

	public string GetModuleIdWithPlayer()
	{
		if (playState.Player.Tile == null)
		{
			return string.Empty;
		}
		int num = FindGeneratedModuleIndex(playState.Player.Tile.Y);
		if (num == -1)
		{
			return string.Empty;
		}
		return generatedModules[num].Id;
	}
}
