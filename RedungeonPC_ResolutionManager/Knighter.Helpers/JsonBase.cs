using System.Collections.Generic;

namespace Knighter.Helpers;

public class JsonBase
{
	public virtual int ToInt()
	{
		return 0;
	}

	public virtual List<JsonObject> ToListOfObjects()
	{
		return new List<JsonObject>();
	}
}
