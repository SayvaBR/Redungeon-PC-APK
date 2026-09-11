using System.Collections.Generic;

namespace Knighter.Helpers;

public class JsonArray : JsonBase
{
	public readonly List<JsonObject> Objects;

	public JsonObject this[int n] => Objects[n];

	public JsonArray()
	{
		Objects = new List<JsonObject>();
	}

	public override List<JsonObject> ToListOfObjects()
	{
		return Objects;
	}
}
