using System;
using Knighter.Localization;
using Knighter.Messages;
using Knighter.UI;
using Microsoft.Xna.Framework;

namespace Knighter.States;

public abstract class State : Component
{
	public enum TransType
	{
		None,
		In,
		Out
	}

	public bool IsOverlay;

	public bool IsOpaque;

	public bool Loaded;

	public bool ShowCoins = true;

	public TransType Transition;

	public int TransT;

	public CoreEvent SuspendedCoreEvent { get; private set; }

	public int TicksInState { get; protected set; }

	public int TransDuration { get; protected set; }

	/// <summary>
	/// A maioria das telas navegáveis usa o contrato A/Enter confirmar e
	/// B/Escape voltar. Estados com ações próprias desenham sua legenda
	/// contextual específica e desativam este padrão.
	/// </summary>
	protected virtual bool ShowContextPromptLegend => true;

	public bool IsTopState => base.core.GetCurrentState() == this;

	public int Trans
	{
		get
		{
			if (Transition != TransType.Out)
			{
				return TransDuration - TransT;
			}
			return TransT;
		}
	}

	public int TransReverse
	{
		get
		{
			if (Transition != TransType.In)
			{
				return TransDuration - TransT;
			}
			return TransT;
		}
	}

	public int TransD(int dt, int dd)
	{
		return Math.Min(Math.Max(Trans - dt - 1, 0), TransDuration - dd);
	}

	public int TransReverseD(int dt, int dd)
	{
		return Math.Min(Math.Max(TransReverse + dt, 0), TransDuration - dd);
	}

	public override void Load()
	{
		Loaded = true;
		base.Load();
	}

	public override void Unload()
	{
		base.core.MessageManager.UnsubscribeFromAll(this);
		base.Unload();
	}

	public virtual void HandleInput()
	{
	}

	public virtual void OnLeaveBehind()
	{
	}

	public virtual void OnReturn()
	{
	}

	public override void Update()
	{
		TicksInState++;
		base.Update();
	}

	public override void Draw()
	{
		if (ShowContextPromptLegend && IsTopState && Transition == TransType.None)
		{
			ContextPromptLegend.DrawVertical(
				base.core.Renderer["fg", 18000, false],
				new Vector2(base.core.Renderer.ScreenWidth - 115f, base.core.Renderer.ScreenHeight - 40f),
				new ContextPrompt(PromptAction.Confirm, __(SId.PROMPT_confirm)),
				new ContextPrompt(PromptAction.Cancel, __(SId.PROMPT_cancel)));
		}
	}

	public void TransitionIn()
	{
		Transition = TransType.In;
		TransT = TransDuration;
		UpdateTransition();
	}

	public virtual void UpdateTransition()
	{
	}

	public void TransitionOut(CoreEvent coreEvent)
	{
		Transition = TransType.Out;
		TransT = TransDuration;
		SuspendedCoreEvent = coreEvent;
		if (TransT == 0)
		{
			OnOutTransitionDone();
		}
	}

	public virtual void OnOutTransitionDone()
	{
		SendMessage(new CoreEventMessage(SuspendedCoreEvent));
	}

	public virtual void OnBackButtonPressed()
	{
	}

	/// <summary>
	/// Ação exclusiva do botão Start: iniciar na tela de título ou alternar pausa.
	/// Estados comuns não o tratam como um botão universal de voltar.
	/// </summary>
	public virtual void OnStartButtonPressed()
	{
	}
}
