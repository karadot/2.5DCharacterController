using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Scipts.Interaction
{
    public class LeverInteractable : InteractableBase
    {
        [SerializeField] private Animator _animator;

        [FormerlySerializedAs("_currentState")] [SerializeField]
        private int currentState;

        [SerializeField] private List<float> blendPoints;

        [SerializeField] private float blendDuration = .2f;

        [SerializeField] private UnityEvent<int> OnStateUpdated;

        private void Awake()
        {
            _animator.SetFloat("LeverPos", blendPoints[currentState]);
            OnStateUpdated?.Invoke(currentState);
        }

        public override void Interact(Interactor _)
        {
            IncreaseState();
        }

        void IncreaseState()
        {
            StartCoroutine(Animate());
            Debug.Log("Animate" + currentState);
        }


        IEnumerator Animate()
        {
            var currentVal = blendPoints[currentState];
            currentState++;
            currentState %= blendPoints.Count;
            OnStateUpdated?.Invoke(currentState);
            var target = blendPoints[currentState];
            var t = 0f;

            while (t <= blendDuration)
            {
                _animator.SetFloat("LeverPos", Mathf.Lerp(currentVal, target, t / blendDuration));
                t += Time.deltaTime;
                yield return null;
            }
        }


        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < OnStateUpdated.GetPersistentEventCount(); i++)
            {
                var mono = (OnStateUpdated.GetPersistentTarget(i) as MonoBehaviour);
                if (!mono) continue;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, mono.transform.position);
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(mono.transform.position, .25f);
            }
        }
    }
}