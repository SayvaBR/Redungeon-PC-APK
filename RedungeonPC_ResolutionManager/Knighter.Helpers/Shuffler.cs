using System;

namespace Knighter.Helpers;

public class Shuffler
{
	public int N { get; private set; }

	public int K { get; private set; }

	public int I { get; private set; }

	public Shuffler(int n)
	{
		Reset(n);
	}

	public void Reset(int n)
	{
		N = n;
		do
		{
			K = SciHelper.GetRandom(0, 100499);
		}
		while (SciHelper.GCD(N, K) != 1);
		I = K;
	}

	public int Next()
	{
		I = (I + K) % N;
		return I;
	}

	public bool Empty()
	{
		return I == K % N;
	}
}
