using UnityEngine;

public class PriorizedLedCity : MonoBehaviour, IResetable
{
    [Header("Identificator")]
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
        if (myLedIndex == targetIndex)
        {
            TurnOnLed(incomingHdrColor);
        }
        else
        {
            TurnOffLed();
        }
    }

    public void TurnOnLed(Color hdrColor)
    {
        if (ledMaterial == null) return;

        ledMaterial.EnableKeyword("_EMISSION");
        ledMaterial.SetColor(EmissionColorKeyword, hdrColor);

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