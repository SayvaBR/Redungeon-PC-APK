using System;

namespace Knighter.Helpers;

public sealed class PreserveAttribute : Attribute
{
	public bool AllMembers;

	public bool Conditional;
}
