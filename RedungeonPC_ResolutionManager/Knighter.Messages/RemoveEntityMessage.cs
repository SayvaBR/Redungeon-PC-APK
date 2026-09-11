using Knighter.Entities;

namespace Knighter.Messages;

public class RemoveEntityMessage : Message
{
	public readonly Entity Entity;

	public RemoveEntityMessage(Entity entity)
		: base(MessageType.RemoveEntity)
	{
		Entity = entity;
	}
}
