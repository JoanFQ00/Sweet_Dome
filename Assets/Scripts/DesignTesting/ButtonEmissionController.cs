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
    [Tooltip("El color e intensidad HDR que quieres que tengan las mallas cuando estén ENCENDIDAS.")]
    [SerializeField] private Color activeEmissionColor = Color.yellow;

    private void Awake()
    {
        validator = GetComponent<ButtonValidator>();

        foreach (Renderer meshRenderer in targetMeshes)
        {
            if (meshRenderer != null)
            {
                // .material clona el material de la malla conservando TODAS las propiedades y keywords intactas
                Material clonedMaterial = meshRenderer.material;

                if (clonedMaterial != null)
                {
                    if (emissionTexture != null)
                    {
                        clonedMaterial.SetTexture("_EmissionMap", emissionTexture);
                    }

                    // Aseguramos que la GPU sepa que este material TIENE pase de emisión activo de por vida
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

    public void ResetValues()
    {
        EvaluateLightState();
    }

    private void EvaluateLightState()
    {
        canPressCached = (validator != null) ? validator.CanPress() : true;

        bool shouldBeOn = !invertLogic ? canPressCached : !canPressCached;

        if (shouldBeOn) TurnOnLight();
        else TurnOffLight();
    }

    public void TurnOnLight()
    {
        foreach (Material mat in runtimeMaterials)
        {
            if (mat != null) mat.SetColor("_EmissionColor", activeEmissionColor);
        }
    }

    public void TurnOffLight()
    {
        foreach (Material mat in runtimeMaterials)
        {
            if (mat != null) mat.SetColor("_EmissionColor", Color.black);
        }
    }

    public bool IsInteractable()
    {
        return canPressCached;
    }
}