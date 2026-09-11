#if DEBUG
using System;
using Knighter.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Knighter.QA;

internal static class UiFixtureInput
{
    internal static InputSnapshot Capture(int frame, InputSnapshot original)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDUNGEON_QA_VISUAL"))) return original;
        string name = Environment.GetEnvironmentVariable("REDUNGEON_QA_SCREEN") ?? "";
        Keys key = Keys.None;
        Buttons button = 0;
        if ((name == "lykos-moon" || name == "lykos-transform" || name == "lykos-expiry" || name == "lykos-level2" || name == "lykos-wolf-fall") && frame == 65) key = Keys.Space;
        if (name == "pause-resume" && frame == 55) button = Buttons.Start;
        if (name == "pause-exit" && frame == 55) button = Buttons.B;
        if (name == "pause-options") { if (frame == 55) button = Buttons.X; if (frame == 100) button = Buttons.B; if (frame == 145) button = Buttons.Start; }
        if (name.StartsWith("reset-flow"))
        {
            if (frame == 35 || frame == 65) key = Keys.Enter;
            if (name.EndsWith("confirm") && frame == 60) key = Keys.Right;
        }
        if (name == "frost-release" && (frame == 80 || frame == 90 || frame == 100 || frame == 110 || frame == 120)) key = Keys.Up;
        if (name == "statistics-scroll" && frame >= 35 && frame < 120) key = Keys.Down;
        if (name == "pause-ps") button = frame == 54 ? Buttons.DPadRight : 0;
        var pad = name.StartsWith("pause") ? new GamePadState(Vector2.Zero, Vector2.Zero, 0, 0, button) : default;
        return new InputSnapshot(key == Keys.None ? default : new KeyboardState(key), default, pad);
    }
}
#endif
