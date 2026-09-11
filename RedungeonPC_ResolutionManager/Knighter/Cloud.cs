using Microsoft.Xna.Framework;

namespace Knighter;

/// <summary>
/// Desktop replacement for Android's Cloud.cs (Google Play Games cloud
/// saves). No cloud sync on desktop - Storage.cs already persists locally,
/// so Sync() is a safe no-op. If the build reveals more members are needed
/// here (beyond Sync()), send me the exact error and I'll extend this.
/// </summary>
public class Cloud : Component
{
	public void Sync()
	{
	}
}
