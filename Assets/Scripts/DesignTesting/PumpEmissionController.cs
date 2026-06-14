using System.Collections;
using UnityEngine;

public class PumpEmissionController : MonoBehaviour, IResetable
{
    [Header("Suscripción a la Consola")]
    [SerializeField] private Machine01 machineSource;

    [Header("Identificación de esta Bomba")]
    [Tooltip("¿Este objeto físico es el surtidor Purificado o el Crudo (Raw)?")]
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
        // Al estar el script en la propia bomba, cogemos su propio Renderer y clonamos su material (Element 0)
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

        // Evaluación inicial nada más arrancar el juego
        if (machineSource != null)
        {
            bool shouldBeActive = CheckIfShouldBeActive(machineSource.GetActivePump());
            ApplyInstantState(shouldBeActive);
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

        // Comprobamos únicamente el tipo basándonos en el nombre del objeto activo en el switch
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

    // INTERFAZ IRESETABLE: Coordinado con el cambio de día instantáneo
    public void ResetValues()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (machineSource != null)
        {
            bool shouldBeActive = CheckIfShouldBeActive(machineSource.GetActivePump());
            ApplyInstantState(shouldBeActive);
        }
    }
}