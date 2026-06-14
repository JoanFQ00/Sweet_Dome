using UnityEngine;

public class ButtonEmissionController : MonoBehaviour
{
    [Header("Configuración del Validador")]
    [Tooltip("El validador físico asignado a este botón (ej: tu CityButtonValidator).")]
    [SerializeField] private ButtonValidator validator;

    [Header("Configuración Estética de Colores")]
    [ColorUsage(true, true)]
    [SerializeField] private Color activeColor = Color.white;

    [ColorUsage(true, true)]
    [SerializeField] private Color disabledColor = Color.black;

    [Header("Mallas / Materiales Secundarios o Externos")]
    [Tooltip("Arrastra aquí la malla externa de la ciudad que deba imitar al botón al 100%.")]
    [SerializeField] private Renderer secondaryButtonMesh;

    [Header("Restricciones de Campaña (Reglas de Flujo)")]
    [Tooltip("MARCA ESTO SÓLO EN EL BOTÓN DE PRIORIDAD. Hace que el botón amanezca completamente apagado en negro el Día 1.")]
    [SerializeField] private bool turnOffOnDay1 = false;

    [Tooltip("MARCA ESTO EN EL BOTÓN DE LA CIUDAD. El botón y su malla externa SOLO se encenderán si CanPress es estrictamente TRUE, ignorando excepciones estéticas del Día 1.")]
    [SerializeField] private bool strictlyFollowValidator = false;

    // Propiedad pública para que el mánager de cursores lea el validador
    public ButtonValidator Validator => validator;

    private Renderer primaryRenderer;
    private Material runtimeMatOne;
    private Material runtimeMatTwo;
    private bool isOverridden = false;

    private void Awake()
    {
        // 1. Capturamos e instanciamos el material del propio botón
        primaryRenderer = GetComponent<Renderer>();
        if (primaryRenderer != null && primaryRenderer.materials.Length > 0)
        {
            runtimeMatOne = primaryRenderer.material;
            runtimeMatOne.EnableKeyword("_EMISSION");
        }

        // 2. Capturamos e instanciamos el material de la malla externa (Luz de la ciudad)
        if (secondaryButtonMesh != null && secondaryButtonMesh.materials.Length > 0)
        {
            runtimeMatTwo = secondaryButtonMesh.material;
            runtimeMatTwo.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        // Si un trigger externo (como el Switch de zona) bloquea el color, congelamos el refresco automático
        if (isOverridden) return;

        // 1. REGLA ULTRA-ESTRICTA (Para tu botón de la Ciudad):
        // Si esta casilla está marcada, obedecemos a ciegas al validador sin importar el calendario
        if (strictlyFollowValidator)
        {
            if (validator != null && validator.CanPress())
            {
                SetEmissionColorToAll(activeColor);
            }
            else
            {
                SetEmissionColorToAll(disabledColor);
            }
            return; // Cortamos la ejecución aquí para que no entre en las reglas de los demás botones
        }

        // 2. COMPORTAMIENTO PARA LOS DEMÁS BOTONES DE LA CABINA (Switch, Prioridad, etc.)
        bool isDay1 = (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1);

        // Excepción del botón de prioridad en el Día 1
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

    /// <summary>
    /// Fuerza a ambos materiales a adoptar un color HDR externo (Bloqueo por Trigger).
    /// </summary>
    public void OverrideEmissionColor(Color customColor)
    {
        isOverridden = true;
        SetEmissionColorToAll(customColor);
    }

    /// <summary>
    /// Libera los materiales para que vuelvan a responder al flujo automático de la consola.
    /// </summary>
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