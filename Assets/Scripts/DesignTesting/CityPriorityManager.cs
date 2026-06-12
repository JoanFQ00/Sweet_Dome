using UnityEngine;

public class CityPriorityManager : MonoBehaviour, IResetable
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

    private FunnelBase prioritizedZone;
    private bool priorityUsedToday = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    // =================================================================
    // IMPLEMENTACIÓN DE INTERFAZ (RESET DE PRIORIDAD DIARIA)
    // =================================================================

    public void ResetValues()
    {
        prioritizedZone = null;
        priorityUsedToday = false;

        OnPriorityResetVisual();

        Debug.Log($"<color=#00FFCC><b>[PRIORITY MANAGER]</b></color> Botón de prioridad restablecido para el nuevo día.");
    }

    protected virtual void OnPriorityResetVisual() { }

    // =================================================================
    // ACCIÓN DEL BOTÓN (Lógica Limpia y Directa)
    // =================================================================

    public void PrioritizeCurrentZone()
    {
        // ELIMINADO: Los 'if' de control de clics repetidos ya no hacen falta aquí.
        // El validador del MainButt se encarga de filtrar las pulsaciones falsas.

        if (cityFunnel == null) return;

        FunnelBase currentZone = cityFunnel.GetCurrentZone();
        if (currentZone == null) return;

        prioritizedZone = currentZone;
        priorityUsedToday = true;

        Debug.Log($"Prioritized zone: {prioritizedZone.name}");
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