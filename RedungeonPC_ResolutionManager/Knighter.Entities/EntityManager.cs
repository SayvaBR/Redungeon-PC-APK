using System;
using System.Collections.Generic;
using Knighter.Helpers;
using Knighter.Messages;
using Microsoft.Xna.Framework;

namespace Knighter.Entities;

public class EntityManager : Component
{
	private readonly List<Entity> entities;

	private readonly List<Entity> entitiesToRemove;

	private readonly List<Pair<Entity, PlatformEntity>> entitiesToAdd;

	public EntityManager()
	{
		entities = new List<Entity>();
		entitiesToAdd = new List<Pair<Entity, PlatformEntity>>();
		entitiesToRemove = new List<Entity>();
	}

	public int EntityCount => entities.Count;

	public override void Load()
	{
		Subscribe(MessageType.SpawnEntity);
		Subscribe(MessageType.RemoveEntity);
		base.Load();
	}

	public override void Unload()
	{
		Unsubscribe(MessageType.SpawnEntity);
		Unsubscribe(MessageType.RemoveEntity);
		foreach (Entity entity in entities)
		{
			entity.Unload();
		}
		base.Unload();
	}

	private Entity Add(Entity entity, PlatformEntity platform)
	{
		entitiesToAdd.Add(new Pair<Entity, PlatformEntity>(entity, platform));
		return entity;
	}

	private void Remove(Entity entity)
	{
		entitiesToRemove.Add(entity);
	}

	public override void Update()
	{
		foreach (Entity item in entitiesToRemove)
		{
			item?.Unload();
			entities.Remove(item);
		}
		entitiesToRemove.Clear();
		foreach (Pair<Entity, PlatformEntity> item2 in entitiesToAdd)
		{
			Entity a = item2.A;
			PlatformEntity b = item2.B;
			a.CurrentPlatform = b;
			a.TryMoveToCoordinates(a.CurrentMap, a.Coordinates);
			a.Load();
			entities.Add(a);
		}
		entitiesToAdd.Clear();
		foreach (Entity entity in entities)
		{
			entity.Update();
		}
		base.Update();
	}

	public void Pause()
	{
		foreach (Entity entity in entities)
		{
			entity.Pause();
		}
	}

	public void Resume()
	{
		foreach (Entity entity in entities)
		{
			entity.Resume();
		}
	}

	public override void Draw()
	{
		foreach (Entity entity in entities)
		{
			base.core.Renderer.BeginOwnerScope(entity);
			entity.Draw();
			base.core.Renderer.EndOwnerScope();
		}
		base.Draw();
	}

	public override void OnMessage(Message message, object sender)
	{
		switch (message.Type)
		{
		case MessageType.SpawnEntity:
		{
			SpawnEntityMessage spawnEntityMessage = message as SpawnEntityMessage;
			Add(spawnEntityMessage.Entity, spawnEntityMessage.Platform);
			break;
		}
		case MessageType.RemoveEntity:
			Remove((message as RemoveEntityMessage).Entity);
			break;
		}
		base.OnMessage(message, sender);
	}

	public List<Entity> GetEntitiesInRadius(Vector2 worldCenterCoordinates, float radius)
	{
		return entities.FindAll((Entity e) => (worldCenterCoordinates - e.WorldCenterCoordinates).Length() <= radius);
	}

	public List<Entity> FindEntities(Func<Entity, bool> condition)
	{
		return entities.FindAll((Entity e) => condition(e));
	}
}
