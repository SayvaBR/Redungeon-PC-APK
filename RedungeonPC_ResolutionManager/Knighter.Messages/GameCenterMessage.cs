namespace Knighter.Messages;

public class GameCenterMessage : Message
{
	public readonly GameCenterEvent GameCenterEvent;

	public GameCenterMessage(GameCenterEvent gameCenterEvent)
		: base(MessageType.GameCenter)
	{
		GameCenterEvent = gameCenterEvent;
	}
}
