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
    private AudioSource activeReceiveLiquidSource;

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

    // Método normal (Usado por la palanca, no reproduce sonido de minijuego)
    public virtual float AddLiquid(float amount)
    {
        float startCapacity = currentCapacity;
        float spaceLeft = maxCapacity - currentCapacity;
        float added = Mathf.Min(amount, spaceLeft);

        UpdateCapacity(added);

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
            float duration = added / 10f;
            if (visualLerpCoroutine != null) StopCoroutine(visualLerpCoroutine);
            visualLerpCoroutine = StartCoroutine(AnimateLiquidVolume(startCapacity, currentCapacity, duration));
        }

        return added;
    }

    public void AddLiquidFromMinigame(float amount)
    {
        // Ejecutamos la adición matemática y visual normal
        float added = AddLiquid(amount);

        // Si realmente entró líquido, lanzamos el sonido tridimensional en este objeto
        if (added > 0f && SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.recieveLiquidSFX != null)
        {
            if (activeReceiveLiquidSource != null)
            {
                Destroy(activeReceiveLiquidSource.gameObject);
            }

            activeReceiveLiquidSource = SoundManagerOwn.Instance.Create3DAudioSource(
                SoundManagerOwn.Instance.recieveLiquidSFX,
                transform.position,
                false
            );

            if (activeReceiveLiquidSource != null)
            {
                activeReceiveLiquidSource.Play();
                Destroy(activeReceiveLiquidSource.gameObject, SoundManagerOwn.Instance.recieveLiquidSFX.length);
            }
        }
    }

    // Método exclusivo para cuando se vacía el líquido al fallar un minijuego
    public void RemoveLiquidFromMinigameFail(float amount)
    {
        // Ejecutamos el vaciado matemático y visual normal
        float removed = RemoveLiquid(amount);

        // Si realmente se eliminó líquido, lanzamos el sonido tridimensional de pérdida en este objeto
        if (removed > 0f && SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.losedLiquidSFX != null)
        {
            // Reutilizamos la variable de audio controlando que no se solape
            if (activeReceiveLiquidSource != null)
            {
                Destroy(activeReceiveLiquidSource.gameObject);
            }

            activeReceiveLiquidSource = SoundManagerOwn.Instance.Create3DAudioSource(
                SoundManagerOwn.Instance.losedLiquidSFX,
                transform.position,
                false
            );

            if (activeReceiveLiquidSource != null)
            {
                activeReceiveLiquidSource.Play();
                Destroy(activeReceiveLiquidSource.gameObject, SoundManagerOwn.Instance.losedLiquidSFX.length);
            }
        }
    }

    public virtual float RemoveLiquid(float amount)
    {
        float startCapacity = currentCapacity;
        float removable = Mathf.Min(amount, currentCapacity);

        UpdateCapacity(-removable);

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