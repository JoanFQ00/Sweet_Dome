using UnityEngine;

public class CityPriorityManager : MonoBehaviour, IResetable
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

    private FunnelBase prioritizedZone;
    private bool priorityUsedToday = false;

    // VARIABLE BLINDADA: Guarda el índice fijo de la zona priorizada
    private int prioritizedZoneIndex = -1;

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
        prioritizedZoneIndex = -1; // Reseteamos el índice blindado para el nuevo día

        OnPriorityResetVisual();

        Debug.Log($"<color=#00FFCC><b>[PRIORITY MANAGER]</b></color> Botón de prioridad restablecido para el nuevo día.");
    }

    protected virtual void OnPriorityResetVisual() { }

    // =================================================================
    // ACCIÓN DEL BOTÓN (Lógica Limpia y Directa)
    // =================================================================

    public void PrioritizeCurrentZone()
    {
        if (cityFunnel == null) return;

        FunnelBase currentZone = cityFunnel.GetCurrentZone();
        if (currentZone == null) return;

        prioritizedZone = currentZone;
        priorityUsedToday = true;

        // CONGELAMOS EL ÍNDICE AQUÍ: Guardamos el índice exacto que tiene la ciudad en este frame
        prioritizedZoneIndex = cityFunnel.GetIdxCurrentZone();

        Debug.Log($"Prioritized zone: {prioritizedZone.name} with fixed Index: {prioritizedZoneIndex}");
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

    /// <summary>
    /// Devuelve el índice REAL y CONGELADO que se guardó al pulsar el botón de prioridad.
    /// Devuelve -1 si aún no se ha usado la prioridad hoy.
    /// </summary>
    public int GetIdxPrioritizedZone()
    {
        return prioritizedZoneIndex;
    }

    public bool IsZonePrioritized(FunnelBase zone)
    {
        return prioritizedZone == zone;
    }
}