#if DEBUG
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Knighter.Graphics;

namespace Knighter;

public class BootLogPanel : IDevPanel
{
	private const int MaxLogLines = 12;

	private static readonly List<string> lines = new List<string>();

	public string Title => "Boot Log";

	public Color TitleColor => new Color(130, 130, 140);

	public static void Log(string message)
	{
		lines.Add($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
		if (lines.Count > MaxLogLines)
		{
			lines.RemoveAt(0);
		}
	}

	public void Update(GameTime gameTime)
	{
	}

	public void CollectLines(Renderer renderer, List<string> output)
	{
		output.AddRange(lines);
	}
}
#endif
