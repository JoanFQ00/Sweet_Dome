using UnityEngine;

public class ButtonEmissionController : MonoBehaviour
{
    [Header("Configuración del Validador")]
    [Tooltip("El validador físico asignado a este botón (como tu Day1ButtonValidator).")]
    [SerializeField] private ButtonValidator validator;

    [Header("Configuración Estética de Colores")]
    [ColorUsage(true, true)]
    [SerializeField] private Color activeColor = Color.white;

    [ColorUsage(true, true)]
    [SerializeField] private Color disabledColor = Color.black;

    [Header("Mallas / Materiales Secundarios")]
    [Tooltip("Opcional: Arrastra aquí el Renderer de la segunda pieza del botón que deba iluminarse idénticamente.")]
    [SerializeField] private Renderer secondaryButtonMesh;

    [Header("Restricciones de Campaña (¡REGLA DÍA 1!)")]
    [Tooltip("MARCA ESTO SÓLO EN EL BOTÓN DE PRIORIDAD. Hace que el botón amanezca completamente apagado en negro el Día 1.")]
    [SerializeField] private bool turnOffOnDay1 = false;

    // Propiedad pública para que el sistema de cursores y el Highlight lean el validador desde fuera
    public ButtonValidator Validator => validator;

    private Renderer primaryRenderer;
    private Material runtimeMatOne;
    private Material runtimeMatTwo;
    private bool isOverridden = false;

    private void Awake()
    {
        // 1. Capturamos e instanciamos el material principal
        primaryRenderer = GetComponent<Renderer>();
        if (primaryRenderer != null && primaryRenderer.materials.Length > 0)
        {
            runtimeMatOne = primaryRenderer.material;
            runtimeMatOne.EnableKeyword("_EMISSION");
        }

        // 2. Capturamos e instanciamos el material secundario si se ha asignado en el Inspector
        if (secondaryButtonMesh != null && secondaryButtonMesh.materials.Length > 0)
        {
            runtimeMatTwo = secondaryButtonMesh.material;
            runtimeMatTwo.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        // Si un trigger externo (como el Switch o la prioridad en días 2+) bloquea el color, congelamos el refresco
        if (isOverridden) return;

        // Comprobamos si estamos en el Día 1
        bool isDay1 = (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1);

        // EXCEPCIÓN DEL BOTÓN DE PRIORIDAD: Si es el Día 1 Y tiene la casilla marcada, a negro absoluto.
        if (isDay1 && turnOffOnDay1)
        {
            SetEmissionColorToAll(disabledColor);
            return;
        }

        // COMPORTAMIENTO PARA LOS DEMÁS BOTONES (Con tu Day1ButtonValidator):
        // Si el validador dice que no se puede pulsar en este frame...
        if (validator != null && !validator.CanPress())
        {
            // Si es el Día 1 pero NO es el de prioridad (la casilla está desmarcada), lo mantenemos encendido estéticamente
            if (isDay1 && !turnOffOnDay1)
            {
                SetEmissionColorToAll(activeColor);
            }
            else
            {
                // En cualquier otro día normal del juego, si el validador da false, se apaga (a negro)
                SetEmissionColorToAll(disabledColor);
            }
        }
        else
        {
            // Si el validador da el visto bueno, color encendido normal
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

    /// <summary>
    /// Comprueba si el botón se puede clicar o interactuar en este frame (Para el Highlight).
    /// </summary>
    public bool IsInteractable()
    {
        bool isDay1 = (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1);

        // El botón de prioridad en el Día 1 no da feedback de ningún tipo
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