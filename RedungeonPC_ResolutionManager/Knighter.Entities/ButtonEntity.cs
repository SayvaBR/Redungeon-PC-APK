using Knighter.Gameplay;
using Knighter.Graphics;
using Knighter.Messages;

namespace Knighter.Entities;

public class ButtonEntity : Entity
{
	private bool pressed;

	private readonly int id;

	private readonly bool invisible;

	public ButtonEntity(int x, int y, TileDesc desc)
		: base(x, y, 1f, 1f)
	{
		id = int.Parse(desc.Str("id"));
		invisible = desc.Str("invisible") == "1";
	}

	public override void Draw()
	{
		if (!invisible)
		{
			base.R[base.Z].DrawSpriteW(pressed ? _(SpriteName.button_down) : _(SpriteName.button_up), base.WorldPosition);
		}
		base.Draw();
	}

	public override void CollideWith(Entity other)
	{
		if (other is PlayerEntity)
		{
			pressed = true;
			Trigger();
		}
		base.CollideWith(other);
	}

	public override void UnCollideWith(Entity other)
	{
		if (other is PlayerEntity)
		{
			pressed = false;
		}
		base.UnCollideWith(other);
	}

	private void Trigger()
	{
		int moduleIndex = base.core.CurrentPlayState.LevelGenerator.FindGeneratedModuleIndex((int)base.core.CurrentPlayState.Player.WorldCoordinates.Y);
		SendMessage(new ButtonTriggerMessage(id, moduleIndex));
	}
}
