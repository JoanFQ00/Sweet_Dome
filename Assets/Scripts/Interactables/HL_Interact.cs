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
        if (TryGetComponent(out WheelRotator radarWheel) && !radarWheel.IsInteractable())
        {
            return;
        }

        if (TryGetComponent(out SliderCont slider) && !slider.IsInteractable())
        {
            return;
        }

        if (emissionController != null && !emissionController.IsInteractable())
        {
            return;
        }

        if (dialRotation != null && !dialRotation.IsInteractable())
        {
            return;
        }

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