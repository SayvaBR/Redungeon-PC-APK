using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class GhostEntity : Entity
{
	private Animation anim;

	public GhostEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		anim = new Animation();
		anim.AddAndPlay("spin", new List<SpriteName>
		{
			SpriteName.ghost_girl_1,
			SpriteName.ghost_girl_2,
			SpriteName.ghost_girl_3,
			SpriteName.ghost_girl_4,
			SpriteName.ghost_girl_5,
			SpriteName.ghost_girl_6,
			SpriteName.ghost_girl_7,
			SpriteName.ghost_girl_8
		});
	}

	public override void Update()
	{
		anim.Update();
		base.Update();
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(anim.GetCurrentFrame(), 16f * (base.WorldCoordinates + new Vector2(0.5f)) + new Vector2(-11f, -14f).Shift(16f * Component._sin((float)base.worldTicks * 0.02f), 0f), Color.White);
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity playerEntity)
		{
			playerEntity.Hurt(InjuryType.General);
		}
		base.CollideWith(other);
	}
}
