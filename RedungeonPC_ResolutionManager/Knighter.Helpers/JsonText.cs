using System;

namespace Knighter.Helpers;

public class JsonText : JsonBase
{
	public readonly string Value;

	public JsonText(string value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value;
	}

	public override int ToInt()
	{
		return Convert.ToInt32(Value);
	}
}
