using Knighter.Entities;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public class Light : Component
{
	public bool Active = true;

	public bool InWorld = true;

	public Color Color = Color.White;

	public float Intencity = 1f;

	public float TargetIntencity = 1f;

	public float Radius = 5f;

	public float TargetRadius = 5f;

	public Vector2 Position;

	public Vector2 Offset = Vector2.Zero;

	public float FollowRate = 1f;

	public float ChangeRate = 0.2f;

	private Entity target;

	private bool hasTarget;

	private bool dying;

	public bool Dead;

	public float ActualRadius => Radius * 16f;

	public Vector2 ActualPosition => Position + Offset;

	public Color ActualColor => Color * Intencity;

	public void Follow(Entity target)
	{
		this.target = target;
		hasTarget = target != null;
	}

	public void Die()
	{
		dying = true;
		TargetIntencity = 0f;
	}

	public override void Update()
	{
		if (Dead)
		{
			return;
		}
		if (hasTarget)
		{
			if (target != null)
			{
				Position += (target.WorldCenter - Position) * FollowRate;
			}
			if ((target == null || target.Unloaded) && !dying)
			{
				Die();
			}
		}
		if (Active || dying)
		{
			Intencity += (TargetIntencity - Intencity) * ChangeRate;
			Radius += (TargetRadius - Radius) * ChangeRate;
		}
		if (dying && Intencity.IsZero())
		{
			Dead = true;
		}
		base.Update();
	}
}
