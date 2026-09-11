using Knighter.Entities;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public sealed class Particle
{
	public ParticleEmitter Parent;

	public bool InWorld;

	public Vector2 Position;

	public Vector2 Velocity;

	public Vector2 Offset;

	public int Age;

	public bool Dead;

	public Vector4 Aux;

	public PlatformEntity Platform;
}
