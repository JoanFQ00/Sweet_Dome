using UnityEngine;

public class ButtonEmissionController : MonoBehaviour
{
    [Header("Validator Configuration")]
    [SerializeField] private ButtonValidator validator;

    [Header("Color Estetic Configuration")]
    [ColorUsage(true, true)]
    [SerializeField] private Color activeColor = Color.white;

    [ColorUsage(true, true)]
    [SerializeField] private Color disabledColor = Color.black;

    [Header("Secondary Material")]
    [SerializeField] private Renderer secondaryButtonMesh;

    [Header("Campaign Constraints")]
    [SerializeField] private bool turnOffOnDay1 = false;

    [SerializeField] private bool strictlyFollowValidator = false;

    public ButtonValidator Validator => validator;

    private Renderer primaryRenderer;
    private Material runtimeMatOne;
    private Material runtimeMatTwo;
    private bool isOverridden = false;

    private void Awake()
    {
        primaryRenderer = GetComponent<Renderer>();
        if (primaryRenderer != null && primaryRenderer.materials.Length > 0)
        {
            runtimeMatOne = primaryRenderer.material;
            runtimeMatOne.EnableKeyword("_EMISSION");
        }

        if (secondaryButtonMesh != null && secondaryButtonMesh.materials.Length > 0)
        {
            runtimeMatTwo = secondaryButtonMesh.material;
            runtimeMatTwo.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        if (isOverridden) return;

        if (strictlyFollowValidator || (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1))
        {
            if (validator != null && validator.CanPress())
            {
                SetEmissionColorToAll(activeColor);
            }
            else
            {
                SetEmissionColorToAll(disabledColor);
            }
            return;
        }

        bool isDay1 = (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1);

        if (isDay1 && turnOffOnDay1)
        {
            SetEmissionColorToAll(disabledColor);
            return;
        }

        if (validator != null && !validator.CanPress())
        {
            if (isDay1 && !turnOffOnDay1)
            {
                SetEmissionColorToAll(activeColor);
            }
            else
            {
                SetEmissionColorToAll(disabledColor);
            }
        }
        else
        {
            SetEmissionColorToAll(activeColor);
        }
    }

    public void OverrideEmissionColor(Color customColor)
    {
        isOverridden = true;
        SetEmissionColorToAll(customColor);
    }

    public void ClearOverride()
    {
        isOverridden = false;
    }

    public bool IsInteractable()
    {
        if (strictlyFollowValidator)
        {
            if (validator == null) return true;
            return validator.CanPress();
        }

        bool isDay1 = (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1);
        if (isDay1 && turnOffOnDay1) return false;

        if (validator == null) return true;
        return validator.CanPress();
    }

    private void SetEmissionColorToAll(Color color)
    {
        if (runtimeMatOne != null)
        {
            runtimeMatOne.SetColor("_EmissionColor", color);
        }
        if (runtimeMatTwo != null)
        {
            runtimeMatTwo.SetColor("_EmissionColor", color);
        }
    }
}