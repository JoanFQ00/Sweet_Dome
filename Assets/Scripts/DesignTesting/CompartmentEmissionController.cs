using System.Collections;
using UnityEngine;

public class CompartmentEmissionController : MonoBehaviour, IResetable
{
    [Header("Suscripción a la Consola")]
    [SerializeField] private Machine01 machineSource;

    [Header("Identificación de este Compartimento")]
    [SerializeField] private LiquidPump.PumpType associatedType = LiquidPump.PumpType.Purified;
    [SerializeField] private int compartmentIndex = 0;

    [Header("Configuración de Colores e Intensidades")]
    [ColorUsage(true, true)]
    [SerializeField] private Color compartmentColor = Color.cyan;

    [Header("Configuración Estructura Metálica (Rejilla)")]
    [SerializeField] private float standardActiveBrightness = 10f;
    [SerializeField] private float standardInactiveBrightness = 0.05f;

    [Header("Configuración Shader de Líquido")]
    [SerializeField] private float shaderActiveStrength = 10f;
    [SerializeField] private float shaderInactiveStrength = 0f;

    [Header("Ajustes de la Transición (Lerp)")]
    [Tooltip("Tiempo en segundos que tarda la luz en encenderse o apagarse por completo.")]
    [SerializeField] private float transitionDuration = 0.25f;

    [Header("Mallas a Iluminar")]
    [Tooltip("Arrastra aquí el objeto externo del Tubo Metálico (ej. Storage).")]
    [SerializeField] private Renderer standardEmissiveMesh;

    private Renderer liquidShaderMesh;
    private Material standardRuntimeMat;
    private Material liquidRuntimeMat;

    // Variables de control para las corrutinas y estados actuales
    private Coroutine transitionCoroutine;
    private float currentStandardBrightness;
    private float currentShaderStrength;
    private bool isCurrentlyActive;

    private void Awake()
    {
        // 1. Capturamos el material nativo del líquido (donde está el script)
        liquidShaderMesh = GetComponent<Renderer>();
        if (liquidShaderMesh != null && liquidShaderMesh.materials.Length > 0)
        {
            liquidRuntimeMat = liquidShaderMesh.material;
        }

        // 2. Cacheamos el material del tubo de metal externo
        if (standardEmissiveMesh != null && standardEmissiveMesh.materials.Length > 0)
        {
            standardRuntimeMat = standardEmissiveMesh.material;
            standardRuntimeMat.EnableKeyword("_EMISSION");
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        // Evaluación inicial inmediata al arrancar el juego
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

        // Iniciamos la transición suave hacia el nuevo estado
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

    /// <summary>
    /// Corrutina que se encarga de hacer el Lerp fluido de las intensidades
    /// </summary>
    private IEnumerator TransitionEffects(bool targetActive)
    {
        isCurrentlyActive = targetActive;

        // Definimos los objetivos según si se enciende o se apaga
        float targetStandard = targetActive ? standardActiveBrightness : standardInactiveBrightness;
        float targetShader = targetActive ? shaderActiveStrength : shaderInactiveStrength;
        Color targetShaderColor = targetActive ? compartmentColor : Color.black;

        // Guardamos los valores desde los que empezamos el Lerp actual
        float startStandard = currentStandardBrightness;
        float startShader = currentShaderStrength;
        Color startShaderColor = liquidRuntimeMat != null ? liquidRuntimeMat.GetColor("_EmissionColor") : Color.black;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Suavizamos el 't' con un SmoothStep para que el frenado y arranque sea más orgánico
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Interpolamos los valores numéricos
            currentStandardBrightness = Mathf.Lerp(startStandard, targetStandard, smoothT);
            currentShaderStrength = Mathf.Lerp(startShader, targetShader, smoothT);
            Color currentShaderColor = Color.Lerp(startShaderColor, targetShaderColor, smoothT);

            // Aplicamos los valores en los materiales en este frame
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

        // Nos aseguramos de clavar los valores finales exactos al terminar
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

    /// <summary>
    /// Aplica el estado de golpe (sin Lerp). Ideal para el inicio del juego o cambio de día instantáneo.
    /// </summary>
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

    // INTERFAZ IRESETABLE: Al cambiar de día, forzamos el estado por defecto instantáneamente
    public void ResetValues()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        if (machineSource != null)
        {
            // Forzamos el estado inicial en seco para evitar que el Lerp actúe durante el fundido a negro del cambio de día
            bool shouldBeActive = CheckIfShouldBeActive(machineSource.GetActivePump());
            ApplyInstantState(shouldBeActive);
        }
    }
}