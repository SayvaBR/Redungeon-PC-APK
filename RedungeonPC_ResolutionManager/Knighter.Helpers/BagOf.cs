using System;
using System.Collections.Generic;
using System.Linq;

namespace Knighter.Helpers;

public class BagOf<T>
{
	private readonly List<Pair<T, int>> contents;

	private bool drawnFirst;

	private T last;

	public int Count => contents.Count;

	public BagOf<T> Clone()
	{
		BagOf<T> bagOf = new BagOf<T>();
		foreach (Pair<T, int> content in contents)
		{
			bagOf.Put(content.A, content.B);
		}
		return bagOf;
	}

	public BagOf()
	{
		contents = new List<Pair<T, int>>();
	}

	public BagOf<T> Put(T item, int quantity = 1)
	{
		contents.Add(new Pair<T, int>(item, quantity));
		return this;
	}

	public void Clear()
	{
		contents.Clear();
	}

	public T Draw()
	{
		return DrawFrom(contents);
	}

	public T Draw(Func<T, bool> condition)
	{
		return DrawFrom(contents.Where((Pair<T, int> item) => condition(item.A)).ToList());
	}

	public T DrawAndRemove()
	{
		T item = Draw();
		Pair<T, int> pair = contents.Find((Pair<T, int> p) => p.A.Equals(item));
		pair.B--;
		if (pair.B <= 0)
		{
			contents.Remove(pair);
		}
		return item;
	}

	private static T DrawFrom(List<Pair<T, int>> set)
	{
		if (set.Count == 0)
		{
			return default(T);
		}
		int to = set.Sum((Pair<T, int> x) => x.B);
		int random = SciHelper.GetRandom(1, to);
		int num = -1;
		int num2 = 0;
		do
		{
			num++;
			num2 += set[num].B;
		}
		while (num2 < random && num < set.Count - 1);
		return set[num].A;
	}

	public T DrawDifferent()
	{
		if (contents.Count == 1)
		{
			return Draw();
		}
		last = Draw((T t) => !drawnFirst || !t.Equals(last));
		drawnFirst = true;
		return last;
	}
}
