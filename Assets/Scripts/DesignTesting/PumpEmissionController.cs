using UnityEngine;
using System.Collections;

public class PumpEmissionController : MonoBehaviour, IResetable
{
    [Header("Console Subscription")]
    [SerializeField] private Machine01 machineSource;

    [Header("Pump Identificator")]
    [SerializeField] private LiquidPump.PumpType pumpType = LiquidPump.PumpType.Purified;

    [Header("Intensities and colors Settings")]
    [ColorUsage(true, true)]
    [SerializeField] private Color pumpColor = Color.cyan;
    [SerializeField] private float activeBrightness = 10f;
    [SerializeField] private float inactiveBrightness = 0.05f;

    [Header("Transition Lerp Settings")]
    [SerializeField] private float transitionDuration = 0.25f;

    private Renderer pumpRenderer;
    private Material runtimeMat;
    private Coroutine transitionCoroutine;
    private Coroutine overrideCoroutine;

    private float currentBrightness;
    private Color currentEffectColor;
    private bool isCurrentlyActive;
    private bool isOverridden = false;

    public LiquidPump.PumpType GetPumpType() => pumpType;

    private void Awake()
    {
        pumpRenderer = GetComponent<Renderer>();
        if (pumpRenderer != null && pumpRenderer.materials.Length > 0)
        {
            runtimeMat = pumpRenderer.material;
            runtimeMat.EnableKeyword("_EMISSION");
        }
        currentEffectColor = pumpColor;
    }

    private void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.RegisterResetable(this);

        if (machineSource != null)
        {
            isCurrentlyActive = CheckIfShouldBeActive(machineSource.GetActivePump());
            ApplyInstantState(isCurrentlyActive);
        }
    }

    private void OnEnable()
    {
        if (machineSource != null) machineSource.OnPumpSwapped += EvaluateActivation;
    }

    private void OnDisable()
    {
        if (machineSource != null) machineSource.OnPumpSwapped -= EvaluateActivation;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);
    }

    private void EvaluateActivation(LiquidPump activePump)
    {
        isCurrentlyActive = CheckIfShouldBeActive(activePump);
        if (isOverridden)
        {
            currentBrightness = isCurrentlyActive ? activeBrightness : inactiveBrightness;
            if (runtimeMat != null) runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
            return;
        }

        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionEffects(isCurrentlyActive));
    }

    private bool CheckIfShouldBeActive(LiquidPump activePump)
    {
        if (activePump == null) return false;
        return (pumpType == LiquidPump.PumpType.Purified && activePump.name.ToLower().Contains("purified")) ||
               (pumpType == LiquidPump.PumpType.Raw && activePump.name.ToLower().Contains("raw"));
    }

    private IEnumerator TransitionEffects(bool targetActive)
    {
        float targetBrightness = targetActive ? activeBrightness : inactiveBrightness;
        float startBrightness = currentBrightness;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsedTime / transitionDuration);

            currentBrightness = Mathf.Lerp(startBrightness, targetBrightness, smoothT);

            if (runtimeMat != null)
            {
                runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
            }
            yield return null;
        }

        currentBrightness = targetBrightness;
        if (runtimeMat != null) runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
    }

    public void OverridePumpColor(Color targetColor)
    {
        isOverridden = true;
        if (overrideCoroutine != null) StopCoroutine(overrideCoroutine);
        overrideCoroutine = StartCoroutine(LerpOverrideColor(targetColor));
    }

    public void RestoreDefaultColor()
    {
        if (!isOverridden) return;
        isOverridden = false;

        if (overrideCoroutine != null) StopCoroutine(overrideCoroutine);
        overrideCoroutine = StartCoroutine(LerpOverrideColor(pumpColor));

        if (machineSource != null) EvaluateActivation(machineSource.GetActivePump());
    }

    private IEnumerator LerpOverrideColor(Color targetColor)
    {
        Color startColor = currentEffectColor;
        float elapsedTime = 0f;
        float duration = 0.12f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            currentEffectColor = Color.Lerp(startColor, targetColor, t);

            if (runtimeMat != null)
            {
                runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
            }
            yield return null;
        }

        currentEffectColor = targetColor;
        if (runtimeMat != null) runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
    }

    private void ApplyInstantState(bool active)
    {
        currentBrightness = active ? activeBrightness : inactiveBrightness;
        currentEffectColor = pumpColor;
        if (runtimeMat != null) runtimeMat.SetColor("_EmissionColor", currentEffectColor * currentBrightness);
    }

    public void ResetValues()
    {
        isOverridden = false;
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        if (overrideCoroutine != null) StopCoroutine(overrideCoroutine);

        bool shouldBeActiveOnNewDay = (pumpType == LiquidPump.PumpType.Purified);
        ApplyInstantState(shouldBeActiveOnNewDay);
    }
}