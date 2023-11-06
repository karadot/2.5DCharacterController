using System;
using UnityEngine;

namespace Scipts.Interaction
{
    [RequireComponent(typeof(PositionLerper))]
    public class InteractableGate : InteractableBase
    {
        [SerializeField] private InteractableKey interactableKey;

        private PositionLerper _positionLerper;

        [SerializeField] private GameObject lockObject;

        private void Awake()
        {
            _positionLerper = GetComponent<PositionLerper>();
        }

        public override void Interact(Interactor interactor)
        {
            if (interactableKey.IsCollected)
            {
                interactableKey.ReleaseKey();
                _positionLerper.UpdatePosition(1);
                lockObject.SetActive(false);
            }
        }
    }
}