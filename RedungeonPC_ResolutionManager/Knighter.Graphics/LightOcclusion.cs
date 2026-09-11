using System;
using System.Collections.Generic;
using Knighter.Entities;
using Knighter.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Knighter.Graphics;

/// <summary>2D visibility fans. Three source samples soften the edge without 3D shadow maps.</summary>
public sealed class LightOcclusion : IDisposable
{
	private const int Rays = 128;
	private readonly BasicEffect effect;
	private readonly GraphicsDevice device;
	private readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[Rays * 3];
	private readonly Vector2[] ends = new Vector2[Rays];
	private readonly Vector2[] directions = new Vector2[Rays];
	private readonly List<RectangleF> blockers = new();
	private readonly List<RectangleF> nearbyBlockers = new();
	public LightOcclusion(GraphicsDevice device)
	{
		this.device = device;
		effect = new BasicEffect(device) { TextureEnabled = true, VertexColorEnabled = true, LightingEnabled = false };
		for (int ray = 0; ray < Rays; ray++)
		{
			float angle = ray * MathHelper.TwoPi / Rays;
			directions[ray] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
		}
	}
	public void Gather(IEnumerable<Entity> entities)
	{
		blockers.Clear();
		foreach (Entity entity in entities)
		{
			if (entity.IsBroken || entity.Unloaded) continue;
			if (entity is WallEntity || entity is StatueEntity || entity is ObstacleEntity { CastsLightShadow: true })
			{
				var p = entity.WorldCoordinates * 16f;
				float inset = entity is WallEntity ? 0f : 3f;
				blockers.Add(new RectangleF(p.X + inset, p.Y + inset, 16f - inset * 2, 16f - inset * 2));
			}
		}
	}
	public static float RayDistance(Vector2 origin, Vector2 direction, RectangleF box, float maximum)
	{
		if (box.Contains(origin)) return maximum; // Allow sources mounted on a wall.
		float near = 0, far = maximum;
		if (!Slab(origin.X, direction.X, box.Left, box.Right, ref near, ref far) ||
			!Slab(origin.Y, direction.Y, box.Top, box.Bottom, ref near, ref far)) return maximum;
		return near;
	}
	private static bool Slab(float origin, float direction, float min, float max, ref float near, ref float far)
	{
		if (MathF.Abs(direction) < 0.00001f) return origin >= min && origin <= max;
		float a = (min - origin) / direction, b = (max - origin) / direction;
		near = MathF.Max(near, MathF.Min(a, b)); far = MathF.Min(far, MathF.Max(a, b));
		return far >= near;
	}
	public void Draw(Light light, Texture2D mask, Color tint, Renderer renderer, Matrix transform)
	{
		float radius = light.ActualRadius * 1.3f;
		// Reject distant boxes once per light, before the three visibility fans.
		nearbyBlockers.Clear();
		Vector2 position = light.ActualPosition;
		foreach (var box in blockers)
		{
			float dx = MathF.Max(MathF.Max(box.Left - position.X, position.X - box.Right), 0f);
			float dy = MathF.Max(MathF.Max(box.Top - position.Y, position.Y - box.Bottom), 0f);
			if (dx * dx + dy * dy <= (radius + 2f) * (radius + 2f)) nearbyBlockers.Add(box);
		}
		effect.World = transform;
		effect.View = Matrix.Identity;
		effect.Projection = Matrix.CreateOrthographicOffCenter(0, renderer.BufferWidth, renderer.BufferHeight, 0, 0, 1);
		effect.Texture = mask;
		device.BlendState = Renderer.ScreenBlend;
		device.DepthStencilState = DepthStencilState.None;
		device.RasterizerState = RasterizerState.CullNone;
		device.SamplerStates[0] = SamplerState.LinearClamp;
		for (int sample = 0; sample < 3; sample++)
		{
			Vector2 source = light.ActualPosition + new Vector2((sample - 1) * 1.5f, sample == 1 ? -1f : 0.5f);
			for (int ray = 0; ray < Rays; ray++)
			{
				Vector2 direction = directions[ray];
				float reach = radius;
				foreach (var box in nearbyBlockers)
					reach = MathF.Min(reach, RayDistance(source, direction, box, reach));
				ends[ray] = source + direction * MathF.Min(radius, reach + 2f);
			}
			Color color = tint * (1f / 3f);
			for (int ray = 0; ray < Rays; ray++)
			{
				vertices[ray * 3] = Vertex(source, source, radius, color, renderer);
				vertices[ray * 3 + 1] = Vertex(ends[ray], source, radius, color, renderer);
				vertices[ray * 3 + 2] = Vertex(ends[(ray + 1) % Rays], source, radius, color, renderer);
			}
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, Rays);
			}
		}
	}
	private static VertexPositionColorTexture Vertex(Vector2 p, Vector2 source, float radius, Color tint, Renderer renderer) =>
		new(new Vector3(renderer.ToScreen(p), 0f), tint, (p - source) / (radius * 2f) + new Vector2(0.5f));
	public void Dispose() => effect.Dispose();
}
