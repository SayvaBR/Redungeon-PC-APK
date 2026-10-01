using System;
using Android.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter;
public enum WindowMode { Windowed, Fullscreen, Borderless }
public sealed class MobileGame : Game
{
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch batch;
    private int lastLoggedWidth;
    private int lastLoggedHeight;
    private int pendingWidth;
    private int pendingHeight;
    private int pendingStableFrames;
    private bool waitingForStableSurface;
    public MobileGame()
    {
        Settings.IsTouchDevice = true;
        // The PC reference layout is intentionally compact. On a phone the
        // same pixel-art units would occupy too little of the display.
        Settings.GuiScale = 0.95f;
        graphics = new GraphicsDeviceManager(this) { IsFullScreen = true,
            SupportedOrientations = DisplayOrientation.Portrait |
                DisplayOrientation.LandscapeLeft |
                DisplayOrientation.LandscapeRight };
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60);
    }
    protected override void LoadContent()
    {
        batch = new SpriteBatch(GraphicsDevice);
        var p = GraphicsDevice.PresentationParameters;
        lastLoggedWidth = p.BackBufferWidth;
        lastLoggedHeight = p.BackBufferHeight;
        pendingWidth = p.BackBufferWidth;
        pendingHeight = p.BackBufferHeight;
        Core.Initialize(this, GraphicsDevice, batch, Content, p.BackBufferWidth, p.BackBufferHeight);
        Core.Instance.Load();
    }
    protected override void Update(GameTime time)
    {
        if (IsActive && Core.Instance != null)
        {
            var p = GraphicsDevice.PresentationParameters;
            if (p.BackBufferWidth != lastLoggedWidth || p.BackBufferHeight != lastLoggedHeight)
            {
                Log.Info("Redungeon", $"buffer={p.BackBufferWidth}x{p.BackBufferHeight} client={Window.ClientBounds.Width}x{Window.ClientBounds.Height}");
                lastLoggedWidth = p.BackBufferWidth;
                lastLoggedHeight = p.BackBufferHeight;
            }
            var resolution = Core.Instance.ResolutionManager;
            if (p.BackBufferWidth <= 0 || p.BackBufferHeight <= 0)
            {
                waitingForStableSurface = true;
                pendingStableFrames = 0;
                return;
            }
            // Track every observed size, including a return to the original
            // orientation before a rotation finishes. Otherwise the previous
            // debounce could remain waiting forever for an abandoned size.
            if (p.BackBufferWidth != pendingWidth || p.BackBufferHeight != pendingHeight)
            {
                pendingWidth = p.BackBufferWidth;
                pendingHeight = p.BackBufferHeight;
                pendingStableFrames = 0;
                waitingForStableSurface = true;
            }
            else if (waitingForStableSurface)
                pendingStableFrames++;

            if (waitingForStableSurface)
            {
                if (pendingStableFrames < 8)
                    return;

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Viewport = new Viewport(0, 0, pendingWidth, pendingHeight);
                if (pendingWidth != resolution.BackbufferWidth || pendingHeight != resolution.BackbufferHeight)
                    resolution.Recalculate(pendingWidth, pendingHeight);
                waitingForStableSurface = false;
                Core.Instance.Input.Reset();
                MouseTouchAdapter.Reset();
                Log.Info("Redungeon", $"surface ready={pendingWidth}x{pendingHeight} logical={resolution.LogicalWidth}x{resolution.LogicalHeight}");
            }
            TouchPanel.DisplayWidth = p.BackBufferWidth;
            TouchPanel.DisplayHeight = p.BackBufferHeight;
            Core.Instance.GameTime = time;
            Core.Instance.Update();
        }
        base.Update(time);
    }
    protected override void Draw(GameTime time)
    {
        var p = GraphicsDevice.PresentationParameters;
        var resolution = Core.Instance?.ResolutionManager;
        // Android can resize between Update and Draw as well.
        if (waitingForStableSurface || resolution == null ||
            p.BackBufferWidth != resolution.BackbufferWidth || p.BackBufferHeight != resolution.BackbufferHeight)
        {
            GraphicsDevice.Clear(Color.Black);
            return;
        }
        GraphicsDevice.Viewport = new Viewport(0, 0, resolution.BackbufferWidth, resolution.BackbufferHeight);
        GraphicsDevice.Clear(Color.Black);
        Core.Instance?.Draw();
        base.Draw(time);
    }
    protected override void UnloadContent() { Core.Instance?.Unload(); batch?.Dispose(); }
    public void OnEnteringBackground() { Core.Instance?.OnEnteringBackground(); Core.Instance?.Input.Reset(); }
    public void SetWindowMode(WindowMode mode) { }
    public void SetWindowedResolution(int width, int height) { }
    public void ApplyVideoSettings(bool vsync, int fpsLimit) { }
}
