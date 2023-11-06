using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController _characterController;
    [SerializeField] private float speed = 5f;

    [SerializeField] private float runMultiMax = 1.5f;
    private float runMultidefault = 1;
    private float currMulti = 1;
    [SerializeField] private Transform visualHolder;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.0f;

    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private DefaultControls _controls;

    private float input;

    private bool _isSprint = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _controls = new DefaultControls();
        _controls.Enable();
        _controls.Movement.Jump.started += JumpOnperformed;
        _controls.Movement.Jump.canceled += JumpOncanceled;

        _controls.Movement.Sprint.started += context => _isSprint = true;
        _controls.Movement.Sprint.canceled += context => _isSprint = false;
    }

    private void JumpOncanceled(InputAction.CallbackContext obj)
    {
        isJumping = false;
    }

    private void JumpOnperformed(InputAction.CallbackContext obj)
    {
        bool isNearGround = Physics.Raycast(transform.position, Vector3.down, groundDetectionRayLenght);

        Debug.DrawRay(transform.position, Vector3.down * groundDetectionRayLenght,
            isNearGround ? Color.green : Color.red);
        if ((isNearGround || groundedPlayer || _coyoteTime < _coyoteMax))
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravity);
            _coyoteTime = _coyoteMax;
            isJumping = true;
            jumpTime = Time.time;
        }
    }

    private float _coyoteTime = 0f;
    [SerializeField] private float _coyoteMax = .15f;

    [SerializeField] private float jumpMax = .3f;
    private float jumpTime;
    private bool isJumping;

    [SerializeField] private float groundDetectionRayLenght = .15f;


    void Update()
    {
        //horizontal movement
        var h = _controls.Movement.Horizontal.ReadValue<float>();
        var dir = Vector3.right * h;
        currMulti = Mathf.Lerp(currMulti, _isSprint ? runMultiMax : runMultidefault,
            Time.deltaTime * 10);

        _characterController.Move(dir * (speed * currMulti * Time.deltaTime));
        if (Mathf.Abs(h) > .1f)
        {
            visualHolder.forward = dir.normalized;
        }

        //jump
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
        var currCollision = _characterController.Move(playerVelocity * Time.deltaTime);
        groundedPlayer = currCollision == CollisionFlags.Below;

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
}