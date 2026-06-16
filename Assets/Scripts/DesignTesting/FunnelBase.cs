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
        if (currentCapacity >= maxCapacity)
        {
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.funnelFull, gameObject);
        }

        float added = Mathf.Min(amount, spaceLeft);

        UpdateCapacity(added);

        UpdateLiquidVisual();
        if (currentCapacity >= maxCapacity)
        {
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.funnelFull, gameObject);
        }

        return added;
    }

    public virtual float RemoveLiquid(float amount)
    {
        float removable = Mathf.Min(amount, currentCapacity);
        if (currentCapacity <= minCapacity)
        {
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.funnelEmpty, gameObject);
        }

        UpdateCapacity(-removable);

        UpdateLiquidVisual();

        if (currentCapacity <= minCapacity)
        {
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.funnelEmpty, gameObject);
        }

        return removable;
    }

    private void UpdateLiquidVisual()
    {
        if (liquidRenderer == null)
            return;

        if (liquidMat == null)
            liquidMat = liquidRenderer.material;

        float normalized = Mathf.Clamp01(currentCapacity / maxCapacity);
        float minShaderValue = -0.4f;
        float shaderValue = Mathf.Lerp(minShaderValue, 1f, normalized);

        liquidMat.SetFloat(fillPropertyName,shaderValue);
    }

    protected override void OnCapacityChanged()
    {
        UpdateLiquidVisual();
    }
}