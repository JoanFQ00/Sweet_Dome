using UnityEngine;

public class PriorizedLedCity : MonoBehaviour, IResetable
{
    [Header("Identificación")]
    [Tooltip("Asigna un índice único a este LED en el inspector (0, 1, 2 o 3) para identificarlo.")]
    public int myLedIndex = 0;

    private Renderer targetRenderer;
    private Material ledMaterial;
    private const string EmissionColorKeyword = "_EmissionColor";

    void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            ledMaterial = targetRenderer.material;
        }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        ResetValues();
    }

    private void OnEnable()
    {
        PriorityPress.OnPressedButton += EvaluateLedSignal;
    }

    private void OnDisable()
    {
        PriorityPress.OnPressedButton -= EvaluateLedSignal;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    private void EvaluateLedSignal(Color incomingHdrColor, int targetIndex)
    {
        // Comprobamos si el botón pulsado va dirigido a nuestro índice
        if (myLedIndex == targetIndex)
        {
            TurnOnLed(incomingHdrColor);
        }
        else
        {
            TurnOffLed();
        }
    }

    // Simplificado: ahora solo recibe el color HDR que ya contiene su propia intensidad de brillo
    public void TurnOnLed(Color hdrColor)
    {
        if (ledMaterial == null) return;

        ledMaterial.EnableKeyword("_EMISSION");

        // Aplicamos el color directamente, Unity ya interpreta la intensidad HDR integrada
        ledMaterial.SetColor(EmissionColorKeyword, hdrColor);

        // Actualizamos la iluminación global en tiempo real
        DynamicGI.SetEmissive(targetRenderer, hdrColor);
    }

    public void TurnOffLed()
    {
        if (ledMaterial == null) return;

        ledMaterial.DisableKeyword("_EMISSION");
        ledMaterial.SetColor(EmissionColorKeyword, Color.black);

        DynamicGI.SetEmissive(targetRenderer, Color.black);
    }

    public void ResetValues()
    {
        TurnOffLed();
    }
}