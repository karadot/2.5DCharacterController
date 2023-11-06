using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    private InteractableBase _lastDetectedInteractable;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && _lastDetectedInteractable)
        {
            _lastDetectedInteractable.Interact(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out InteractableBase detected))
        {
            _lastDetectedInteractable = detected;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_lastDetectedInteractable) return;
        if (other.gameObject == _lastDetectedInteractable.gameObject)
            _lastDetectedInteractable = null;
    }
}