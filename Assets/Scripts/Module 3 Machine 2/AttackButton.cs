using UnityEngine;

public class AttackButton : ButtonValidator
{
    [Header("Referencies")]
    [SerializeField] private RadarVision radarVision;
    [SerializeField] private FunnelBase rawFunnel;

    // Propiedad pública limpia para que el gestor de eventos acceda al embudo
    public FunnelBase RawFunnel => rawFunnel;

    public override bool CanPress()
    {
        // El botón solo se puede hundir si el radar ha enganchado una nave o meteorito válido en su trigger
        if (radarVision == null || radarVision.CurrentTarget == null)
        {
            return false;
        }
        return true;
    }

    public void ExecuteAttack()
    {
        if (radarVision == null || radarVision.CurrentTarget == null) return;

        // Capturamos el objetivo que está colisionando actualmente en la mira
        GameObject targetToDelete = radarVision.CurrentTarget;

        // Mandamos la orden al RadarMinigame para que procese la baja del objeto
        RadarVision.OnObjectiveGet?.Invoke(targetToDelete);
    }
}