using System;

namespace Knighter.Helpers;

public static class DateTimeHelper
{
	public static string SafeNow()
	{
		try
		{
			return DateTime.Now.ToString();
		}
		catch
		{
			return string.Empty;
		}
	}
}
