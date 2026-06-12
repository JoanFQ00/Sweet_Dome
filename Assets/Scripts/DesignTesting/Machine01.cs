using System;
using UnityEngine;

public class Machine01 : MonoBehaviour, IResetable
{
    [Header("Pump Settings")]
    [SerializeField] private LiquidPump purifiedPump;
    [SerializeField] private LiquidPump rawPump;
    [SerializeField] private bool isPurifiedMode;

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
    // ACCIONES DE LA MÁQUINA (Lógica Limpia sin ifs de días)
    // =================================================================

    public void SwapPump()
    {
        // ELIMINADO: El control del Día 1 ya lo hace el ChangeZoneButtonValidator
        isPurifiedMode = !isPurifiedMode;

        // Nota: Si ya tienes sonido de click en el MainButt y este pumpSwap suena igual, 
        // podrías borrar esta línea de audio para que no se duplique.
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