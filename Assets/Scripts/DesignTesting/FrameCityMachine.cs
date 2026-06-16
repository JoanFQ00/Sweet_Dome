using System.Collections.Generic;
using UnityEngine;

public class FrameCityMachine : MonoBehaviour, IResetable
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        ResetValues();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    public void ResetValues()
    {
        indexZone = 0;

        if (cityWayPoints != null && cityWayPoints.Count > 0 && cityWayPoints[0] != null)
        {
            transform.localPosition = cityWayPoints[0].localPosition;
            targetPosition = cityWayPoints[0].localPosition;
        }
        else
        {
            targetPosition = transform.localPosition;
        }
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private int GetUnlockedCount()
    {
        if (GameManager.Instance == null || cityFunnel == null) return 0;

        int currentDay = (int)GameManager.Instance.currentDay;
        int count = 0;

        var zonesData = cityFunnel.GetCityZonesData();
        if (zonesData == null) return 0;

        foreach (var zone in zonesData)
        {
            if (zone != null && (int)zone.unlockDay <= currentDay)
                count++;
        }

        return count;
    }

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

    private void ApplyTarget(int max)
    {
        indexZone = Mathf.Clamp(indexZone, 0, max - 1);

        if (cityWayPoints != null && indexZone < cityWayPoints.Count && cityWayPoints[indexZone] != null)
        {
            targetPosition = cityWayPoints[indexZone].localPosition;
        }
    }
}