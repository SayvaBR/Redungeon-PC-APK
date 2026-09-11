#if DEBUG
using System;
using System.Collections.Generic;
using System.IO;
using Knighter.Entities;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Knighter.QA;
using Knighter.States;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter;

public sealed partial class Core
{
    private string lykosQa;
    private ShopState lykosShop;
    private readonly List<FollowerEntity> lykosMeals = new();
    private SerpentEntity lykosSerpent;
    private FollowerEntity lykosMimic;
    private bool lykosPurchased, lykosStarted, lykosActivated, lykosVisualWolf, lykosExpired, lykosDied;
    private bool lykosIceMimicPassed, lykosWebPassed, allCharactersMaxed;
    private int lykosBeforeCoins;

    private static void LykosCheck(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Lykos QA: " + message);
    }

    private void OpenLykosQa(string fixture)
    {
        lykosQa = fixture;
        allCharactersMaxed = true;
        foreach (Character character in Enum.GetValues(typeof(Character)))
            allCharactersMaxed &= ProfileData.Characters[character].Unlocked &&
                ProfileData.Characters[character].Level == CharDescription.Get[character].Levels.Count;
        ProfileData.LanguageSelectorPending = ProfileData.ControlsSelectorPending = false;
        ProfileData.LearnedSwipes = true;
        OptionsData.DynamicLighting = OptionsData.LightBloom = true;
        ProfileData.Character = Character.Wolf;
        ProfileData.Characters[Character.Wolf].Level = fixture == "lykos-level2" ? 2 : 4;
        ProfileData.Characters[Character.Wolf].Unlocked = true;
        ProfileData.Coins = 100;
        ProfileData.SetStat(Stat.LykosBonesEaten, 0);
        if (fixture is "lykos-shop" or "lykos-purchase" or "lykos-selector")
        {
            if (fixture != "lykos-selector") {
            ProfileData.Character = Character.Knight;
            ProfileData.Characters[Character.Wolf].Unlocked = false;
            ProfileData.Characters[Character.Wolf].Level = 1;
            }
            ResetInternal();
            lykosShop = new ShopState(Character.Wolf);
            PushState(lykosShop);
        }
        else
        {
            ResetInternal(true);
            PrepareLykosArena();
        }
        var desc = CharDescription.Get[Character.Wolf];
        LykosCheck(desc.UnlockPrice == 100, "wrong unlock price");
        LykosCheck(LocaleManager.GetForCurrentLocale(desc.Name.ToString()) == "Lykos", "wrong name");
        foreach (string form in new[] { "human", "wolf" })
        foreach (string dir in new[] { "n", "s", "e", "w" })
        for (int frame = 1; frame <= 4; frame++)
            LykosCheck(SpriteManager.HasSprite($"lykos_{form}_{dir}_{frame}"), "missing walk sprite");
        foreach (SpriteName id in new[] { desc.Portrait, desc.Icon, desc.SkullSprite, SpriteName.lykos_zapped, SpriteName.lykos_wolf_zapped })
            LykosCheck(SpriteManager.GetSprite(id).TextureName == "lykos-atlas", "foreign UI/death sprite " + id);
    }

    private void PrepareLykosArena()
    {
        var play = CurrentPlayState;
        var player = play.Player;
        foreach (var e in play.EntityManager.FindEntities(e => e != player)) SendMessage(new RemoveEntityMessage(e));
        int x = (int)player.WorldCoordinates.X, y = (int)player.WorldCoordinates.Y;
        for (int dy = -9; dy <= 3; dy++) for (int dx = -4; dx <= 4; dx++)
            play.TileMap.AddTile(new DungeonTile(x + dx, y + dy, TileType.Floor));
        play.TerminatorTarget = player.WorldPosition.Y + 1000;
        play.TerminatorDontKeepUp = true;
        play.Camera.JumpTo(player.WorldCenter.Shift(0, -25));
        lykosStarted = player is WolfChar;
    }

    internal void TickLykosQa(int frame)
    {
        if (lykosQa == null) return;
        if (lykosQa == "lykos-purchase")
        {
            if (frame == 35) lykosShop.BuyForQa();
            if (frame == 70)
            {
                LykosCheck(ProfileData.Coins == 0 && ProfileData.Character == Character.Wolf && ProfileData.Characters[Character.Wolf].Unlocked, "purchase didn't deduct exactly 100/select");
                var disk = new Storage(QaSession.StoragePath);
                int coins = -1, character = -1; bool unlocked = false;
                disk.TryGetInt("coins", ref coins); disk.TryGetInt("character", ref character); disk.TryGetBool("character-Wolf-unlocked", ref unlocked);
                LykosCheck(coins == 0 && character == (int)Character.Wolf && unlocked, "save did not persist purchase");
                lykosPurchased = true;
            }
            if (frame == 100) GetCurrentState().OnBackButtonPressed();
            if (frame == 145) lykosShop.PlayForQa();
            if (frame == 190) { PrepareLykosArena(); LykosCheck(CurrentPlayState.Player is WolfChar, "shop started a different character"); }
        }
        if (CurrentPlayState?.Player is not WolfChar wolf) return;
        if (lykosQa == "lykos-serpent" && frame == 25)
        {
            lykosSerpent = new SerpentEntity(null, wolf.WorldCoordinates.X + 1, wolf.WorldCoordinates.Y, 0, false);
            SendMessage(new SpawnEntityMessage(lykosSerpent, null));
        }
        if (lykosQa == "lykos-serpent" && frame == 40)
        {
            wolf.Jump(new Vector2(1, 0));
            LykosCheck(lykosSerpent.IsBroken, "serpent skeleton was crossed without being eaten");
            LykosCheck(ProfileData.GetStat(Stat.LykosBonesEaten) == 1, "serpent bone was not counted exactly once");
        }
        if (lykosQa == "lykos-ice-mimic" && frame == 25)
        {
            var floor = wolf.Tile as DungeonTile;
            LykosCheck(floor != null, "player is not standing on a dungeon tile");
            // Feed the real slide behavior an ice tile without mutating the rendered arena tile.
            var ice = new DungeonTile(floor.X, floor.Y, TileType.Ice) { Map = floor.Map };
            wolf.LastMovementDir = Vector2.UnitX;
            lykosMimic = new FollowerEntity((int)wolf.WorldCoordinates.X + 1, (int)wolf.WorldCoordinates.Y, null, FollowerKind.Red);
            SendMessage(new SpawnEntityMessage(lykosMimic, null));
            wolf.BSlide.SlowLanding = false;
            wolf.BSlide.OnEnterTile(ice);
        }
        if (lykosQa == "lykos-ice-mimic" && frame == 45)
        {
            LykosCheck(lykosMimic?.IsBroken == true, "ice slide did not bite the mimic");
            LykosCheck(!wolf.BSlide.Sliding, "ice slide did not stop at the mimic");
            LykosCheck(!wolf.WorldCoordinates.IsEqualTo(lykosMimic.WorldCoordinates), "player overlapped the mimic tile");
            LykosCheck(ProfileData.GetStat(Stat.LykosBonesEaten) == 1, "mimic bone was not counted exactly once");
            lykosIceMimicPassed = true;
        }
        if (lykosQa == "lykos-web" && frame == 35)
        {
            var desc = new TileDesc { ElementJson = new JsonObject() };
            desc.ElementJson.Fields["difficulty"] = new JsonNumber(2);
            var web = new WebEntity((int)wolf.WorldCoordinates.X, (int)wolf.WorldCoordinates.Y, desc, 0);
            SendMessage(new SpawnEntityMessage(web, null));
            LykosCheck(wolf.WebCapture(web) && wolf.TrappedInWeb, "wolf form escaped the web");
            wolf.TryTriggerAbility();
            LykosCheck(!wolf.WolfForm, "ability activated while trapped in a web");
            lykosWebPassed = true;
        }
        if (lykosQa is "lykos-return" or "lykos-revive")
        {
            if (frame == 90) { wolf.Hurt(InjuryType.Spikes); lykosDied = wolf.Dead; }
            if (frame == 240)
            {
                LykosCheck(GetCurrentState() is ContinueState, "continue screen missing");
                if (lykosQa == "lykos-revive") ((ContinueState)GetCurrentState()).PayForQa();
                else GetCurrentState().OnBackButtonPressed();
            }
        }
        if ((lykosQa is "lykos-moon" or "lykos-expiry") && frame == 25)
        {
            for (int i = 0; i < 5; i++)
            {
                var skeleton = new FollowerEntity((int)wolf.WorldCoordinates.X + (i == 0 ? 1 : -3 + i), (int)wolf.WorldCoordinates.Y - (i == 0 ? 0 : 3), null, FollowerKind.Red);
                lykosMeals.Add(skeleton); SendMessage(new SpawnEntityMessage(skeleton, null));
            }
        }
        if ((lykosQa is "lykos-moon" or "lykos-expiry") && frame == 40)
        {
            wolf.Jump(new Vector2(1, 0)); // A real move into a dormant skeleton.
            LykosCheck(lykosMeals[0].IsBroken, "bite failed during movement");
            foreach (var skeleton in lykosMeals) wolf.Hurt(InjuryType.Follower, skeleton);
            LykosCheck(ProfileData.GetStat(Stat.LykosBonesEaten) == 5, "skeleton counted twice or not eaten");
            LykosCheck(wolf.Power.Reinforced, "five skeletons did not prepare reinforcement");
        }
        if (frame == 64) lykosBeforeCoins = ProfileData.Coins;
        if ((lykosQa is "lykos-moon" or "lykos-transform" or "lykos-expiry" or "lykos-level2" or "lykos-wolf-fall") && frame == 72)
        {
            LykosCheck(wolf.WolfForm, "ability input didn't activate moon");
            LykosCheck(ProfileData.Coins == lykosBeforeCoins, "moon spent coins");
            int expectedDuration = lykosQa == "lykos-level2" ? 180 :
                (lykosQa is "lykos-transform" or "lykos-wolf-fall" ? 300 : 420);
            LykosCheck(wolf.Power.DurationTicks == expectedDuration, "incorrect duration/boost");
            foreach (var injury in new[] { InjuryType.Spikes, InjuryType.Zap, InjuryType.Flame, InjuryType.Crushed, InjuryType.Bolt }) wolf.Hurt(injury);
            LykosCheck(!wolf.Dead, "moon did not protect against traps");
            LykosCheck(!wolf.TryResist(InjuryType.Bat) && !wolf.TryResist(InjuryType.Slime) && !wolf.TryResist(InjuryType.Follower), "moon incorrectly protects against enemies");
            wolf.ApplySpell(SpellType.Ice);
            LykosCheck(wolf.SpellEffects[SpellType.Ice].Active, "moon incorrectly resisted magic");
            wolf.SpellEffects[SpellType.Ice].Deactivate();
            LykosCheck(CurrentPlayState.SloMo == (lykosQa != "lykos-level2"), "slow motion level mismatch");
            lykosActivated = true;
        }
        if ((lykosQa is "lykos-moon" or "lykos-transform" or "lykos-expiry" or "lykos-level2" or "lykos-wolf-fall") && frame == 82)
        {
            LykosCheck(wolf.ZappedSprite == SpriteName.lykos_wolf_zapped, "visual form did not switch to wolf");
            lykosVisualWolf = true;
        }
        if (lykosQa == "lykos-wolf-fall" && frame == 90)
        {
            LykosCheck(wolf.WolfForm, "fall test was not in wolf form");
            wolf.Fall();
            lykosDied = wolf.Dead || wolf.Falling;
        }
        if (lykosQa == "lykos-expiry" && frame == 510)
        {
            LykosCheck(!wolf.WolfForm && !CurrentPlayState.SloMo && wolf.Power.CooldownTicks > 0, "expiry/cleanup failed");
            LykosCheck(wolf.GetCurrentSprite().TextureName == "lykos-atlas", "wrong sprite after expiry");
            lykosExpired = true;
        }
        if (lykosQa.StartsWith("lykos-death-") && frame == 90)
        {
            InjuryType injury = lykosQa.Substring("lykos-death-".Length) switch {
                "fall" => InjuryType.Fall, "zap" => InjuryType.Zap, "fire" => InjuryType.Flame,
                "crush" => InjuryType.Crushed, "bolt" => InjuryType.Bolt, _ => InjuryType.Spikes };
            if (injury == InjuryType.Fall) wolf.Fall(); else wolf.Hurt(injury);
            if (injury == InjuryType.Zap) SendMessage(new SpawnEntityMessage(new ZappedEffectEntity(wolf), null));
            lykosDied = wolf.Dead || wolf.Falling;
        }
    }

    private void VerifyLykosQa()
    {
        LykosCheck(lykosQa is "lykos-shop" or "lykos-selector" || lykosStarted, "did not reach gameplay");
        if (lykosQa == "lykos-return") LykosCheck(lykosDied && GetCurrentState() is MenuState, "death did not return to menu");
        if (lykosQa == "lykos-revive") LykosCheck(lykosDied && GetCurrentState() is PlayState && CurrentPlayState.Player is WolfChar live && !live.Dead, "revive did not restore Lykos");
        if (lykosQa == "lykos-purchase") LykosCheck(lykosPurchased, "purchase flow not verified");
        if (lykosQa is "lykos-moon" or "lykos-transform" or "lykos-level2" or "lykos-expiry" or "lykos-wolf-fall") LykosCheck(lykosActivated && lykosVisualWolf, "moon/visual wolf not verified");
        if (lykosQa == "lykos-expiry") LykosCheck(lykosExpired, "expiry not verified");
        if (lykosQa == "lykos-serpent") LykosCheck(lykosSerpent?.IsBroken == true, "serpent consumption not verified");
        if (lykosQa == "lykos-ice-mimic") LykosCheck(lykosIceMimicPassed, "ice/mimic collision not verified");
        if (lykosQa == "lykos-web") LykosCheck(lykosWebPassed, "web capture not verified");
        if (lykosQa == "lykos-all-unlocked") LykosCheck(allCharactersMaxed, "not every character loaded unlocked at maximum level");
        if (lykosQa == "lykos-wolf-fall") LykosCheck(lykosDied, "wolf form did not fall");
        if (lykosQa.StartsWith("lykos-death-")) LykosCheck(lykosDied, "death state did not trigger");
        File.WriteAllText(Environment.GetEnvironmentVariable("REDUNGEON_QA_VISUAL") + ".txt",
            $"PASS {lykosQa}\nState={GetCurrentState().GetType().Name}\nCoins={ProfileData.Coins}\nCharacter={ProfileData.Character}\nBones={ProfileData.GetStat(Stat.LykosBonesEaten)}\n");
    }
}
#endif
