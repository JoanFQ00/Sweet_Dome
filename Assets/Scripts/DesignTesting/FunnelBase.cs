using UnityEngine;
using System.Collections.Generic;

public class FunnelBase : CapabilityBase
{
    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    [Header("Liquid Shader")]
    [SerializeField] private Renderer liquidRenderer;

    [SerializeField]
    private string fillPropertyName = "_FillAmount";

    private Material liquidMat;

    protected override void Start()
    {
        base.Start();

        if (liquidRenderer != null)
        {
            liquidMat = liquidRenderer.material;
        }

        UpdateLiquidVisual();
    }

    protected override float PutInitialCapacity(Day day)
    {
        return day switch
        {
            Day.Day1 => initialValues[0],
            Day.Day2 => initialValues[1],
            Day.Day3 => initialValues[2],
            Day.Day4 => initialValues[3],
            _ => initialValues[4]
        };
    }

    protected override void OnStartDayFinished()
    {
        UpdateLiquidVisual();
    }

    public virtual float AddLiquid(float amount)
    {
        float spaceLeft = maxCapacity - currentCapacity;

        float added = Mathf.Min(amount, spaceLeft);

        UpdateCapacity(added);

        UpdateLiquidVisual();

        return added;
    }

    public virtual float RemoveLiquid(float amount)
    {
        float removable = Mathf.Min(amount, currentCapacity);

        UpdateCapacity(-removable);

        UpdateLiquidVisual();

        return removable;
    }

    private void UpdateLiquidVisual()
    {
        if (liquidRenderer == null)
            return;

        if (liquidMat == null)
            liquidMat = liquidRenderer.material;

        // NORMALIZE 0-maxCapacity -> 0-1
        float normalized =
            Mathf.Clamp01(currentCapacity / maxCapacity);

        // CONVERT 0-1 -> -1 to 1
        float shaderValue =
            Mathf.Lerp(-1f, 1f, normalized);

        liquidMat.SetFloat(
            fillPropertyName,
            shaderValue
        );
    }

    protected override void OnCapacityChanged()
    {
        UpdateLiquidVisual();
    }
}