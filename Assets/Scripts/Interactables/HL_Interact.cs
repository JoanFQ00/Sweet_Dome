using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    private Color originalColor;
    private BaseAction baseAction;
    private ButtonEmissionController emissionController;
    private DialRotation dialRotation;
    private Material sharedRuntimeMaterial;

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend == null) rend = GetComponent<Renderer>();

        baseAction = GetComponent<BaseAction>();
        emissionController = GetComponent<ButtonEmissionController>();
        dialRotation = GetComponent<DialRotation>();

        if (rend != null)
        {
            sharedRuntimeMaterial = rend.material;
            originalColor = sharedRuntimeMaterial.color;
        }
    }

    public void LightenColor()
    {
        // 1. CONTROL DE RUEDAS DE RADAR: Si está bloqueado el radar, abortamos el brillo
        if (TryGetComponent(out WheelRotator radarWheel) && !radarWheel.IsInteractable())
        {
            return;
        }

        // 2. CONTROL DEL SLIDER DE POSICIÓN: Si no hay evento activo, abortamos el brillo
        if (TryGetComponent(out SliderCont slider) && !slider.IsInteractable())
        {
            return;
        }

        // 3. CONTROL DE BOTONES NORMALES DE CONSOLA: Bloqueo si no son interactuables
        if (emissionController != null && !emissionController.IsInteractable())
        {
            return;
        }

        // 4. CONTROL DE DIALES DE FRECUENCIA: Bloqueo si la onda ya se resolvió o no hay evento
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