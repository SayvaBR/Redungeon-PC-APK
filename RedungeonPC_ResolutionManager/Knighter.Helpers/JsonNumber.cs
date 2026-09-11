namespace Knighter.Helpers;

public class JsonNumber : JsonBase
{
	public int Value;

	public JsonNumber(int value)
	{
		Value = value;
	}

	public override string ToString()
	{
		return Value.ToString();
	}

	public override int ToInt()
	{
		return Value;
	}
}
