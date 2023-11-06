using System;
using Scipts.States;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scipts
{
    public class PlayerStateHandler : MonoBehaviour
    {
        private StateMachine _stateMachine;
        private StateMachine _gravityStateMachine;

        [SerializeField] private GroundedState groundedState;
        [SerializeField] private AirborneState airborneState;
        [SerializeField] private DashState dashState;
        [SerializeField] private DiveState diveState;

        private IdleState _idleState;
        private DefaultControls _controls;
        public CharacterController CharacterController { get; private set; }

        private void Awake()
        {
            _controls = new DefaultControls();
            _controls.Enable();
            groundedState.SetControl(_controls);
            airborneState.SetControl(_controls);
            _idleState = new IdleState();
            CharacterController = GetComponent<CharacterController>();
            _stateMachine = new StateMachine("Player State Machine");
            _gravityStateMachine = new StateMachine("Gravity State Machine");
            _controls = new DefaultControls();
            _controls.Enable();
            _stateMachine.SetState(groundedState);

            //_stateMachine.AddTransition(_groundedState, _dashState, () => _controls.Movement.Dash.IsPressed());
            _stateMachine.AddTransition(dashState, groundedState, () => dashState.IsCompleted);
            _stateMachine.AddTransition(_idleState, groundedState, () => diveState.IsCompleted);

            _gravityStateMachine.SetState(airborneState);
            //_gravityStateMachine.AddTransition(airborneState, diveState, () => _controls.Movement.Dive.IsPressed());
            _gravityStateMachine.AddTransition(_idleState, airborneState, () => dashState.IsCompleted);
            _gravityStateMachine.AddTransition(diveState, airborneState, () => diveState.IsCompleted);
        }

        private void Update()
        {
            if (dashState.IsReady && _controls.Movement.Dash.IsPressed() &&
                _stateMachine._currentState.GetType() != typeof(DashState))
            {
                _stateMachine.SetState(dashState);
                _gravityStateMachine.SetState(_idleState);
            }

            if (!airborneState.IsGrounded && diveState.IsReady && _controls.Movement.Dive.IsPressed() &&
                _gravityStateMachine._currentState.GetType() != typeof(DiveState))
            {
                _stateMachine.SetState(_idleState);
                _gravityStateMachine.SetState(diveState);
            }

            _stateMachine.Tick();
            _gravityStateMachine.Tick();
        }
    }
}