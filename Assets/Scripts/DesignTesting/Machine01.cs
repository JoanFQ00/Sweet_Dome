using System;
using UnityEngine;

public class Machine01 : MonoBehaviour
{
    [Header("Pump Settings")]
    [SerializeField] private LiquidPump purifiedPump;
    [SerializeField] private LiquidPump rawPump;
    [SerializeField] private bool isPurifiedMode;

    public Action<LiquidPump> OnPumpSwapped;

    // Cambiado a Start para garantizar que las TVs y UIs ya estén en OnEnable escuchando
    private void Start()
    {
        InvokeInitialSwap();
    }

    public void SwapPump()
    {
        if (GameManager.Instance.currentDay != Day.Day1)
        {
            isPurifiedMode = !isPurifiedMode;
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.pumpSwap, gameObject);

            OnPumpSwapped?.Invoke(GetActivePump());
        }
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

    private void InvokeInitialSwap()
    {
        if (purifiedPump != null && rawPump != null)
        {
            OnPumpSwapped?.Invoke(GetActivePump());
        }
    }
}