using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scipts.States
{
    public class DiveState : MonoBehaviour, IState
    {
        [SerializeField] private float diveGravity;
        private CharacterController _controller;
        public bool canTransitionItSelf { get; set; }

        public bool IsCompleted { get; private set; }

        [SerializeField] private AnimationCurve diveCurve;

        private float _curveTimer = 0f;

        [SerializeField] private float cooldown = 3f;

        public bool IsReady { get; private set; } = true;

        private Animator _animator;

        [SerializeField] private List<TrailRenderer> diveTrails;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void Tick()
        {
            if (IsCompleted) return;
            _curveTimer += Time.deltaTime;
            var collisionFlags =
                _controller.Move(Vector3.down * ((diveCurve.Evaluate(_curveTimer) * diveGravity) * Time.deltaTime));
            if (collisionFlags == CollisionFlags.Below)
                IsCompleted = true;
        }

        private int _lastAnimStateHash;

        public void OnEnter()
        {
            IsCompleted = false;
            IsReady = false;
            _curveTimer = 0;
            _animator.CrossFade("Dive", .1f);
            ToggleTrails(true);
        }

        void ToggleTrails(bool active)
        {
            for (int i = 0; i < diveTrails.Count; i++)
            {
                diveTrails[i].emitting = active;
            }
        }

        public void OnExit()
        {
            ToggleTrails(false);
            IsCompleted = false;
            _curveTimer = 0;
            StartCoroutine(StartCooldown());
            _animator.CrossFade(_lastAnimStateHash, .1f);
        }

        IEnumerator StartCooldown()
        {
            IsReady = false;
            yield return new WaitForSeconds(cooldown);
            IsReady = true;
        }
    }
}