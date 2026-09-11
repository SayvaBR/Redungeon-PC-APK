using System.Collections.Generic;

namespace Knighter.Gameplay;

public class StateMachine<TState, TEvent> where TState : struct where TEvent : struct
{
	public class StateConfig
	{
		public readonly TState State;

		public readonly StateMachine<TState, TEvent> Owner;

		public readonly Dictionary<TEvent, TState> Transitions;

		public bool AutoTransition;

		public TState AutoTransitionTarget;

		public int AutoDelay = -1;

		public StateConfig(StateMachine<TState, TEvent> machine, TState state)
		{
			Owner = machine;
			State = state;
			Transitions = new Dictionary<TEvent, TState>();
		}

		public StateConfig On(TEvent eventType, TState state)
		{
			Transitions.Add(eventType, state);
			return this;
		}

		public StateConfig AutoTransitionTo(TState state)
		{
			AutoTransition = true;
			AutoTransitionTarget = state;
			return this;
		}

		public StateConfig IsInitial()
		{
			Owner.SetInitialState(State);
			return this;
		}

		public StateConfig After(int delay)
		{
			AutoDelay = delay;
			return this;
		}

		public StateConfig ForcedOn(TEvent eventType)
		{
			Owner.forcedTransitions.Add(eventType, State);
			return this;
		}
	}

	private readonly Dictionary<TState, StateConfig> states;

	private TState initialState;

	private StateConfig currentStateConfig;

	public TState PrevState;

	protected readonly Dictionary<TEvent, TState> forcedTransitions;

	public int TicksInState { get; private set; }

	public TState CurrentState { get; private set; }

	public bool JustEnteredState => TicksInState == 1;

	public StateMachine()
	{
		states = new Dictionary<TState, StateConfig>();
		forcedTransitions = new Dictionary<TEvent, TState>();
		TicksInState = 0;
	}

	public StateConfig State(TState state)
	{
		if (states.Count == 0)
		{
			initialState = state;
		}
		StateConfig stateConfig;
		if (states.ContainsKey(state))
		{
			stateConfig = states[state];
		}
		else
		{
			stateConfig = new StateConfig(this, state);
			states.Add(state, stateConfig);
		}
		return stateConfig;
	}

	public void SetInitialState(TState state)
	{
		initialState = state;
	}

	public void Start()
	{
		PrevState = initialState;
		EnterState(initialState);
	}

	public bool EnterState(TState state)
	{
		if (!states.ContainsKey(state))
		{
			return false;
		}
		PrevState = CurrentState;
		CurrentState = state;
		currentStateConfig = states[state];
		TicksInState = 0;
		return true;
	}

	public bool IsIn(TState state)
	{
		return EqualityComparer<TState>.Default.Equals(state, CurrentState);
	}

	public bool Trigger(TEvent eventType)
	{
		if (currentStateConfig == null)
		{
			return false;
		}
		if (currentStateConfig.Transitions.ContainsKey(eventType))
		{
			EnterState(currentStateConfig.Transitions[eventType]);
			return true;
		}
		if (forcedTransitions.ContainsKey(eventType))
		{
			EnterState(forcedTransitions[eventType]);
			return true;
		}
		return false;
	}

	public void Update()
	{
		int ticksInState = TicksInState + 1;
		TicksInState = ticksInState;
		if (currentStateConfig != null && currentStateConfig.AutoTransition && currentStateConfig.AutoDelay >= 0 && TicksInState >= currentStateConfig.AutoDelay)
		{
			EnterState(currentStateConfig.AutoTransitionTarget);
		}
	}
}
