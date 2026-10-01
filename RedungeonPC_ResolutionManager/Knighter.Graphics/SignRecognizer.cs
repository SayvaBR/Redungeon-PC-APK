using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;

namespace Knighter.Graphics;

public class SignRecognizer : Component
{
	public readonly Dictionary<string, SignMeta> SignMetas;

	private readonly Dictionary<string, List<string>> data;

	public SignRecognizer()
	{
		SignMetas = new Dictionary<string, SignMeta>();
		data = new Dictionary<string, List<string>>();
	}

	public override void Load()
	{
		LoadTrainingData("Content/Other/signs.txt");
		base.Load();
	}

	private void LoadTrainingData(string filePath)
	{
		string text = string.Empty;
		using (StreamReader streamReader = new StreamReader(TitleContainer.OpenStream(filePath)))
		{
			text = streamReader.ReadToEnd();
		}

		string[] array = text.Split('\n');
		int num = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TrimEnd() == "===")
			{
				num = i;
				break;
			}
		}
		int num2 = num + 1;
		for (int j = 0; j < num; j += 2)
		{
			string[] array2 = array[j].TrimEnd().ToLower().Split(':');
			string key = array2[0];
			int num3 = int.Parse(array[j + 1].TrimEnd());
			SignMetas.Add(key, new SignMeta
			{
				Complexity = int.Parse(array2[1])
			});
			data[key] = new List<string>();
			for (int k = 0; k < num3; k++)
			{
				data[key].Add(array[num2 + k].TrimEnd());
			}
			num2 += num3;
		}
	}

	public bool RecognizeAgainst(List<Vector2> points, string signName, SignRotation rotation, bool mirrored)
	{
		int num = 100500;
		int num2 = -100500;
		int num3 = 100500;
		int num4 = -100500;
		foreach (Vector2 point in points)
		{
			int val = (int)point.X;
			int val2 = (int)point.Y;
			num = Math.Min(num, val);
			num2 = Math.Max(num2, val);
			num3 = Math.Min(num3, val2);
			num4 = Math.Max(num4, val2);
		}
		int num5 = num2 - num;
		int num6 = num4 - num3;
		if (num6 > 0 && num5 / num6 > 3)
		{
			int num7 = (num5 - num6) / 2;
			num3 -= num7;
			num4 += num7;
		}
		else if (num5 > 0 && num6 / num5 > 3)
		{
			int num8 = (num6 - num5) / 2;
			num -= num8;
			num2 += num8;
		}
		num5 = num2 - num;
		num6 = num4 - num3;
		int num9 = num5 / 3;
		int num10 = num6 / 3;
		char c = '-';
		string text = CountIntersections(points) + "x";
		string text2 = string.Empty;
		bool flag = false;
		switch (rotation)
		{
		case SignRotation.None:
			text2 = "ABCDEFGHI";
			flag = true;
			break;
		case SignRotation.Quarter:
			text2 = "CFIBEHADG";
			flag = false;
			break;
		case SignRotation.Half:
			text2 = "IHGFEDCBA";
			flag = true;
			break;
		case SignRotation.ThreeQuarter:
			text2 = "GDAHEBIFC";
			flag = false;
			break;
		}
		if (mirrored)
		{
			text2 = ((!flag) ? $"{text2[6]}{text2[7]}{text2[8]}{text2[3]}{text2[4]}{text2[5]}{text2[0]}{text2[1]}{text2[2]}" : $"{text2[2]}{text2[1]}{text2[0]}{text2[5]}{text2[4]}{text2[3]}{text2[8]}{text2[7]}{text2[6]}");
		}
		foreach (Vector2 point2 in points)
		{
			int num11 = (int)point2.X - num;
			int num12 = (int)point2.Y - num3;
			int num13 = ((num11 >= num9) ? ((num11 < num9 * 2) ? 1 : 2) : 0);
			int num14 = ((num12 >= num10) ? ((num12 < num10 * 2) ? 1 : 2) : 0);
			char c2 = text2[num14 * 3 + num13];
			if (c2 != c)
			{
				text += c2;
				c = c2;
			}
		}
		foreach (string item in data[signName])
		{
			if (OffByOne(text, item))
			{
				return true;
			}
		}
		return false;
	}

	private static bool OffByOne(string s1, string s2)
	{
		if (s1.Equals(s2))
		{
			return true;
		}
		if (s1.Equals(string.Empty) || s2.Equals(string.Empty))
		{
			return false;
		}
		int i;
		for (i = 0; s1.Length > i && s2.Length > i && s1[i] == s2[i]; i++)
		{
		}
		StringBuilder stringBuilder = new StringBuilder(s1);
		string text = ((s1.Length > i) ? stringBuilder.Remove(i, 1).ToString() : s1);
		stringBuilder = new StringBuilder(s2);
		string value = ((s2.Length > i) ? stringBuilder.Remove(i, 1).ToString() : s2);
		if (!s1.Equals(value) && !s2.Equals(text))
		{
			return text.Equals(value);
		}
		return true;
	}

	private static int CountIntersections(List<Vector2> points)
	{
		int num = 0;
		for (int i = 0; i < points.Count - 1; i++)
		{
			Vector2 a = points[i];
			Vector2 b = points[i + 1];
			for (int j = i + 2; j < points.Count - 1; j++)
			{
				Vector2 c = points[j];
				Vector2 d = points[j + 1];
				if (Intersect(a, b, c, d) && !AlmostParallel(a, b, c, d))
				{
					num++;
				}
			}
		}
		return num;
	}

	private static bool Ccw(Vector2 a, Vector2 b, Vector2 c)
	{
		return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
	}

	private static bool Intersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		if (Ccw(a, c, d) != Ccw(b, c, d))
		{
			return Ccw(a, b, c) != Ccw(a, b, d);
		}
		return false;
	}

	private static bool AlmostParallel(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		float num = (float)Math.Atan2(a.Y - b.Y, a.X - b.X);
		float num2 = (float)Math.Atan2(c.Y - d.Y, c.X - d.X);
		float num3 = num - num2;
		num3 = Math.Abs(num3 / (float)Math.PI);
		if (num3 > 1f)
		{
			num3 -= 1f;
		}
		if (!(num3 < 0.15f))
		{
			return num3 > 0.85f;
		}
		return true;
	}
}
