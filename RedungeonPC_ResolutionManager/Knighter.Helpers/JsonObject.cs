using System.Collections.Generic;

namespace Knighter.Helpers;

public class JsonObject : JsonBase
{
	public readonly Dictionary<string, JsonBase> Fields;

	public JsonBase this[string name] => Fields[name];

	public bool Contains(string name)
	{
		return Fields.ContainsKey(name);
	}

	public JsonObject()
	{
		Fields = new Dictionary<string, JsonBase>();
	}
}
