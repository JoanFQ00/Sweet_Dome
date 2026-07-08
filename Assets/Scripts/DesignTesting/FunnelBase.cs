using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    private Coroutine visualLerpCoroutine;

    // Propiedad pública que lee CapabilityUI en su Update
    public float CurrentVisualCapacity { get; private set; }

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

        CurrentVisualCapacity = currentCapacity;
        UpdateLiquidVisual(currentCapacity);
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
        CurrentVisualCapacity = currentCapacity;
        UpdateLiquidVisual(currentCapacity);
    }

    public virtual float AddLiquid(float amount)
    {
        float startCapacity = currentCapacity;
        float spaceLeft = maxCapacity - currentCapacity;
        float added = Mathf.Min(amount, spaceLeft);

        UpdateCapacity(added);

        // Si la palanca está activa (IsAbsorbing o IsExpelling), el flujo es frame a frame.
        // Sincronizamos el valor visual directamente con el real para evitar retrasos y desincronizaciones.
        if (leverSource != null && (leverSource.IsAbsorbing || leverSource.IsExpelling))
        {
            if (visualLerpCoroutine != null)
            {
                StopCoroutine(visualLerpCoroutine);
                visualLerpCoroutine = null;
            }
            CurrentVisualCapacity = currentCapacity;
            UpdateLiquidVisual(currentCapacity);
        }
        else
        {
            // Si el cambio viene de un script externo de golpe (como el puzzle de la barra), 
            // calculamos la duración exacta simulando la velocidad de la bomba (10f) e iniciamos la animación.
            float duration = added / 10f;
            if (visualLerpCoroutine != null) StopCoroutine(visualLerpCoroutine);
            visualLerpCoroutine = StartCoroutine(AnimateLiquidVolume(startCapacity, currentCapacity, duration));
        }

        return added;
    }

    public virtual float RemoveLiquid(float amount)
    {
        float startCapacity = currentCapacity;
        float removable = Mathf.Min(amount, currentCapacity);

        UpdateCapacity(-removable);

        // Si la palanca está activa, seguimos el flujo en tiempo real de forma instantánea y síncrona.
        if (leverSource != null && (leverSource.IsAbsorbing || leverSource.IsExpelling))
        {
            if (visualLerpCoroutine != null)
            {
                StopCoroutine(visualLerpCoroutine);
                visualLerpCoroutine = null;
            }
            CurrentVisualCapacity = currentCapacity;
            UpdateLiquidVisual(currentCapacity);
        }
        else
        {
            // Si viene del minijuego de la barra, forzamos el vaciado a velocidad de simulación de bomba.
            float duration = removable / 10f;
            if (visualLerpCoroutine != null) StopCoroutine(visualLerpCoroutine);
            visualLerpCoroutine = StartCoroutine(AnimateLiquidVolume(startCapacity, currentCapacity, duration));
        }

        return removable;
    }

    private IEnumerator AnimateLiquidVolume(float fromCapacity, float toCapacity, float duration)
    {
        if (duration <= 0f)
        {
            CurrentVisualCapacity = toCapacity;
            UpdateLiquidVisual(toCapacity);
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            CurrentVisualCapacity = Mathf.Lerp(fromCapacity, toCapacity, elapsedTime / duration);
            UpdateLiquidVisual(CurrentVisualCapacity);
            yield return null;
        }

        CurrentVisualCapacity = toCapacity;
        UpdateLiquidVisual(toCapacity);
        visualLerpCoroutine = null;
    }

    private void UpdateLiquidVisual(float capacityValue)
    {
        if (liquidRenderer == null)
            return;

        if (liquidMat == null)
            liquidMat = liquidRenderer.material;

        float normalized = 0f;

        if (funnelType == FunnelType.Big)
        {
            float offsetValue = Mathf.Lerp(15f, maxCapacity, capacityValue / maxCapacity);
            normalized = Mathf.Clamp01(offsetValue / maxCapacity);
        }
        else
        {
            normalized = Mathf.Clamp01(capacityValue / maxCapacity);
        }

        float minShaderValue = -0.4f;
        float shaderValue = Mathf.Lerp(minShaderValue, 1f, normalized);

        liquidMat.SetFloat(fillPropertyName, shaderValue);
    }

    protected override void OnCapacityChanged()
    {
        if (visualLerpCoroutine == null)
        {
            CurrentVisualCapacity = currentCapacity;
            UpdateLiquidVisual(currentCapacity);
        }
    }
}