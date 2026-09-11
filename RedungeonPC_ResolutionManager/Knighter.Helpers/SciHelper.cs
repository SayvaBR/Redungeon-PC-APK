using System;
using Microsoft.Xna.Framework;

namespace Knighter.Helpers;

public static class SciHelper
{
	public static float Eps;

	public static float BigFloat;

	private static Random random;

	static SciHelper()
	{
		Eps = 0.001f;
		BigFloat = 100500f;
		random = new Random();
	}

	/// <summary>
	/// Fixa a sequência pseudoaleatória para gravações/replays determinísticos
	/// de QA. O jogo normal continua usando uma seed não determinística.
	/// </summary>
	public static void SetRandomSeed(int seed)
	{
		random = new Random(seed);
	}

	public static int GCD(int a, int b)
	{
		if (a < 0)
		{
			a *= -1;
		}
		if (b < 0)
		{
			b *= -1;
		}
		while (b > 0)
		{
			int num = b;
			b = a % b;
			a = num;
		}
		return a;
	}

	public static int LCM(int a, int b)
	{
		if (a < 0)
		{
			a *= -1;
		}
		if (b < 0)
		{
			b *= -1;
		}
		return a * (b / GCD(a, b));
	}

	public static int GetRandom(int from, int to, int? except = null)
	{
		if (from > to)
		{
			int num = from;
			from = to;
			to = num;
		}
		int num2;
		do
		{
			num2 = random.Next() % (to - from + 1) + from;
		}
		while (num2 == except);
		return num2;
	}

	public static int GetRandom()
	{
		return random.Next();
	}

	public static bool ChanceRoll(float chance = 0.5f)
	{
		return (float)GetRandom(1, 100) <= chance * 100f;
	}

	public static bool IsZero(Vector2 v)
	{
		return v.Length() < Eps;
	}

	public static float GetRandom(float from, float to)
	{
		return (float)random.NextDouble() * (to - from) + from;
	}

	public static Vector2 GetRandomVectorInCircle(float radius)
	{
		return new Vector2(GetRandom(0f - radius, radius), GetRandom(0f - radius, radius));
	}

	public static Vector2 GetRandomVectorInRect(Vector2 size)
	{
		return new Vector2(GetRandom(0f, size.X), GetRandom(0f, size.Y));
	}

	public static float GetNormalRandom(float mean, float deviation)
	{
		float num = GetRandom(0f, 1f);
		float num2 = GetRandom(0f, 1f);
		float num3 = (float)(Math.Sqrt(-2.0 * Math.Log(num)) * Math.Sin(Math.PI * 2.0 * (double)num2));
		return mean + deviation * num3;
	}

	public static string GetVerboseRange(int value, int step)
	{
		if (step == 0)
		{
			return string.Empty;
		}
		int num = value / step * step;
		return num + "-" + (num + step);
	}
}
