using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Knighter.Helpers;

public static class Extensions
{
	public static void RemoveAll<T>(this LinkedList<T> list, Func<T, bool> condition)
	{
		LinkedListNode<T> linkedListNode = list.First;
		while (linkedListNode != null)
		{
			LinkedListNode<T> next = linkedListNode.Next;
			if (condition(linkedListNode.Value))
			{
				list.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
	}

	public static Color FromRgb(this Color c, int value)
	{
		c.A = byte.MaxValue;
		c.R = (byte)((value >> 16) % 256);
		c.G = (byte)((value >> 8) % 256);
		c.B = (byte)(value % 256);
		return c;
	}

	public static int Mod(this int x, int y)
	{
		int num = x % y;
		if (num < 0)
		{
			return num + y;
		}
		return num;
	}

	public static Vector2 XY(this Vector3 v)
	{
		return new Vector2(v.X, v.Y);
	}

	public static Vector2 XY(this Vector4 v)
	{
		return new Vector2(v.X, v.Y);
	}

	public static Vector2 Direction(this Vector2 v)
	{
		Vector2 zero = Vector2.Zero;
		if (float.IsNaN(v.X))
		{
			v.X = 0f;
		}
		if (float.IsNaN(v.Y))
		{
			v.Y = 0f;
		}
		if (Math.Abs(v.X) > Math.Abs(v.Y))
		{
			zero.X = Math.Sign(v.X);
		}
		else
		{
			zero.Y = Math.Sign(v.Y);
		}
		return zero;
	}

	public static string DirectionId(this Vector2 v)
	{
		if (v.Y > 0f)
		{
			return "s";
		}
		if (v.Y < 0f)
		{
			return "n";
		}
		if (!(v.X > 0f))
		{
			return "w";
		}
		return "e";
	}

	public static Vector2 Clone(this Vector2 v)
	{
		return new Vector2(v.X, v.Y);
	}

	public static Vector2 Copy(this Vector2 v, Vector2 other)
	{
		v.X = other.X;
		v.Y = other.Y;
		return v;
	}

	public static Vector2 Shift(this Vector2 v, float x, float y)
	{
		return new Vector2(v.X + x, v.Y + y);
	}

	public static void SetX(this Vector2 v, float x)
	{
		v.X = x;
	}

	public static void SetY(this Vector2 v, float y)
	{
		v.Y = y;
	}

	public static bool IsEqualTo(this Vector2 v, Vector2 other)
	{
		if (v.X.IsEqualTo(other.X))
		{
			return v.Y.IsEqualTo(other.Y);
		}
		return false;
	}

	public static Rectangle FromFloats(this Rectangle r, float x, float y, float width, float height)
	{
		r.X = (int)x;
		r.Y = (int)y;
		r.Width = (int)width;
		r.Height = (int)height;
		return r;
	}

	public static bool IsZero(this float f)
	{
		return Math.Abs(f) < SciHelper.Eps;
	}

	public static bool IsEqualTo(this float f, float other)
	{
		return (other - f).IsZero();
	}

	public static bool IsZero(this Vector3 v)
	{
		return v.Length() < SciHelper.Eps;
	}
}
