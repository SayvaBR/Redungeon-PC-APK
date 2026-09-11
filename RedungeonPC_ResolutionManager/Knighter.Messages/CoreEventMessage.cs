namespace Knighter.Messages;

public class CoreEventMessage : Message
{
	public readonly CoreEvent CoreEvent;

	public CoreEventMessage(CoreEvent coreEvent)
		: base(MessageType.CoreEvent)
	{
		CoreEvent = coreEvent;
	}
}
