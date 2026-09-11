namespace Knighter.Messages;

public class ScreenshotMessage : Message
{
	public readonly WhenToTakeScreenshot When;

	public readonly Screenshot Screenshot;

	public ScreenshotMessage(WhenToTakeScreenshot when, Screenshot screenshot)
		: base(MessageType.Screenshot)
	{
		When = when;
		Screenshot = screenshot;
	}
}
