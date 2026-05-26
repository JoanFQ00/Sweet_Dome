using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> cityWayPoints;
    int indexZone = 0;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetPosition;

    void Start()
    {
        indexZone = 0;
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    public void GoRight()
    {
        if (cityWayPoints.Count == 0) return;
        indexZone = (indexZone + 1) % cityWayPoints.Count;

        targetPosition = cityWayPoints[indexZone].localPosition;
    }

    public void GoLeft()
    {
        if (cityWayPoints.Count == 0) return;
        indexZone = (indexZone - 1 + cityWayPoints.Count) % cityWayPoints.Count;

        targetPosition = cityWayPoints[indexZone].localPosition;
    }
}