using System.Collections.Generic;
using UnityEngine;

public class FrameCityMachine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

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
    // CORE
    // -------------------------
    private int GetUnlockedCount()
    {
        int currentDay = (int)GameManager.Instance.currentDay;

        int count = 0;

        foreach (var zone in cityFunnel.GetCityZonesData())
        {
            if ((int)zone.unlockDay <= currentDay)
                count++;
        }

        return count;
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