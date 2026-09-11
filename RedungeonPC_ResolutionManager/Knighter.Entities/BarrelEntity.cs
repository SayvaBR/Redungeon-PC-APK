using System.Diagnostics;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

[DebuggerDisplay("Looted: {Looted}, contains: ???")]
public class BarrelEntity : Entity
{
	private Sprite sprite;

	public bool Looted;

	public BarrelEntity(int x, int y)
		: base(x, y, 1f, 1f)
	{
		sprite = base.core.SpriteManager.GetSprite(SpriteName.barrel);
		Looted = false;
	}

	public override void Draw()
	{
		base.core.Renderer[base.Z].DrawSpriteW(sprite, base.WorldPosition + new Vector2(0f, -5f));
		DrawShadow(base.WorldCenter.Shift(0f, 2.5f), 0.6f, 0.6f);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		return other is FragmentEntity;
	}

	public override void InteractWith(Entity other)
	{
		if (other is PlayerEntity { Dead: false } && !Looted)
		{
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCoordinates, SpriteName.barrel_lid, 50), null));
			SendMessage(new SpawnEntityMessage(new FragmentEntity(base.WorldCoordinates, SpriteName.barrel_ring, 50), null));
			sprite = base.core.SpriteManager.GetSprite(SpriteName.barrel_open);
			(other as PlayerEntity).CollectCoins(5, this, Color.White);
			base.core.ParticleManager.MakeItemToHudEmitter(base.WorldCenter, ItemType.GoldCoin).AttachTo(this).Emit(5);
			Looted = true;
		}
	}
}
