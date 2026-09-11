using System;
using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

/// <summary>Lykos keeps the original Wolf save id. All poses and UI assets are his own.</summary>
public sealed class WolfChar : PlayerEntity
{
    private readonly Animation humanAnimation, wolfAnimation;
    private Light moonLight;
    private int morphTicks;
    private bool transformingIntoWolf;
    private bool ownsSlowMotion, previousSlowMotion, previousAffectsPlayer;
    private float previousSlowFactor;
    private string deathForm;
    public LykosPower Power { get; }
    public bool WolfForm => Power.Active;

    [Preserve]
    public WolfChar(int x, int y) : base(x, y)
    {
        Power = new LykosPower(core.ProfileData.CurrentCharLevel);
        humanAnimation = CreateAnimation("human");
        wolfAnimation = CreateAnimation("wolf");
        animation = humanAnimation;
        normalAnimSpeed = .095f;
        AnimateUTurns = false;
        PosShift = new Vector2(-8, -16);
        ShadowShift = new Vector2(0, 3);
        ZappedSprite = SpriteName.lykos_zapped;
    }

    private static Animation CreateAnimation(string form)
    {
        var result = new Animation(.1f);
        foreach (string dir in new[] { "s", "n", "e", "w" })
            result.Add(dir, "lykos_" + form + "_" + dir + "_", "1234");
        result.Add("spin", new[] {
            Core.Instance.SpriteManager.GetSprite("lykos_" + form + "_n_1"),
            Core.Instance.SpriteManager.GetSprite("lykos_" + form + "_e_1"),
            Core.Instance.SpriteManager.GetSprite("lykos_" + form + "_s_1"),
            Core.Instance.SpriteManager.GetSprite("lykos_" + form + "_w_1") });
        result.Play("s");
        return result;
    }

    public override void Load()
    {
        base.Load();
        moonLight = playState.LightManager.AddLight(new Color(160, 191, 255), 4f, 0f, this);
        moonLight.FollowRate = 1f;
        moonLight.ChangeRate = .12f;
    }

    public override void Jump(Vector2 direction)
    {
        if (!Dead && !Falling && !Paralized() && !TrappedInWeb && !BSlide.Sliding)
        {
            Vector2 effective = SpellEffects[SpellType.Confusion].Active ? -direction : direction;
            Vector2 target = WorldCenter + effective * 16;
            foreach (var entity in playState.EntityManager.FindEntities(e => e is FollowerEntity && !e.IsBroken && Vector2.DistanceSquared(e.WorldCenter, target) < 144))
                EatSkeleton((FollowerEntity)entity);
            foreach (var entity in playState.EntityManager.FindEntities(e => e is SerpentEntity serpent && !serpent.IsChineseDragon && !serpent.Head().IsBroken && Vector2.DistanceSquared(e.WorldCenter, target) < 144))
                EatSerpent((SerpentEntity)entity);
        }
        base.Jump(direction);
    }

    public override void InteractWith(Entity other)
    {
        if (other is FollowerEntity skeleton) EatSkeleton(skeleton);
        else if (other is SerpentEntity serpent) EatSerpent(serpent);
        base.InteractWith(other);
    }

    private void EatSkeleton(FollowerEntity skeleton)
    {
        if (Dead || skeleton == null || skeleton.IsBroken) return;
        Vector2 position = skeleton.WorldCenterCoordinates;
        skeleton.Break(this);
        RegisterBone(position);
    }

    private void EatSerpent(SerpentEntity segment)
    {
        SerpentEntity head = segment?.Head();
        if (Dead || head == null || head.IsChineseDragon || head.IsBroken || head.BreakT >= 0) return;
        Vector2 position = head.WorldCenterCoordinates;
        head.Break(this);
        RegisterBone(position);
    }

    private void RegisterBone(Vector2 position)
    {
        bool wasReady = Power.Reinforced;
        Power.EatBone();
        _inc(Stat.LykosBonesEaten);
        SendMessage(new SpawnEntityMessage(new EffectEntity(position.Shift(0, -.35f), "hit_claws_", "123").SetLayer("default", Z + 3, false), null));
        SendMessage(new SpawnEntityMessage(new FloatingTextEntity(position, __(SId.LYKOS_bone), new Color(255, 215, 158)), null));
        Rumble.Pulse("lykos-bite", .15f, .25f, 5);
        if (!wasReady && Power.Reinforced)
            playState.Hud.ShowAlert(__(SId.SKILL_REINFORCEMENT_name), __(SId.LYKOS_boost_ready), Color.Gold, 90, SpriteName.lykos_reinforce);
    }

    public override bool TryResist(InjuryType injuryType, Entity offender = null)
    {
        if (injuryType == InjuryType.Follower && offender is FollowerEntity follower) { EatSkeleton(follower); return follower.IsBroken; }
        if (injuryType == InjuryType.Serpent && offender is SerpentEntity serpent && !serpent.IsChineseDragon) { EatSerpent(serpent); return serpent.Head().IsBroken; }
        // Only physical traps are neutralized. Enemies, spells and darkness remain dangerous.
        return WolfForm && injuryType is InjuryType.Spikes or InjuryType.Saw or InjuryType.Crushed or InjuryType.Bolt or InjuryType.Axe or InjuryType.Flame or InjuryType.Zap;
    }

    public override bool TryResistSpell(SpellType spellType, Entity offender = null) => false;
    protected override bool TryResistFall() => false;
    public override bool WebCapture(WebEntity web) => base.WebCapture(web);

    public override void TryTriggerAbility()
    {
        if (Dead || Falling || Burning || TrappedInWeb || TeleportPending || !Power.TryActivate()) return;
        transformingIntoWolf = true;
        morphTicks = 24;
        DeactivateSpellEffects();
        moonLight.TargetIntencity = .8f;
        moonLight.TargetRadius = 5f;
        if (Power.Level >= 3)
        {
            previousSlowMotion = playState.SloMo;
            previousAffectsPlayer = playState.SloMoAffectsPlayer;
            previousSlowFactor = playState.SloMoFactor;
            playState.SloMo = true;
            playState.SloMoAffectsPlayer = false;
            playState.SloMoFactor = .45f;
            ownsSlowMotion = true;
        }
        Abilities.SkillCharge[Skill.FullMoon] = 0;
        SendMessage(new PlaySoundMessage(SoundName.lykos_howl, .82f));
        playState.Camera.Shake("lykos-moon", 1.2f, 10);
        Rumble.Pulse("lykos-transform", .3f, .55f, 12);
    }

    private void ReleaseMoonEffects()
    {
        if (ownsSlowMotion)
        {
            playState.SloMo = previousSlowMotion;
            playState.SloMoAffectsPlayer = previousAffectsPlayer;
            playState.SloMoFactor = previousSlowFactor;
            ownsSlowMotion = false;
        }
        if (moonLight != null) { moonLight.TargetIntencity = 0; moonLight.TargetRadius = 4; }
    }

    protected override void UpdateAbilities()
    {
        if (Power.Tick())
        {
            ReleaseMoonEffects();
            animation = humanAnimation;
            animation.Play(FacingDirection.DirectionId());
            ZappedSprite = SpriteName.lykos_zapped;
            morphTicks = 12;
            SendMessage(new PlaySoundMessage(SoundName.kazhan_turn_back, .65f));
            if (Tile != null) OnEnterTile(Tile);
        }
        Abilities.SkillCharge[Skill.FullMoon] = Power.Charge;
    }

    public override void Update()
    {
        if (morphTicks > 0)
        {
            morphTicks--;
            if (transformingIntoWolf && morphTicks == 11)
            {
                animation = wolfAnimation;
                animation.Play(FacingDirection.DirectionId());
                ZappedSprite = SpriteName.lykos_wolf_zapped;
                playState.Camera.Shake("lykos-morph", 2f, 8);
            }
            if (morphTicks == 0) transformingIntoWolf = false;
        }
        normalAnimSpeed = WolfForm ? .11f : .095f;
        base.Update();
    }

    public override bool Paralized() => morphTicks > 0 || base.Paralized();

    public override void Draw()
    {
        if (!Dead && !Falling)
            DrawShadow(WorldCenter.Shift(0, 5) + dAnim, WolfForm ? .9f : .78f, .65f, .9f);
        base.Draw();
        if (Dead || Falling) return;
        if (morphTicks > 0)
        {
            float progress = 1f - morphTicks / 24f;
            float arc = MathF.Sin(progress * MathF.PI);
            Vector2 moon = WorldCenter.Shift(0, -17 - arc * 8) + dAnim;
            core.Renderer["bg", Z + 1, false].DrawSpriteW(_(SpriteName.glow_big), moon,
                new Color(174, 202, 255) * (.25f + arc * .45f), new Vector2(.65f + arc * .35f), 0, SpriteFlip.None, SpriteOrigin.Center);
            core.Renderer["bg", Z + 2, false].DrawSpriteW(_("lykos_full_moon"), moon,
                new Color(242, 231, 188) * (.55f + arc * .4f), new Vector2(.72f + arc * .18f), 0, SpriteFlip.None, SpriteOrigin.Center);
            core.Renderer["bg", Z + 3, false].DrawSpriteW(_(SpriteName.lykos_moon), moon.Shift(-8, -7),
                new Color(48, 38, 59) * arc, new Vector2(.9f), 0, SpriteFlip.None, SpriteOrigin.TopLeft);
            float p = morphTicks / 24f;
            core.Renderer[Z + 2].DrawSpriteW(_(SpriteName.glow_big), WorldCenter.Shift(0, -9) + dAnim,
                new Color(150, 194, 255) * (p * .55f), new Vector2(.3f + (1-p) * .25f), 0, SpriteFlip.None, SpriteOrigin.Center);
        }
        if (WolfForm)
        {
            var bar = new RectangleF(WorldCenter.X + dAnim.X - 10, WorldCenter.Y + dAnim.Y + 7, 20, 3);
            core.Renderer["fg", -10, false].DrawRectangleW(bar, new Color(27, 22, 34));
            bar.Width = 18f * Power.RemainingTicks / Power.DurationTicks;
            bar.Height = 1; bar.X++; bar.Y++;
            core.Renderer["fg", -9, false].DrawRectangleW(bar, new Color(255, 197, 102));
        }
        if (Power.Level >= 4)
        {
            Vector2 counter = new Vector2(8, topSafeArea + 31);
            core.Renderer["fg", 110, false].DrawSpriteS(_(SpriteName.lykos_bite), counter, Color.White, new Vector2(.55f));
            for (int i = 0; i < 5; i++)
                core.Renderer["fg", 111, false].DrawRectangleS(new RectangleF(counter.X + 16 + i * 4, counter.Y + 5, 3, 3),
                    Power.Reinforced || i < Power.Bones ? Color.Gold : new Color(63, 49, 57));
        }
    }

    protected override void OnDying(InjuryType causeOfDeath)
    {
        deathForm = WolfForm ? "wolf" : "human";
        ReleaseMoonEffects();
    }

    public override bool SpawnFragments(bool bolt = false)
    {
        string form = deathForm ?? (WolfForm ? "wolf" : "human");
        SendMessage(new SpawnEntityMessage(new EffectEntity(WorldCenterCoordinates.Shift(0, -.5f),
            "lykos_" + form + "_" + FacingDirection.DirectionId() + "_death_", "111222222", mirrored: true).Speed(.16f).SetLayer("default", Z + 2, true), CurrentPlatform));
        SendMessage(new PlaySoundMessage(SoundName.follower_death, .65f));
        return true;
    }

    // Electrical/slime effects already own their death animation; don't duplicate a corpse.
    public override bool SpawnLeftovers(Vector2 pos, bool bolt = false) => true;
    public override SpriteName ShotSprite(int dir) => SpriteName.lykos_shot;

    public override void Unload()
    {
        ReleaseMoonEffects();
        moonLight?.Die();
        base.Unload();
    }
}
