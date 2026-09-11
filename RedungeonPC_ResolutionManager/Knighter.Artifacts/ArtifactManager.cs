using System;
using System.Collections.Generic;

namespace Knighter.Artifacts;

public class ArtifactManager : Component
{
	private readonly Dictionary<Artifact, bool> opened;

	public Artifact Current { get; private set; }

	public ArtifactManager()
	{
		opened = new Dictionary<Artifact, bool>();
		foreach (Artifact value in Enum.GetValues(typeof(Artifact)))
		{
			if (value != Artifact.None)
			{
				opened.Add(value, value: false);
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public void Enable(Artifact artifact)
	{
		if (opened[artifact])
		{
			Current = artifact;
		}
	}

	public void Disable(Artifact artifact)
	{
		Current = Artifact.None;
	}

	public bool IsOpened(Artifact artifact)
	{
		return opened[artifact];
	}

	public void LoadFromStorage()
	{
		foreach (Artifact value in Enum.GetValues(typeof(Artifact)))
		{
			if (value != Artifact.None)
			{
				bool result = false;
				base.core.Storage.TryGetBool($"artifact-{value}", ref result);
				opened[value] = result;
			}
		}
	}

	public void SaveIntoStorage()
	{
		foreach (Artifact value in Enum.GetValues(typeof(Artifact)))
		{
			if (value != Artifact.None)
			{
				base.core.Storage.SetBool($"artifact-{value}", opened[value]);
			}
		}
	}
}
