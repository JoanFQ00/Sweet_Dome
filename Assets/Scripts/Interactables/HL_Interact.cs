using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    private Color originalColor;
    private BaseAction baseAction;
    private ButtonEmissionController emissionController;
    private DialRotation dialRotation; // NUEVA REFERENCIA
    private Material sharedRuntimeMaterial;

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend == null) rend = GetComponent<Renderer>();

        baseAction = GetComponent<BaseAction>();
        emissionController = GetComponent<ButtonEmissionController>();
        dialRotation = GetComponent<DialRotation>(); // Buscamos si es una ruedecita

        if (rend != null)
        {
            sharedRuntimeMaterial = rend.material;
            originalColor = sharedRuntimeMaterial.color;
        }
    }

    public void LightenColor()
    {
        // ... Añade esta comprobación dentro del método LightenColor() de tu HL_Interact.cs actual:

        // CONTROL DE RUEDAS DE RADAR: Si es un dial giratorio de radar y está bloqueado, abortamos el brillo
        if (TryGetComponent(out WheelRotator radarWheel) && !radarWheel.IsInteractable())
        {
            return;
        }

        // 1. Control de botones normales bloqueados
        if (emissionController != null && !emissionController.IsInteractable())
        {
            return;
        }

        // 2. NUEVO CONTROL DE DIALES: Si es una rueda de frecuencia y está bloqueada, abortamos el brillo
        if (dialRotation != null && !dialRotation.IsInteractable())
        {
            return;
        }

        // Lógica de tiempo de tu juego
        if (baseAction != null && baseAction.act != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.dayTime != null)
            {
                bool isTime = baseAction.act.hourToInteract == GameManager.Instance.dayTime.currentHour;
                bool isAlwaysAccessible = baseAction.act.alwaysAccesible;
                if (!isTime && !isAlwaysAccessible)
                {
                    return;
                }
            }
        }

        if (sharedRuntimeMaterial != null)
        {
            sharedRuntimeMaterial.color = originalColor + (highlightColor * lightenAmount);
        }
    }

    public void ResetColor()
    {
        if (sharedRuntimeMaterial != null)
        {
            sharedRuntimeMaterial.color = originalColor;
        }
    }
}