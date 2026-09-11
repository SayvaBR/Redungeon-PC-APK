using Knighter.Graphics;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.States;

/// <summary>The original helmet splash, held for two seconds before Nitrome.</summary>
public sealed class SplashState : State
{
    protected override bool ShowContextPromptLegend => false;
    public SplashState() { ShowCoins = false; TransDuration = 0; }

    public override void Update()
    {
        base.Update();
        if (TicksInState == 120)
            SendMessage(new CoreEventMessage(CoreEvent.ShowNitromeLogo));
    }

    public override void Draw()
    {
        R["bg"].FillScreen(Color.Black);
        var helmet = new Sprite { TextureName = "splash-knight", X = 85, Y = 160, Width = 150, Height = 134, SrcWidth = 150, SrcHeight = 134 };
        float scale = System.Math.Min(R.ScreenWidth, R.ScreenHeight) * .46f / helmet.Width;
        R["fg"].DrawSpriteS(helmet, R.ScreenCenter, Color.White, new Vector2(scale), origin: SpriteOrigin.Center);
    }
}
