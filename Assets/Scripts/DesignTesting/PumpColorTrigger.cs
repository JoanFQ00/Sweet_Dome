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

    private void HandlePumpSwapped(LiquidPump activePump)
    {
        if (targetButton == null || machineSource == null) return;

        if (targetButton.Validator != null && !targetButton.Validator.CanPress())
        {
            targetButton.OverrideEmissionColor(Color.black);
            return;
        }

        Color selectedColor = machineSource.IsPurifiedMode ? purifiedHdrColor : rawHdrColor;

        targetButton.OverrideEmissionColor(selectedColor);
    }

    protected override void Start()
    {
        base.Start();

        if (machineSource != null)
        {
            HandlePumpSwapped(null);
        }
    }

    public override void ResetValues()
    {
        base.ResetValues();
        if (machineSource != null)
        {
            HandlePumpSwapped(null);
        }
    }
}