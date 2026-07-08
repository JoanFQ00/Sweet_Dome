using UnityEngine;
using System.Collections.Generic;

public class FunnelBase : CapabilityBase
{
    public enum FunnelType
    {
        Small,
        Big
    }

    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    [Header("Funnel Configuration")]
    [SerializeField] private FunnelType funnelType = FunnelType.Small;

    [Header("Lever Interactivity")]
    [SerializeField] private Lever leverSource;

    [Header("Liquid Shader")]
    [SerializeField] private Renderer liquidRenderer;

    [SerializeField] private string fillPropertyName = "_FillAmount";
    [SerializeField] private string waveSpeedPropertyName = "_WaveSpeed";

    private Material liquidMat;

    protected override void Start()
    {
        base.Start();

        if (liquidRenderer != null)
        {
            liquidMat = liquidRenderer.material;
            if (liquidMat.HasProperty(waveSpeedPropertyName))
            {
                liquidMat.SetFloat(waveSpeedPropertyName, 1.3f);
            }
        }

        UpdateLiquidVisual();
    }

    public void SetWaveSpeed(float speed)
    {
        if (liquidMat == null && liquidRenderer != null)
        {
            liquidMat = liquidRenderer.material;
        }

        if (liquidMat != null && liquidMat.HasProperty(waveSpeedPropertyName))
        {
            liquidMat.SetFloat(waveSpeedPropertyName, speed);
        }
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

        float normalized = 0f;

        if (funnelType == FunnelType.Big)
        {
            float offsetValue = Mathf.Lerp(15f, maxCapacity, currentCapacity / maxCapacity);
            normalized = Mathf.Clamp01(offsetValue / maxCapacity);
        }
        else
        {
            normalized = Mathf.Clamp01(currentCapacity / maxCapacity);
        }

        float minShaderValue = -0.4f;
        float shaderValue = Mathf.Lerp(minShaderValue, 1f, normalized);

        liquidMat.SetFloat(fillPropertyName, shaderValue);
    }

    protected override void OnCapacityChanged()
    {
        UpdateLiquidVisual();
    }
}