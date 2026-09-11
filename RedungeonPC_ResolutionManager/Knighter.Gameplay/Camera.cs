using Knighter.Entities;
using Knighter.Helpers;
using Microsoft.Xna.Framework;

namespace Knighter.Gameplay;

public class Camera : Component
{
	private Vector2 position;

	private PlatformEntity platform;

	private Vector2 target;

	private Entity targetEntity;

	private int shakeDuration;

	private float shakeStrength;

	private string shakeId;

	private Vector2 shakeOffset;

	public HandleBox ZoomBox;

	public HandleBox YOffsetBox;

	public Vector2 Offset => shakeOffset - base.core.Renderer.ScreenCenter / Zoom - new Vector2(0f, (float)base.core.Renderer.ScreenHeight * 0.1f) / Zoom + new Vector2(0f, YOffsetBox.Value) / Zoom;

	public Vector2 Position => ((platform == null) ? Vector2.Zero : platform.WorldPosition) + position + Offset;

	public float Zoom => ZoomBox.Value / Settings.GuiScale;

	public Camera()
	{
		targetEntity = null;
		shakeOffset = Vector2.Zero;
		ZoomBox = new HandleBox();
		YOffsetBox = new HandleBox();
	}

	public void SetTarget(Vector2 newTarget)
	{
		target = newTarget.Clone();
	}

	public void JumpTo(Vector2 newPosition)
	{
		JumpTo(newPosition.X, newPosition.Y);
	}

	public void JumpTo(float x, float y)
	{
		position = new Vector2(x, y);
		target = position.Clone();
	}

	public void JumpToTarget()
	{
		position = target.Clone();
	}

	public void Follow(Entity target)
	{
		targetEntity = target;
		UpdateTarget();
	}

	public void Shake(string id, float strength = 2f, int duration = 10)
	{
		if (id == shakeId)
		{
			shakeStrength = strength;
			shakeDuration = duration;
		}
		else if (strength > shakeStrength)
		{
			shakeStrength = strength;
			shakeDuration = duration;
			shakeId = id;
		}
	}

	private void UpdateOrigin(PlatformEntity newPlatform)
	{
		if (newPlatform != platform)
		{
			position += ((platform == null) ? Vector2.Zero : platform.WorldPosition);
			position -= newPlatform?.WorldPosition ?? Vector2.Zero;
			if (targetEntity == null)
			{
				target += ((platform == null) ? Vector2.Zero : platform.WorldPosition);
				target -= newPlatform?.WorldPosition ?? Vector2.Zero;
			}
		}
		platform = newPlatform;
	}

	public void UpdateTarget()
	{
		if (targetEntity != null)
		{
			if (targetEntity.CurrentPlatform != null)
			{
				UpdateOrigin(targetEntity.CurrentPlatform);
				target = targetEntity.Center;
			}
			else
			{
				UpdateOrigin(null);
				target = targetEntity.WorldCenter;
			}
			if (targetEntity.TeleportPending && targetEntity.DestTeleport != null)
			{
				UpdateOrigin(targetEntity.DestTeleport.CurrentPlatform);
				target = targetEntity.DestTeleport.Center;
			}
		}
		else
		{
			UpdateOrigin(null);
		}
	}

	public override void Update()
	{
		if (shakeDuration > 0)
		{
			shakeOffset = SciHelper.GetRandomVectorInCircle(shakeStrength);
			shakeDuration--;
		}
		else
		{
			shakeOffset = Vector2.Zero;
			shakeStrength = 0f;
			shakeId = "";
		}
		UpdateTarget();
		position += (target - position) * 0.05f;
		ZoomBox.Update();
		YOffsetBox.Update();
		base.Update();
	}
}
