using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Messages;
using Knighter.Tiles;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class BoxEntity : Entity, IPushableEntity
{
	private enum BS
	{
		H,
		V,
		RollH_X,
		RollH_Y,
		RollV_X,
		RollV_Y
	}

	private enum BE
	{
		PushX,
		PushY
	}

	private Vector2 moveDir;

	private bool moveWhileUpdate;

	private StateMachine<BS, BE> sm;

	private Sprite sprite;

	private static int RollingD = 5;

	private Vector2 drawShift;

	public BoxEntity(int x, int y, TileDesc desc)
		: base(x, y, 0.8f, 0.8f)
	{
		InitSlideBehavior();
		BSlide.slideDelay = 5;
		sm = new StateMachine<BS, BE>();
		sm.State(BS.H).IsInitial().On(BE.PushX, BS.RollH_X)
			.On(BE.PushY, BS.RollH_Y);
		sm.State(BS.V).On(BE.PushX, BS.RollV_X).On(BE.PushY, BS.RollV_Y);
		sm.State(BS.RollH_X).After(RollingD).AutoTransitionTo(BS.V);
		sm.State(BS.RollH_Y).After(RollingD).AutoTransitionTo(BS.H);
		sm.State(BS.RollV_X).After(RollingD).AutoTransitionTo(BS.H);
		sm.State(BS.RollV_Y).After(RollingD).AutoTransitionTo(BS.V);
		sm.Start();
	}

	private void HandleStateMachine()
	{
		sm.Update();
		switch (sm.CurrentState)
		{
		case BS.H:
			sprite = _(SpriteName.box_2);
			break;
		case BS.V:
			sprite = _(SpriteName.box_1);
			break;
		case BS.RollH_X:
			sprite = ((moveDir.X > 0f) ? _(SpriteName.box_roll_h2) : _(SpriteName.box_roll_h1));
			break;
		case BS.RollH_Y:
			sprite = _(SpriteName.box_roll_v2);
			break;
		case BS.RollV_X:
			sprite = ((moveDir.X > 0f) ? _(SpriteName.box_roll_h1) : _(SpriteName.box_roll_h2));
			break;
		case BS.RollV_Y:
			sprite = _(SpriteName.box_roll_v1);
			break;
		}
		BS currentState = sm.CurrentState;
		if ((uint)(currentState - 2) <= 3u)
		{
			drawShift = -moveDir * 0.5f * 16f + new Vector2(0f, -5f);
			if (sm.CurrentState == BS.RollH_X || sm.CurrentState == BS.RollV_X)
			{
				drawShift.Y -= 5f;
			}
		}
		else
		{
			drawShift = new Vector2(0f, -5f);
		}
	}

	public override void Update()
	{
		HandleStateMachine();
		if (moveWhileUpdate)
		{
			TryMoveToCoordinates(base.CurrentMap, base.Coordinates + moveDir);
			moveWhileUpdate = false;
			DidMoved();
		}
		base.Update();
	}

	public override void Draw()
	{
		base.R[base.Z].DrawSpriteW(sprite, base.WorldPosition + drawShift);
		base.Draw();
	}

	public override bool IsPassableFor(Entity other)
	{
		if (other is PlayerEntity playerEntity)
		{
			int dx = (int)playerEntity.LastMovementDir.X;
			int dy = (int)playerEntity.LastMovementDir.Y;
			return CanMove(dx, dy);
		}
		return false;
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity playerEntity)
		{
			int dx = (int)playerEntity.LastMovementDir.X;
			int dy = (int)playerEntity.LastMovementDir.Y;
			if (CanMove(dx, dy))
			{
				Move(dx, dy);
			}
		}
		base.InteractWith(other);
	}

	private bool CanMove(int dx, int dy)
	{
		return TryMoveToCoordinates(base.CurrentMap, base.Coordinates + new Vector2(dx, dy), 0, actuallyMove: false);
	}

	private void Move(int dx, int dy)
	{
		moveDir = new Vector2(dx, dy);
		moveWhileUpdate = true;
		if (dx != 0)
		{
			sm.Trigger(BE.PushX);
		}
		if (dy != 0)
		{
			sm.Trigger(BE.PushY);
		}
	}

	private void DidMoved()
	{
	}

	public void Crash(Entity offender)
	{
	}

	public override void OnEnterTile(Tile tile)
	{
		if (tile.Type == TileType.Pit && CurrentPlatform == null)
		{
			Fall();
		}
		base.OnEnterTile(tile);
	}

	private void Fall()
	{
		if (!base.Flying)
		{
			SendMessage(new RemoveEntityMessage(this));
		}
	}
}
