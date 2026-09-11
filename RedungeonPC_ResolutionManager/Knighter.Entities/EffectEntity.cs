using Knighter.Graphics;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class EffectEntity : Entity
{
	private readonly Animation anim;

	private bool screenEffect;

	private bool mirrored;

	private string layer;

	private int depth;

	private bool lit;

	public EffectEntity(Vector2 coordinates, string animPrefix, string animSequence, bool screenEffect = false, bool mirrored = false)
		: base(coordinates.X, coordinates.Y, 0f, 0f)
	{
		anim = new Animation(screenEffect ? 0.3f : 0.2f, loop: false);
		anim.Add("effect", animPrefix, animSequence);
		anim.Play("effect");
		this.screenEffect = screenEffect;
		this.mirrored = mirrored;
		layer = "bg";
		depth = 0;
		lit = false;
	}

	public EffectEntity Speed(float speed)
	{
		anim.Speed = speed;
		return this;
	}

	public EffectEntity SetLayer(string layer, int depth, bool lit = true)
	{
		this.layer = layer;
		this.depth = depth;
		this.lit = lit;
		return this;
	}

	public override void Update()
	{
		anim.Update();
		if (anim.Paused)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
		base.Update();
	}

	public override void Draw()
	{
		if (screenEffect)
		{
			base.core.Renderer["fg"].DrawSpriteS(anim.GetCurrentFrame(), base.core.Renderer.ScreenCenter, null, null, 0f, (!mirrored) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
		}
		else
		{
			base.core.Renderer[layer, depth, lit].DrawSpriteW(anim.GetCurrentFrame(), base.WorldCenter, null, null, 0f, (!mirrored) ? SpriteFlip.Horizontal : SpriteFlip.None, SpriteOrigin.Center);
		}
		base.Draw();
	}
}
