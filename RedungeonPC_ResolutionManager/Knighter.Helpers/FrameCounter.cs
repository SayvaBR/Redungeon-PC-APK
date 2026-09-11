using System.Collections.Generic;
using System.Linq;

namespace Knighter.Helpers;

public sealed class FrameCounter
{
	public const int MaximumSamples = 100;

	private readonly Queue<float> sampleBuffer = new Queue<float>();

	public long TotalFrames { get; private set; }

	public float TotalSeconds { get; private set; }

	public float AverageFramesPerSecond { get; private set; }

	public float CurrentFramesPerSecond { get; private set; }

	public void Update(float deltaTime)
	{
		CurrentFramesPerSecond = 1f / deltaTime;
		sampleBuffer.Enqueue(CurrentFramesPerSecond);
		if (sampleBuffer.Count > 100)
		{
			sampleBuffer.Dequeue();
			AverageFramesPerSecond = sampleBuffer.Average((float i) => i);
		}
		else
		{
			AverageFramesPerSecond = CurrentFramesPerSecond;
		}
		long totalFrames = TotalFrames + 1;
		TotalFrames = totalFrames;
		TotalSeconds += deltaTime;
	}
}
