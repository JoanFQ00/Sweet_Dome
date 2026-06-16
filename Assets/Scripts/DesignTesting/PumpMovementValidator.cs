using UnityEngine;

public class PumpMovementValidator : ButtonValidator
{
    public enum DirectionType
    {
        Left,
        Right
    }

    [Header("Console Referencies")]
    [SerializeField] private Machine01 machineSource;

    [Header("Button Config")]
    [SerializeField] private DirectionType buttonDirection = DirectionType.Left;

    public override bool CanPress()
    {
        if (machineSource == null) return false;

        LiquidPump activePump = machineSource.GetActivePump();
        if (activePump == null) return false;

        int currentIndex = activePump.CurrentFunnelIndex;

        if (buttonDirection == DirectionType.Left)
        {
            if (currentIndex <= 0)
            {
                return false;
            }
        }
        else if (buttonDirection == DirectionType.Right)
        {
            int maxLimit = 1;

            if (currentIndex >= maxLimit)
            {
                return false;
            }
        }
        return true;
    }
}