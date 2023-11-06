using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PositionLerper : MonoBehaviour
{
    [SerializeField] public List<Vector3> localPositions;
    [SerializeField] private Transform target;

    [SerializeField] private float moveSpeed;

    public void UpdatePosition(int newPositionIndex)
    {
        StopAllCoroutines();
        StartCoroutine(MoveTo(newPositionIndex));
    }

    IEnumerator MoveTo(int newPositionIndex)
    {
        var currPosition = target.localPosition;
        var targetPosition = localPositions[newPositionIndex];
        var distance = Vector3.Distance(currPosition, targetPosition);
        while (distance > .01f)
        {
            currPosition = Vector3.MoveTowards(currPosition, targetPosition, moveSpeed * Time.deltaTime);
            target.localPosition = currPosition;
            distance = Vector3.Distance(currPosition, targetPosition);
            yield return null;
        }
    }
}