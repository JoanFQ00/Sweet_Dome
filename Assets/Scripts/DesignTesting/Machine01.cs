using UnityEngine;

public class Machine01 : MonoBehaviour
{
    [Header("Pump Settings")]
    [SerializeField] private LiquidPump purifiedPump;
    [SerializeField] private LiquidPump rawPump;
    [SerializeField] private bool isPurifiedMode;

    public void SwapPump()
    {
        isPurifiedMode = !isPurifiedMode;
    }

    public void MovePumpToRight()
    {
        if (isPurifiedMode) purifiedPump.GoRight();
        else rawPump.GoRight();
    }

    public void MovePumpToLeft()
    {
        if (isPurifiedMode) purifiedPump.GoLeft();
        else rawPump.GoLeft();
    }

    public LiquidPump GetActivePump()
    {
        return isPurifiedMode ? purifiedPump : rawPump;
    }
}