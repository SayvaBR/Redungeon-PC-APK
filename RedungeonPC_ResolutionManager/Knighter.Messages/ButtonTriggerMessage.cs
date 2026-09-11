namespace Knighter.Messages;

public class ButtonTriggerMessage : Message
{
	public readonly int Id;

	public readonly int ModuleIndex;

	public ButtonTriggerMessage(int id, int moduleIndex)
		: base(MessageType.ButtonTrigger)
	{
		Id = id;
		ModuleIndex = moduleIndex;
	}
}
