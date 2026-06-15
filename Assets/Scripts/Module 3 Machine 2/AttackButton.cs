using UnityEngine;

public class AttackButton : ButtonValidator
{
    [Header("References")]
    [SerializeField] private RadarVision radarVision;
    [SerializeField] private FunnelBase rawFunnel;

    public FunnelBase RawFunnel => rawFunnel;

    public override bool CanPress()
    {
        if (radarVision == null || radarVision.CurrentTarget == null)
        {
            return false;
        }
        return true;
    }

    public void ExecuteAttack()
    {
        if (radarVision == null || radarVision.CurrentTarget == null) return;

        GameObject targetToDelete = radarVision.CurrentTarget;

        RadarVision.OnObjectiveGet?.Invoke(targetToDelete);
    }
}