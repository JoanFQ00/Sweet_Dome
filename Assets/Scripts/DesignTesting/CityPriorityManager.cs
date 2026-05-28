using UnityEngine;

public class CityPriorityManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

    private FunnelBase prioritizedZone;
    private bool priorityUsedToday = false;

    private void OnEnable()
    {
        GameManager.NewDay += ResetPriority;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= ResetPriority;
    }

    // -------------------------
    // BUTTON
    // -------------------------
    public void PrioritizeCurrentZone()
    {
        if (priorityUsedToday)
            return;

        FunnelBase currentZone = cityFunnel.GetCurrentZone();

        if (currentZone == null)
            return;

        prioritizedZone = currentZone;
        priorityUsedToday = true;

        Debug.Log($"Prioritized zone: {prioritizedZone.name}");
    }

    // -------------------------
    // RESET EACH DAY
    // -------------------------
    private void ResetPriority()
    {
        prioritizedZone = null;
        priorityUsedToday = false;
    }

    // -------------------------
    // GETTERS
    // -------------------------
    public bool HasUsedPriorityToday()
    {
        return priorityUsedToday;
    }

    public FunnelBase GetPrioritizedZone()
    {
        return prioritizedZone;
    }

    public bool IsZonePrioritized(FunnelBase zone)
    {
        return prioritizedZone == zone;
    }
}