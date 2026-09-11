using System;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public static class FrostLayout
{
    // One scale for both axes: crystals retain their shape in portrait and landscape.
    public static Rectangle Corner(int width, int height, int textureWidth, int textureHeight, float progress, int x, int y)
    {
        float extent = .45f + MathHelper.SmoothStep(0, 1, MathHelper.Clamp(progress, 0, 1)) * .55f;
        float scale = Math.Min((float)width / textureWidth, (float)height / textureHeight) * extent;
        int w = Math.Max(1, (int)MathF.Round(textureWidth * .5f * scale));
        int h = Math.Max(1, (int)MathF.Round(textureHeight * .5f * scale));
        return new Rectangle(x == 0 ? 0 : width - w, y == 0 ? 0 : height - h, w, h);
    }
}
