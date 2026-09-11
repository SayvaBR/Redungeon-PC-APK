namespace Knighter.Helpers;

public class Pair<T1, T2>
{
	public T1 A { get; set; }

	public T2 B { get; set; }

	public Pair(T1 a, T2 b)
	{
		A = a;
		B = b;
	}
}
