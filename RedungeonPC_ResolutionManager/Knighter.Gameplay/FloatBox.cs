namespace Knighter.Gameplay;

public class FloatBox
{
	public float F;

	public FloatBox(float value)
	{
		F = value;
	}

	public static implicit operator float(FloatBox fb)
	{
		return fb.F;
	}
}
