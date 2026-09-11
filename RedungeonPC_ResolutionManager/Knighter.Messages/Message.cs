namespace Knighter.Messages;

public class Message : Component
{
	public MessageType Type { get; private set; }

	public Message(MessageType type)
	{
		Type = type;
	}
}
