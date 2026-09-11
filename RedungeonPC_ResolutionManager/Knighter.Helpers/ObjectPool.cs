using System.Collections.Generic;

namespace Knighter.Helpers;

public class ObjectPool<T> where T : new()
{
	private readonly List<T> pool;

	private int position;

	private int capacity;

	public ObjectPool(int capacity)
	{
		this.capacity = capacity;
		pool = new List<T>(capacity);
		for (int i = 0; i < capacity; i++)
		{
			pool.Add(new T());
		}
	}

	public T Get()
	{
		if (position == pool.Count) pool.Add(new T());
		return pool[position++];
	}

	public void Clear()
	{
		position = 0;
	}
}
