using System;
using UnityEngine;

namespace Scipts.Interaction
{
    public class InteractableKey : InteractableBase
    {
        public bool IsCollected { get; set; }

        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public override void Interact(Interactor interactor)
        {
            _collider.enabled = false;
            IsCollected = true;
            transform.parent = interactor.transform;
        }

        public void ReleaseKey()
        {
            transform.parent = null;
            gameObject.SetActive(false);
        }
    }
}