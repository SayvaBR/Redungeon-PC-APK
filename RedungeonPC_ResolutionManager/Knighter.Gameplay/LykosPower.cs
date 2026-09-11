using System;

namespace Knighter.Gameplay;

/// <summary>60 Hz player clock: duration and cooldown do not slow with the dungeon.</summary>
public sealed class LykosPower
{
    public int Level { get; }
    public int RemainingTicks { get; private set; }
    public int CooldownTicks { get; private set; }
    public int DurationTicks { get; private set; }
    public int Bones { get; private set; }
    public bool Reinforced { get; private set; }
    public bool Active => RemainingTicks > 0;
    public int RechargeDuration => (Level >= 3 ? 15 : 20) * 60;
    public bool Ready => Level >= 2 && !Active && CooldownTicks == 0;
    public float Charge => Active ? 0f : 1f - (float)CooldownTicks / RechargeDuration;

    public LykosPower(int level) => Level = Math.Clamp(level, 1, 4);

    public bool TryActivate()
    {
        if (!Ready) return false;
        DurationTicks = (Level >= 3 ? 5 : 3) * 60 + (Reinforced ? 120 : 0);
        RemainingTicks = DurationTicks;
        Reinforced = false;
        return true;
    }

    public void EatBone()
    {
        if (Level < 4 || Reinforced) return;
        if (++Bones == 5) { Bones = 0; Reinforced = true; }
    }

    // Returns true exactly on expiry; recharge starts after the transformation.
    public bool Tick()
    {
        if (Active)
        {
            if (--RemainingTicks == 0) { CooldownTicks = RechargeDuration; return true; }
        }
        else if (CooldownTicks > 0) CooldownTicks--;
        return false;
    }
}
