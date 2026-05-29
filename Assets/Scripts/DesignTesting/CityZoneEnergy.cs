using UnityEngine;

public class CityZoneEnergy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FunnelBase funnel;
    [SerializeField] private CityPriorityManager priorityManager;

    [Header("Drain")]
    [SerializeField] private float drainPerSecond = 1f;

    [Header("Priority")]
    [SerializeField] private float prioritizedLiquidMultiplier = 2f;

    [SerializeField]
    private float prioritizedDrainMultiplier = 0.25f;

    private void Start()
    {
        funnel = GetComponent<FunnelBase>();
    }

    private void Update()
    {
        if (funnel == null || DayTime.Instance.currentHour < 15)
            return;

        float finalDrain = drainPerSecond;

        // -------------------------
        // PRIORITY REDUCES DRAIN
        // -------------------------
        if (priorityManager != null &&
            priorityManager.IsZonePrioritized(funnel))
        {
            finalDrain *= prioritizedDrainMultiplier;
        }

        funnel.RemoveLiquid(finalDrain * Time.deltaTime);
    }

    // -------------------------
    // BONUS WHEN RECEIVING
    // -------------------------
    public float GetModifiedAmount(float amount)
    {
        if (priorityManager != null &&
            priorityManager.IsZonePrioritized(funnel))
        {
            return amount * prioritizedLiquidMultiplier;
        }

        return amount;
    }
}