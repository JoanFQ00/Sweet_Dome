using UnityEngine;

public class PumpColorTrigger : CustomButtonColorTrigger
{
    [Header("Machine Subscription References")]
    [SerializeField] private Machine01 machineSource;

    [Header("Bicolor Inteligence Config")]
    [ColorUsage(true, true)]
    [SerializeField] private Color purifiedHdrColor = Color.cyan;

    [ColorUsage(true, true)]
    [SerializeField] private Color rawHdrColor = Color.red;

    private void OnEnable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped += HandlePumpSwapped;
        }
    }

    private void OnDisable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped -= HandlePumpSwapped;
        }
    }
    private void Update()
    {
        RefreshButtonVisualState();
    }

    private void HandlePumpSwapped(LiquidPump activePump)
    {
        RefreshButtonVisualState();
    }

    private void RefreshButtonVisualState()
    {
        if (targetButton == null) return;

        if (targetButton.Validator != null && targetButton.Validator.CanPress())
        {
            if (machineSource != null)
            {
                Color selectedColor = machineSource.IsPurifiedMode ? purifiedHdrColor : rawHdrColor;
                targetButton.OverrideEmissionColor(selectedColor);
            }
        }
        else
        {
            targetButton.OverrideEmissionColor(Color.black);
        }
    }

    protected override void Start()
    {
        base.Start();
        RefreshButtonVisualState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
        RefreshButtonVisualState();
    }
}