using System.Collections.Generic;
using Microsoft.Xna.Framework.Input.Touch;
namespace Knighter;
public static class MouseTouchAdapter
{
    public static TouchCollection GetState()
    {
        var touches = new List<TouchLocation>();
        foreach (var touch in TouchPanel.GetState())
            touches.Add(new TouchLocation(touch.Id, touch.State,
                Core.Instance.ResolutionManager.WindowToLogical(touch.Position)));
        return new TouchCollection(touches.ToArray());
    }
    public static void Reset() { }
}
