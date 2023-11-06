using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scipts.States
{
    public class DashState : MonoBehaviour, IState
    {
        public bool canTransitionItSelf { get; set; } = false;

        [SerializeField] private float duration, speed;
        [SerializeField] private Transform visualHolder;
        private float timer = 0f;
        private CharacterController _characterController;

        [SerializeField] private float cooldown = 3f;
        public bool IsReady { get; private set; } = true;
        public bool IsCompleted => duration <= timer;
        private Animator _animator;

        [SerializeField] private ParticleSystem dashParticle;
        [SerializeField] private TrailRenderer dashTrail;

        private void Awake()
        {
            canTransitionItSelf = false;
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void Tick()
        {
            if (IsCompleted) return;
            var delta = Time.deltaTime;
            _characterController.Move(visualHolder.forward * (speed * delta));
            timer += delta;
        }

        private int _lastAnimStateHash;

        public void OnEnter()
        {
            IsReady = false;
            timer = 0;
            _lastAnimStateHash = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            _animator.CrossFade("Dash", .1f);
            dashParticle.Play();
            dashTrail.emitting = true;
        }

        public void OnExit()
        {
            dashTrail.emitting = false;
            timer = 0;
            _animator.CrossFade(_lastAnimStateHash, .1f);
            StartCoroutine(StartCooldown());
        }

        IEnumerator StartCooldown()
        {
            IsReady = false;
            yield return new WaitForSeconds(cooldown);
            IsReady = true;
        }
    }
}