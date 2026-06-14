using UnityEngine;
using System.Collections.Generic;

public class ButtonEmissionController : MonoBehaviour, IResetable
{
    private ButtonValidator validator;
    private List<Material> runtimeMaterials = new List<Material>();
    private bool canPressCached = true;

    [Header("Logic Configuration")]
    [Tooltip("Si está marcado, la luz funcionará al revés: se ENCENDERÁ cuando NO se pueda pulsar.")]
    [SerializeField] private bool invertLogic = false;

    [Header("Configuración de Mallas")]
    [Tooltip("Arrastra aquí todas las mallas (Mesh Renderers) de este botón que quieras que se iluminen juntas.")]
    [SerializeField] private List<Renderer> targetMeshes = new List<Renderer>();

    [Header("Configuración Visual del Inspector")]
    [Tooltip("Arrastra aquí la textura (Emission Map) de este botón (la que tiene la rayita blanca).")]
    [SerializeField] private Texture emissionTexture;

    [ColorUsage(true, true)]
    [Tooltip("El color e intensidad HDR por defecto del inspector cuando el botón está ENCENIDO.")]
    [SerializeField] private Color activeEmissionColor = Color.yellow;

    // SISTEMA DE MEMORIA PARA ACCIONES EXTERNAS
    private bool isColorOverridden = false;
    private Color overriddenColor;

    private void Awake()
    {
        validator = GetComponent<ButtonValidator>();

        foreach (Renderer meshRenderer in targetMeshes)
        {
            if (meshRenderer != null)
            {
                Material clonedMaterial = meshRenderer.material;

                if (clonedMaterial != null)
                {
                    if (emissionTexture != null)
                    {
                        clonedMaterial.SetTexture("_EmissionMap", emissionTexture);
                    }

                    clonedMaterial.EnableKeyword("_EMISSION");
                    clonedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                    runtimeMaterials.Add(clonedMaterial);
                }
            }
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        EvaluateLightState();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    private void Update()
    {
        EvaluateLightState();
    }

    // =================================================================
    // INTERFAZ IRESETABLE (P.EJ: INICIO DE UN NUEVO DÍA)
    // =================================================================
    public void ResetValues()
    {
        // 1. Eliminamos cualquier forzado de color externo para devolver el botón a su diseño original
        ClearOverride();

        // 2. Evaluamos el estado de iluminación inicial que le toque por validador
        EvaluateLightState();
    }

    private void EvaluateLightState()
    {
        // 1. Primero comprobamos de forma obligatoria la disponibilidad del validador
        canPressCached = (validator != null) ? validator.CanPress() : true;

        // 2. Determinamos si la luz debería estar encendida o apagada de forma absoluta
        bool shouldBeOn = !invertLogic ? canPressCached : !canPressCached;

        if (shouldBeOn)
        {
            // CASO A: El botón está disponible y debe iluminarse.
            // Si la máquina de bombas (u otro script) ha inyectado un color forzado, usamos ese.
            if (isColorOverridden)
            {
                TurnOnLight(overriddenColor);
            }
            else
            {
                // Si nadie lo está forzando, usa su color amarillo/blanco por defecto del inspector.
                TurnOnLight(activeEmissionColor);
            }
        }
        else
        {
            // CASO B: El botón NO está disponible. 
            // Da igual qué color le haya mandado la máquina: se apaga fulminantemente en negro.
            TurnOffLight();
        }
    }

    // Modificado para poder recibir cualquier color de forma interna o externa
    public void TurnOnLight(Color targetColor)
    {
        foreach (Material mat in runtimeMaterials)
        {
            if (mat != null) mat.SetColor("_EmissionColor", targetColor);
        }
    }

    public void TurnOffLight()
    {
        foreach (Material mat in runtimeMaterials)
        {
            if (mat != null) mat.SetColor("_EmissionColor", Color.black);
        }
    }

    // =================================================================
    // API PÚBLICA PARA LLAMAR DESDE OTROS SCRIPTS INDIVIDUALES
    // =================================================================

    /// <summary>
    /// Fuerza al botón a brillar con un color e intensidad HDR totalmente personalizados desde fuera.
    /// </summary>
    public void OverrideEmissionColor(Color newHdrColor)
    {
        isColorOverridden = true;
        overriddenColor = newHdrColor;
        TurnOnLight(overriddenColor);
    }

    /// <summary>
    /// Cancela el forzado de color externo y devuelve el botón a su funcionamiento automático por validador.
    /// </summary>
    public void ClearOverride()
    {
        isColorOverridden = false;
        EvaluateLightState();
    }

    public bool IsInteractable()
    {
        return canPressCached;
    }
}