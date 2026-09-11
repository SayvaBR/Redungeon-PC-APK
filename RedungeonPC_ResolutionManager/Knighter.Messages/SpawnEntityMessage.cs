using Knighter.Entities;

namespace Knighter.Messages;

public class SpawnEntityMessage : Message
{
	public readonly Entity Entity;

	public readonly PlatformEntity Platform;

	public SpawnEntityMessage(Entity entity, PlatformEntity platform)
		: base(MessageType.SpawnEntity)
	{
		Entity = entity;
		Platform = platform;
	}
}
