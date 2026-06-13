using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    private Color originalColor;
    private BaseAction baseAction;
    private ButtonEmissionController emissionController;
    private Material sharedRuntimeMaterial;

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (rend == null) rend = GetComponent<Renderer>();

        baseAction = GetComponent<BaseAction>();
        emissionController = GetComponent<ButtonEmissionController>();

        if (rend != null)
        {
            // ¡LA CLAVE!: En lugar de hacer un 'new Material()', le pedimos '.material'.
            // Si el ButtonEmissionController ya lo clonó en el Awake, usará ese mismo.
            // Si es un objeto estático normal, lo clonará de forma limpia por primera vez.
            sharedRuntimeMaterial = rend.material;
            originalColor = sharedRuntimeMaterial.color;
        }
    }

    public void LightenColor()
    {
        // Si es un botón bloqueado, no permitimos el highlight para no confundir al jugador
        if (emissionController != null && !emissionController.IsInteractable())
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
            // Sumamos el color de resaltado al color base. La rayita de emisión se queda intacta
            sharedRuntimeMaterial.color = originalColor + (highlightColor * lightenAmount);
        }
    }

    public void ResetColor()
    {
        if (sharedRuntimeMaterial != null)
        {
            // Devolvemos el color base a su estado normal
            sharedRuntimeMaterial.color = originalColor;
        }
    }
}