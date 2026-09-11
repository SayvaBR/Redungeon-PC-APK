using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Knighter;

public class PostEffectManager : Component
{
	public float DrunkF;

	public float DrunkA;

	public float DrunkDoublingA;

	public float InnerVignette;

	public float OuterVignette;

	public Color VignetteTint;

	public Vector2 SpotlightCenter;

	public float SpotlightRadius;

	public float SpotlightGrain;
	public float SpotlightOpacity;

	public bool Enabled => EnabledEffects.Count > 0;

	public List<PostEffectType> EnabledEffects { get; private set; }

	public PostEffectManager()
	{
		foreach (PostEffectType value in Enum.GetValues(typeof(PostEffectType)))
		{
			ResetEffect(value);
		}
		EnabledEffects = new List<PostEffectType>();
	}

	public void Reset()
	{
		foreach (PostEffectType value in Enum.GetValues(typeof(PostEffectType)))
		{
			DisableEffect(value);
			ResetEffect(value);
		}
	}

	public void EnableEffect(PostEffectType postEffect)
	{
		if (!EnabledEffects.Contains(postEffect))
		{
			EnabledEffects.Add(postEffect);
		}
	}

	public void DisableEffect(PostEffectType postEffect)
	{
		if (EnabledEffects.Contains(postEffect))
		{
			EnabledEffects.Remove(postEffect);
		}
	}

	public void ResetEffect(PostEffectType postEffect)
	{
		switch (postEffect)
		{
		case PostEffectType.Drunk:
			DrunkF = 1f;
			DrunkA = 0.03f;
			DrunkDoublingA = 0.01f;
			break;
		case PostEffectType.Vignette:
			InnerVignette = 0.7f;
			OuterVignette = 1.2f;
			VignetteTint = new Color(0.54f, 0.76f, 0f);
			break;
		case PostEffectType.Spotlight:
			SpotlightCenter = new Vector2(0.5f, 0.6f);
			SpotlightRadius = 0.3f;
			SpotlightGrain = 150f;
			SpotlightOpacity = 0f;
			break;
		}
	}
}
