using System;
using System.Diagnostics;

namespace Knighter;

public static class Debug
{
	[Conditional("DEBUG")]
	public static void Assert(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception(message);
		}
	}

	[Conditional("DEBUG")]
	public static void Print(string message)
	{
		Console.WriteLine($"> {message}");
	}
}
