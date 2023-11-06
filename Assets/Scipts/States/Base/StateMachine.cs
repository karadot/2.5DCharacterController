using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

    public class StateMachine
    {
        public IState _currentState { get; private set; }

        private Dictionary<Type, List<Transition>> _transitions = new Dictionary<Type, List<Transition>>();

        private List<Transition> _currentTransitions = new List<Transition>();
        private List<Transition> _anyTransitions = new List<Transition>();

        private static List<Transition> _emptyTransitions = new List<Transition>(0);

        private string name;

        public StateMachine(string name)
        {
            this.name = name;
        }

        public void Tick()
        {
            var transition = GetTransition();

            if (transition != null)
                SetState(transition.To);

            _currentState.Tick();
        }

        public void SetState(IState state)
        {
            if (state.canTransitionItSelf == false && state == _currentState)
                return;

            _currentState?.OnExit();
            _currentState = state;

            _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);

            if (_currentTransitions == null)
                _currentTransitions = _emptyTransitions;

            _currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var toCreate))
            {
                toCreate = new List<Transition>();
                _transitions[from.GetType()] = toCreate;
            }

            toCreate.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState to, Func<bool> predicate)
        {
            _anyTransitions.Add(new Transition(to, predicate));
        }

        private Transition GetTransition()
        {
            for (var index = 0; index < _anyTransitions.Count; index++)
            {
                var transition = _anyTransitions[index];
                if (transition.Condition() && (_currentState != transition.To ||
                                               (_currentState == transition.To && _currentState.canTransitionItSelf)))
                {
                    //Debug.Log("nameof(_currentState) + = " + nameof(_currentState) + " " + nameof(transition.To));
                    return transition;
                }
            }

            for (var index = 0; index < _currentTransitions.Count; index++)
            {
                var transition = _currentTransitions[index];
                if (transition.Condition() && (_currentState != transition.To ||
                                               (_currentState == transition.To && _currentState.canTransitionItSelf)))
                {
                    //Debug.Log(name + " " + _currentState.GetType() + " " + transition.To.GetType());
                    return transition;
                }
            }

            return null;
        }
    }
