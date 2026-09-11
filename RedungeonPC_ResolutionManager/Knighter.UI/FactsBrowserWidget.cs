using System;
using Knighter.Helpers;
using Knighter.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Knighter.UI;

public sealed class FactsBrowserWidget : Component, IMenuWidget
{
    private readonly FactsState facts;
    private int? wheel;
    private int? dragId;
    private float dragY;
    public RectangleF Bounds { get; set; }
    public bool IsFocusable => true;
    public bool ConsumesHorizontalInput => false;
    public FactsBrowserWidget(FactsState.FactsPage page) { facts = new FactsState(page); }
    public bool Scroll(int direction) => facts.ScrollEmbedded(-direction * 28);
    public void Update(float dt)
    {
        var mouse = core.Input.Current.Mouse;
        var position = core.ResolutionManager.WindowToLogical(new Vector2(mouse.X, mouse.Y));
        if (wheel.HasValue && Bounds.Contains(position)) facts.ScrollEmbedded((mouse.ScrollWheelValue - wheel.Value) * 0.35f);
        wheel = mouse.ScrollWheelValue;
        foreach (var touch in core.TouchState)
        {
            if (touch.State == TouchLocationState.Pressed && Bounds.Contains(touch.Position)) { dragId = touch.Id; dragY = touch.Position.Y; }
            else if (dragId == touch.Id)
            {
                facts.ScrollEmbedded(touch.Position.Y - dragY);
                dragY = touch.Position.Y;
                if (touch.State == TouchLocationState.Released) dragId = null;
            }
        }
    }
    public void Draw(bool focused, int depth) => facts.DrawEmbedded(Bounds, depth);
    public void Activate() { }
    public void Adjust(int direction) { }
    public void HandleClick(Vector2 p) { }
}
