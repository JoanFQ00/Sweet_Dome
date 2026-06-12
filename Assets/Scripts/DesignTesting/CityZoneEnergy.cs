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
    public bool isEnergyFlowPaused = false; // Interruptor individual controlado por la central

    private void Start()
    {
        if (funnel == null)
            funnel = GetComponent<FunnelBase>();
    }

    private void Update()
    {
        // 1. Procesar la transición suave de líquido si está activa (Independiente de la hora del juego)
        if (isTransitioningLiquid && funnel != null)
        {
            ProcessLiquidTransition();
        }

        // Restricción horaria de tu juego (15:00 a 21:00) para el drenaje pasivo y grietas
        if (funnel == null || DayTime.Instance.currentHour < 15)
            return;

        // 2. Manejo del temporizador de la grieta
        if (isCrackActive)
        {
            crackTimer -= Time.deltaTime;
            if (crackTimer <= 0f)
            {
                isCrackActive = false;
                Debug.Log($"[{gameObject.name}] La grieta de la cúpula ha sido reparada.");
            }
        }

        // 3. Calcular el drenaje de energía por segundo
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

    // -----------------------------------------------------------------
    // EFECTO 1: Disparar el evento de la Grieta
    // -----------------------------------------------------------------
    public void TriggerCrackEvent(float duration, float severityMultiplier)
    {
        isCrackActive = true;
        crackTimer = duration;
        crackDrainMultiplier = severityMultiplier;

        Debug.Log($"<color=red>[EVENTO GRIETA]</color> Activado en {gameObject.name} por {duration}s con severidad x{severityMultiplier}");
    }

    // -----------------------------------------------------------------
    // EFECTO 2: Cambiar el líquido a un valor fijo (Controlado por Update)
    // -----------------------------------------------------------------
    /// <summary>
    /// Inicia una transición suave hacia un valor de líquido específico sin usar corrutinas.
    /// </summary>
    public void ChangeLiquidDirectly(float targetValue, float duration)
    {
        if (funnel == null) return;

        // Configurar variables para que el Update tome el control
        lerpStartValue = funnel.CurrentCapacity;
        lerpTargetValue = Mathf.Clamp(targetValue, funnel.MinCapacity, funnel.MaxCapacity);
        lerpDuration = duration;
        lerpTimer = 0f;

        // Si la duración es 0 o menor, aplicamos el cambio instantáneo para evitar divisiones por cero
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

    /// <summary>
    /// Se ejecuta cada frame dentro de Update() mientras isTransitioningLiquid sea true.
    /// </summary>
    private void ProcessLiquidTransition()
    {
        lerpTimer += Time.deltaTime;
        float t = lerpTimer / lerpDuration;

        // Curva suave tipo SmoothStep (óptima y matemática, sin costes extras)
        t = t * t * (3f - 2f * t);

        // Calculamos el valor interpolado que debería tener en este frame
        float nextValue = Mathf.Lerp(lerpStartValue, lerpTargetValue, t);

        // Calculamos la diferencia respecto a lo que tiene el funnel ahora mismo
        float difference = nextValue - funnel.CurrentCapacity;
        ApplyLiquidDifference(difference);

        // Si el temporizador llega al final, terminamos la transición
        if (lerpTimer >= lerpDuration)
        {
            isTransitioningLiquid = false;

            // Ajuste final exacto por si acaso
            float finalDiff = lerpTargetValue - funnel.CurrentCapacity;
            ApplyLiquidDifference(finalDiff);
        }
    }

    /// <summary>
    /// Helper interno para interactuar con los métodos del FunnelBase de forma limpia
    /// </summary>
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

    // BONUS WHEN RECEIVING
    public float GetModifiedAmount(float amount)
    {
        if (priorityManager != null && priorityManager.IsZonePrioritized(funnel))
        {
            return amount * prioritizedLiquidMultiplier;
        }

        return amount;
    }
}