namespace Knighter.Helpers;

public class JsonBoolean : JsonBase
{
	public readonly bool Value;

	public JsonBoolean(bool value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override int ToInt()
	{
		if (!Value)
		{
			return 0;
		}
		return 1;
	}
}
