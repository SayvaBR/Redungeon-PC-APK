using System;

namespace Knighter.Entities;

public class CannonballEntity : FireballEntity
{
	private int dx;

	private int dy;

	private int speed;

	private int maxDistance;

	private float v;

	private float passed;

	public CannonballEntity(Entity parent, float x, float y, int dx, int dy, int speed, int maxDistance, BallType type)
		: base(parent, x, y, type)
	{
		this.dx = dx;
		this.dy = dy;
		this.speed = speed;
		this.maxDistance = maxDistance;
		v = 1f / (float)speed;
	}

	public override void Update()
	{
		if (!IsBroken)
		{
			x += v * (float)dx;
			y += v * (float)dy;
			UpdateTiles();
		}
		passed += Math.Abs(v * (float)dx) + Math.Abs(v * (float)dy);
		if (passed >= (float)maxDistance)
		{
			Break(null);
		}
		if (base.Age < 30)
		{
			if (dx != 0 || dy < 0)
			{
				dDepth = -20;
			}
		}
		else
		{
			dDepth = 0;
		}
		if (base.Age >= 1000)
		{
			Break(null);
		}
		base.Update();
	}

	public override void Draw()
	{
		if (base.Age >= 10)
		{
			if (dx != 0)
			{
				offset.Y = -3f;
			}
			if (dy != 0)
			{
				offset.X = -1f;
			}
			base.Draw();
		}
	}
}
