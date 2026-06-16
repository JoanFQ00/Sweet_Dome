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
    [SerializeField] private float prioritizedDrainMultiplier = 0.25f;

    [Header("Event: Dome Crack (Grieta)")]
    [SerializeField] private bool isCrackActive = false;
    [SerializeField] private float crackDrainMultiplier = 3f;
    private float crackTimer = 0f;

    [Header("Event: Liquid Lerp (Update Driven)")]
    [SerializeField] private bool isTransitioningLiquid = false;
    private float lerpStartValue;
    private float lerpTargetValue;
    private float lerpDuration;
    private float lerpTimer;

    [Header("Global State")]
    public bool isEnergyFlowPaused = false;

    private void Start()
    {
        if (funnel == null)
            funnel = GetComponent<FunnelBase>();
    }

    private void Update()
    {
        if (isTransitioningLiquid && funnel != null)
        {
            ProcessLiquidTransition();
        }

        if (funnel == null || DayTime.Instance.currentHour < 15)
            return;

        if (isCrackActive)
        {
            crackTimer -= Time.deltaTime;
            if (crackTimer <= 0f)
            {
                isCrackActive = false;
            }
        }

        float finalDrain = drainPerSecond;
        if (priorityManager != null && priorityManager.IsZonePrioritized(funnel))
        {
            finalDrain *= prioritizedDrainMultiplier;
        }

        if (isCrackActive)
        {
            finalDrain *= crackDrainMultiplier;
        }

        funnel.RemoveLiquid(finalDrain * Time.deltaTime);
    }

    public void TriggerCrackEvent(float duration, float severityMultiplier)
    {
        isCrackActive = true;
        crackTimer = duration;
        crackDrainMultiplier = severityMultiplier;
    }

    public void ChangeLiquidDirectly(float targetValue, float duration)
    {
        if (funnel == null) return;

        lerpStartValue = funnel.CurrentCapacity;
        lerpTargetValue = Mathf.Clamp(targetValue, funnel.MinCapacity, funnel.MaxCapacity);
        lerpDuration = duration;
        lerpTimer = 0f;

        if (duration <= 0f)
        {
            float diff = lerpTargetValue - funnel.CurrentCapacity;
            ApplyLiquidDifference(diff);
            isTransitioningLiquid = false;
        }
        else
        {
            isTransitioningLiquid = true;
        }
    }

    private void ProcessLiquidTransition()
    {
        lerpTimer += Time.deltaTime;
        float t = lerpTimer / lerpDuration;

        t = t * t * (3f - 2f * t);

        float nextValue = Mathf.Lerp(lerpStartValue, lerpTargetValue, t);

        float difference = nextValue - funnel.CurrentCapacity;
        ApplyLiquidDifference(difference);

        if (lerpTimer >= lerpDuration)
        {
            isTransitioningLiquid = false;
            float finalDiff = lerpTargetValue - funnel.CurrentCapacity;
            ApplyLiquidDifference(finalDiff);
        }
    }

    private void ApplyLiquidDifference(float difference)
    {
        if (difference > 0f)
        {
            funnel.AddLiquid(difference);
        }
        else if (difference < 0f)
        {
            funnel.RemoveLiquid(Mathf.Abs(difference));
        }
    }

    public float GetModifiedAmount(float amount)
    {
        if (priorityManager != null && priorityManager.IsZonePrioritized(funnel))
        {
            return amount * prioritizedLiquidMultiplier;
        }

        return amount;
    }
}