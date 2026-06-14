using System.Collections;
using UnityEngine;

public class PumpEmissionController : MonoBehaviour, IResetable
{
    [Header("Suscripción a la Consola")]
    [SerializeField] private Machine01 machineSource;

    [Header("Identificación de esta Bomba")]
    [SerializeField] private LiquidPump.PumpType pumpType = LiquidPump.PumpType.Purified;

    [Header("Configuración de Colores e Intensidades")]
    [ColorUsage(true, true)]
    [SerializeField] private Color pumpColor = Color.cyan;
    [SerializeField] private float activeBrightness = 10f;
    [SerializeField] private float inactiveBrightness = 0.05f;

    [Header("Ajustes de la Transición (Lerp)")]
    [SerializeField] private float transitionDuration = 0.25f;

    private Renderer pumpRenderer;
    private Material runtimeMat;
    private Coroutine transitionCoroutine;
    private float currentBrightness;

    private void Awake()
    {
        pumpRenderer = GetComponent<Renderer>();
        if (pumpRenderer != null && pumpRenderer.materials.Length > 0)
        {
            runtimeMat = pumpRenderer.material;
            runtimeMat.EnableKeyword("_EMISSION");
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        if (machineSource != null)
        {
            ApplyInstantState(CheckIfShouldBeActive(machineSource.GetActivePump()));
        }
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
            float t = elapsedTime / transitionDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentBrightness = Mathf.Lerp(startBrightness, targetBrightness, smoothT);

            if (runtimeMat != null)
            {
                runtimeMat.SetColor("_EmissionColor", pumpColor * currentBrightness);
            }

            yield return null;
        }

        currentBrightness = targetBrightness;
        if (runtimeMat != null)
        {
            runtimeMat.SetColor("_EmissionColor", pumpColor * currentBrightness);
        }
    }

    private void ApplyInstantState(bool active)
    {
        currentBrightness = active ? activeBrightness : inactiveBrightness;

        if (runtimeMat != null)
        {
            runtimeMat.SetColor("_EmissionColor", pumpColor * currentBrightness);
        }
    }

    // ¡SOLUCIÓN! Al cambiar de día, forzamos de golpe que el pump Purified esté encendido y el Raw apagado,
    // garantizando que se sincronicen perfectamente con el estado inicial de Machine01.
    public void ResetValues()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // El nuevo día siempre arranca con la máquina en modo Purified
        bool shouldBeActiveOnNewDay = (pumpType == LiquidPump.PumpType.Purified);

        ApplyInstantState(shouldBeActiveOnNewDay);
    }
}