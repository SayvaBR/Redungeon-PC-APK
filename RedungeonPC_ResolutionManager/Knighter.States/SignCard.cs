using Knighter.Graphics;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public class SignCard
{
	public string Name;

	public Animation Animation;

	public bool Mirrored;

	public SignRotation Rotation;

	public bool Active;

	public Vector2 Position;

	public Vector2 TargetPosition;

	public int RotT;

	public int FadeT;

	public int FadeD = 15;

	public Sprite Sprite => Animation.GetCurrentFrame();
}
