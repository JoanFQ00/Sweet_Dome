using System.Collections;
using UnityEngine;

public class CompartmentEmissionController : MonoBehaviour, IResetable
{
    [Header("Console Subscription")]
    [SerializeField] private Machine01 machineSource;

    [Header("Behaviour identicator")]
    [SerializeField] private LiquidPump.PumpType associatedType = LiquidPump.PumpType.Purified;
    [SerializeField] private int compartmentIndex = 0;

    [Header("Starter values Config")]
    [SerializeField] private int purifiedPumpInitialIndex = 0;
    [SerializeField] private int rawPumpInitialIndex = 1;

    [Header("Color Intensities Config")]
    [ColorUsage(true, true)]
    [SerializeField] private Color compartmentColor = Color.cyan;

    [Header("Metallic structure Config")]
    [SerializeField] private float standardActiveBrightness = 10f;
    [SerializeField] private float standardInactiveBrightness = 0.05f;

    [Header("Shader Liquid Config")]
    [SerializeField] private float shaderActiveStrength = 10f;
    [SerializeField] private float shaderInactiveStrength = 0f;

    [Header("Transition Lerp Settings")]
    [SerializeField] private float transitionDuration = 0.25f;

    [Header("Light Mesh")]
    [SerializeField] private Renderer standardEmissiveMesh;

    private Renderer liquidShaderMesh;
    private Material standardRuntimeMat;
    private Material liquidRuntimeMat;

    private Coroutine transitionCoroutine;
    private float currentStandardBrightness;
    private float currentShaderStrength;
    private bool isCurrentlyActive;

    private bool isInitialized = false;

    private void Awake()
    {
        liquidShaderMesh = GetComponent<Renderer>();
        if (liquidShaderMesh != null && liquidShaderMesh.materials.Length > 0)
        {
            liquidRuntimeMat = liquidShaderMesh.material;
        }

        if (standardEmissiveMesh != null && standardEmissiveMesh.materials.Length > 0)
        {
            standardRuntimeMat = standardEmissiveMesh.material;
            standardRuntimeMat.EnableKeyword("_EMISSION");
        }

        ApplyInstantState(false);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        ExecuteInitialSetup();
        Invoke(nameof(EnableInitialization), 0.05f);
    }

    private void OnEnable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped += EvaluateActivation;
        }
    }

    private void OnDisable()
    {
        if (machineSource != null)
        {
            machineSource.OnPumpSwapped -= EvaluateActivation;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    private void EvaluateActivation(LiquidPump activePump)
    {
        if (!isInitialized) return;

        bool shouldBeActive = CheckIfShouldBeActive(activePump);

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionEffects(shouldBeActive));
    }

    private bool CheckIfShouldBeActive(LiquidPump activePump)
    {
        if (activePump == null) return false;

        bool typeMatches = (associatedType == LiquidPump.PumpType.Purified && activePump.name.ToLower().Contains("purified")) ||
                            (associatedType == LiquidPump.PumpType.Raw && activePump.name.ToLower().Contains("raw"));

        bool indexMatches = activePump.CurrentFunnelIndex == compartmentIndex;

        return typeMatches && indexMatches;
    }

    private IEnumerator TransitionEffects(bool targetActive)
    {
        isCurrentlyActive = targetActive;

        float targetStandard = targetActive ? standardActiveBrightness : standardInactiveBrightness;
        float targetShader = targetActive ? shaderActiveStrength : shaderInactiveStrength;
        Color targetShaderColor = targetActive ? compartmentColor : Color.black;

        float startStandard = currentStandardBrightness;
        float startShader = currentShaderStrength;
        Color startShaderColor = liquidRuntimeMat != null ? liquidRuntimeMat.GetColor("_EmissionColor") : Color.black;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentStandardBrightness = Mathf.Lerp(startStandard, targetStandard, smoothT);
            currentShaderStrength = Mathf.Lerp(startShader, targetShader, smoothT);
            Color currentShaderColor = Color.Lerp(startShaderColor, targetShaderColor, smoothT);

            if (standardRuntimeMat != null)
            {
                standardRuntimeMat.SetColor("_EmissionColor", compartmentColor * currentStandardBrightness);
            }

            if (liquidRuntimeMat != null)
            {
                liquidRuntimeMat.SetColor("_EmissionColor", currentShaderColor);
                liquidRuntimeMat.SetFloat("_EmissionStrengh", currentShaderStrength);
            }

            yield return null;
        }

        currentStandardBrightness = targetStandard;
        currentShaderStrength = targetShader;

        if (standardRuntimeMat != null)
        {
            standardRuntimeMat.SetColor("_EmissionColor", compartmentColor * currentStandardBrightness);
        }
        if (liquidRuntimeMat != null)
        {
            liquidRuntimeMat.SetColor("_EmissionColor", targetShaderColor);
            liquidRuntimeMat.SetFloat("_EmissionStrengh", currentShaderStrength);
        }
    }

    private void ApplyInstantState(bool active)
    {
        isCurrentlyActive = active;
        currentStandardBrightness = active ? standardActiveBrightness : standardInactiveBrightness;
        currentShaderStrength = active ? shaderActiveStrength : shaderInactiveStrength;

        if (standardRuntimeMat != null)
        {
            standardRuntimeMat.SetColor("_EmissionColor", compartmentColor * currentStandardBrightness);
        }

        if (liquidRuntimeMat != null)
        {
            liquidRuntimeMat.SetColor("_EmissionColor", active ? compartmentColor : Color.black);
            liquidRuntimeMat.SetFloat("_EmissionStrengh", currentShaderStrength);
        }
    }

    private void ExecuteInitialSetup()
    {
        bool shouldBeActiveOnStart = (associatedType == LiquidPump.PumpType.Purified && compartmentIndex == purifiedPumpInitialIndex);
        ApplyInstantState(shouldBeActiveOnStart);
    }

    private void EnableInitialization()
    {
        isInitialized = true;
    }

    public void ResetValues()
    {
        isInitialized = false;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        ExecuteInitialSetup();

        CancelInvoke(nameof(EnableInitialization));
        Invoke(nameof(EnableInitialization), 0.05f);
    }
}