using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scipts.States
{
    public class AirborneState : MonoBehaviour, IState, IInputListener
    {
        public bool canTransitionItSelf { get; set; }

        private Vector3 playerVelocity;
        private bool groundedPlayer;
        public bool IsGrounded => groundedPlayer;
        private DefaultControls _controls;
        private CharacterController _controller;

        private float _coyoteTime = 0f;
        [SerializeField] private float _coyoteMax = .15f;

        [SerializeField] private float jumpMax = .3f;
        private float jumpTime;
        private bool isJumping;

        [SerializeField] private float groundDetectionRayLenght = .15f;

        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpHeight = 1.0f;
        private Animator _animator;

        [SerializeField] private ParticleSystem _particleSystem;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void SetControl(DefaultControls controls)
        {
            _controls = controls;
            _controls.Movement.Jump.started += JumpOnperformed;
            _controls.Movement.Jump.canceled += JumpOncanceled;
        }

        private void JumpOncanceled(InputAction.CallbackContext obj)
        {
            isJumping = false;
        }

        private void JumpOnperformed(InputAction.CallbackContext obj)
        {
            bool isNearGround = Physics.Raycast(_controller.transform.position, Vector3.down, groundDetectionRayLenght);


            Debug.DrawRay(_controller.transform.position, Vector3.down * groundDetectionRayLenght,
                isNearGround ? Color.green : Color.red);
            if ((isNearGround || groundedPlayer || _coyoteTime < _coyoteMax))
            {
                _particleSystem.Play();
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
                _coyoteTime = _coyoteMax;
                isJumping = true;
                jumpTime = Time.time;
            }
        }

        public void Tick()
        {
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = -1;
            }

            if (isJumping && _controls.Movement.Jump.IsPressed())
            {
                if (Time.time - jumpTime <= jumpMax)
                {
                    playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
                }
                else
                {
                    isJumping = false;
                }
            }


            if (!groundedPlayer && playerVelocity.y < 0)
            {
                _coyoteTime += Time.deltaTime;
            }

            playerVelocity.y += gravity * Time.deltaTime;
            var currCollision = _controller.Move(playerVelocity * Time.deltaTime);
            var previous = groundedPlayer;
            groundedPlayer = currCollision == CollisionFlags.Below;
            if (previous)
            {
                if (true != groundedPlayer)
                    _animator.CrossFade("Airborne", .1f);
            }
            else
            {
                if (groundedPlayer)
                    _animator.CrossFade("Ground", .1f);
            }

            _animator.SetFloat("Jump", playerVelocity.y);
            if ((currCollision & CollisionFlags.Above) != 0)
            {
                isJumping = false;
                playerVelocity.y = -1;
            }

            playerVelocity.y = Mathf.Max(playerVelocity.y, gravity);

            if (groundedPlayer)
            {
                isJumping = false;
                _coyoteTime = 0;
            }
        }

        public void OnEnter()
        {
            playerVelocity.y = 0;
        }

        public void OnExit()
        {
            playerVelocity.y = 0;
        }
    }
}