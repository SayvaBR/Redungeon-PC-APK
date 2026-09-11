namespace Knighter.Messages;

public class ReceivedProductsMessage : Message
{
	public ReceivedProductsMessage()
		: base(MessageType.ReceivedProducts)
	{
	}
}
