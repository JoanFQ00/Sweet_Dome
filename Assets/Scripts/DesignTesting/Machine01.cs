using System;
using UnityEngine;

public class Machine01 : MonoBehaviour, IResetable
{
    [Header("Pump Settings")]
    [SerializeField] private LiquidPump purifiedPump;
    [SerializeField] private LiquidPump rawPump;
    [SerializeField] private bool isPurifiedMode;

    // PROPIEDAD PÚBLICA: Permite a otros scripts leer el estado actual de forma limpia
    public bool IsPurifiedMode => isPurifiedMode;

    public Action<LiquidPump> OnPumpSwapped;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        InvokeInitialSwap();
    }

    public void ResetValues()
    {
        isPurifiedMode = true;
        InvokeInitialSwap();
    }

    // =================================================================
    // ACCIONES DE LA MÁQUINA
    // =================================================================

    public void SwapPump()
    {
        isPurifiedMode = !isPurifiedMode;

        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.pumpSwap, gameObject);

        OnPumpSwapped?.Invoke(GetActivePump());
    }

    public void MovePumpToRight()
    {
        if (isPurifiedMode) purifiedPump.ShiftRight();
        else rawPump.ShiftRight();
    }

    public void MovePumpToLeft()
    {
        if (isPurifiedMode) purifiedPump.ShiftLeft();
        else rawPump.ShiftLeft();
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