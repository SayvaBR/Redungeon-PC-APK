using System.Collections.Generic;
using Knighter.Graphics;
using Knighter.Helpers;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public sealed class ResetProgressState : State
{
	protected override bool ShowContextPromptLegend => false;
	private readonly MenuFocusController navigation;
	private readonly List<IMenuWidget> buttons;
	private RectangleF panel;
	private bool committed;
#if DEBUG
	internal bool ConfirmForQA;
#endif
	public ResetProgressState()
	{
		IsOverlay = true; IsOpaque = true; ShowCoins = false; TransDuration = 12;
		navigation = new MenuFocusController(core.Input);
		buttons = new List<IMenuWidget> {
			new MenuFooterButtonWidget(SId.MISC_no, OnBackButtonPressed, MenuTheme.Border),
			new MenuFooterButtonWidget(SId.MISC_yes, Confirm, MenuTheme.Accent) };
		Layout();
		navigation.SetWidgets(buttons);
	}
	private void Layout()
	{
		panel = new RectangleF(R.ScreenCenter.X - 140, R.ScreenCenter.Y - 65, 280, 130);
		buttons[0].Bounds = new RectangleF(panel.X + 12, panel.Bottom - 36, 122, 25);
		buttons[1].Bounds = new RectangleF(panel.X + 146, panel.Bottom - 36, 122, 25);
	}
	public override void Update()
	{
		Layout();
		if (Transition == TransType.None && !committed) navigation.Update();
#if DEBUG
		if (Transition == TransType.None && ConfirmForQA && !committed) Confirm();
#endif
		base.Update();
	}
	public override void Draw()
	{
		R["fg", 2000, false].FillScreen(Color.Black * 0.9f);
		MenuTheme.Panel(R, panel, 2001);
		R["fg", 2020, false].DrawTextS(__(SId.PC_reset_warning), panel.TopLeft + new Vector2(12, 12),
			TextProfile.OrangeBoldText.Alter(width: 256, height: 70, scale: 0.8f, boxAlignment: Alignment2D.Left, textAlignment: Alignment2D.Left));
		navigation.Draw(); base.Draw();
	}
	private void Confirm() { if (committed) return; committed = true; core.ResetLocalProgress(); }
	public override void OnBackButtonPressed() { if (!committed) TransitionOut(CoreEvent.PopState); }
}
