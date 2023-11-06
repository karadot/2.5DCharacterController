using System;
using UnityEngine;

namespace Scipts.States
{
    public class GroundedState : MonoBehaviour, IState, IInputListener
    {
        public bool canTransitionItSelf { get; set; }
        private CharacterController _characterController;
        private DefaultControls _controls;

        [SerializeField] private float speed = 5f;
        [SerializeField] private float runMultiMax = 1.5f;
        private float runMultidefault = 1;
        private float currMulti = 1;
        private bool _isSprint = false;
        [SerializeField] private Transform visualHolder;

        private Animator _animator;

        [SerializeField] private ParticleSystem[] groundedParticles;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
        }

        public void SetControl(DefaultControls controls)
        {
            _controls = controls;
            _controls.Movement.Sprint.started += context => _isSprint = true;
            _controls.Movement.Sprint.canceled += context => _isSprint = false;
        }

        public void Tick()
        {
            var h = _controls.Movement.Horizontal.ReadValue<float>();
            var dir = Vector3.right * h;
            currMulti = Mathf.Lerp(currMulti, _isSprint ? runMultiMax : runMultidefault,
                Time.deltaTime * 10);

            _characterController.Move(dir * (speed * currMulti * Time.deltaTime));
            if (Mathf.Abs(h) > .1f)
            {
                visualHolder.forward = dir.normalized;
            }

            _animator.SetFloat("Speed", Mathf.Abs(h * currMulti));
        }

        public void OnEnter()
        {
            ToggleParticles(true);
        }

        public void OnExit()
        {
            ToggleParticles(false);
        }

        void ToggleParticles(bool active)
        {
            for (int i = 0; i < groundedParticles.Length; i++)
            {
                if (active)
                    groundedParticles[i].Play();
                else
                    groundedParticles[i].Stop();
            }
        }
    }

    public interface IInputListener
    {
        public void SetControl(DefaultControls controls);
    }
}