using System.Collections.Generic;
using UnityEngine;

public class FrameCityMachine : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private List<Transform> cityWayPoints;

    private int indexZone = 0;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private Vector3 targetPosition;

    void Start()
    {
        indexZone = 0;

        if (cityWayPoints.Count > 0)
            targetPosition = cityWayPoints[0].localPosition;
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    // -------------------------
    // CORE: zonas desbloqueadas
    // -------------------------
    private int GetUnlockedCount()
    {
        return (int)GameManager.Instance.currentDay + 1;
    }

    // -------------------------
    // INPUT
    // -------------------------
    public void GoRight()
    {
        int max = Mathf.Min(GetUnlockedCount(), cityWayPoints.Count);
        if (max <= 0) return;

        indexZone = (indexZone + 1) % max;
        ApplyTarget(max);
    }

    public void GoLeft()
    {
        int max = Mathf.Min(GetUnlockedCount(), cityWayPoints.Count);
        if (max <= 0) return;

        indexZone = (indexZone - 1 + max) % max;
        ApplyTarget(max);
    }

    // -------------------------
    // APPLY POSITION
    // -------------------------
    private void ApplyTarget(int max)
    {
        indexZone = Mathf.Clamp(indexZone, 0, max - 1);
        targetPosition = cityWayPoints[indexZone].localPosition;
    }
}