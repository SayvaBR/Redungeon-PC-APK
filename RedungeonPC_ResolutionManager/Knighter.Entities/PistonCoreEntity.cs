using Knighter.Messages;

namespace Knighter.Entities;

public class PistonCoreEntity : Entity
{
	public PistonEntity Piston;

	public PistonCoreEntity(int x, int y, PistonEntity piston)
		: base(x, y, 1f, 1f)
	{
		Piston = piston;
	}

	public override void Break(Entity offender)
	{
		IsBroken = true;
		Piston.Break(offender);
		base.Break(offender);
	}

	public override void Unload()
	{
		if (Piston != null)
		{
			SendMessage(new RemoveEntityMessage(Piston));
		}
		base.Unload();
	}
}
