using Knighter.States;

namespace Knighter.Messages;

public class PushStateMessage : Message
{
	public readonly State State;

	public PushStateMessage(State state)
		: base(MessageType.PushState)
	{
		State = state;
	}
}
