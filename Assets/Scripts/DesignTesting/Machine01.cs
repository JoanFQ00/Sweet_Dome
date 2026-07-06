using System;
using UnityEngine;

public class Machine01 : MonoBehaviour, IResetable
{
    [Header("Pump Settings")]
    [SerializeField] private LiquidPump purifiedPump;
    [SerializeField] private LiquidPump rawPump;
    [SerializeField] private bool isPurifiedMode;

    [Header("Animations (Animator)")]
    [SerializeField] private Animator purifiedAnimator;
    [SerializeField] private Animator rawAnimator;
    [SerializeField] private string triggerRight = "TurnRight";
    [SerializeField] private string triggerLeft = "TurnLeft";

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

    public void SwapPump()
    {
        isPurifiedMode = !isPurifiedMode;

        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.pumpSwap, gameObject);

        OnPumpSwapped?.Invoke(GetActivePump());
    }

    public void MovePumpToRight()
    {
        if (isPurifiedMode)
        {
            purifiedPump.ShiftRight();
            purifiedAnimator.SetTrigger(triggerRight);
        }
        else
        {
            rawPump.ShiftRight();
            rawAnimator.SetTrigger(triggerRight);
        }


        OnPumpSwapped?.Invoke(GetActivePump());
    }

    public void MovePumpToLeft()
    {
        if (isPurifiedMode)
        {
            purifiedPump.ShiftLeft();
            purifiedAnimator.SetTrigger(triggerLeft);
        }
        else
        {
            rawPump.ShiftLeft();
            rawAnimator.SetTrigger(triggerLeft);
        }

        OnPumpSwapped?.Invoke(GetActivePump());
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