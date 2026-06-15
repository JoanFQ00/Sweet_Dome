using UnityEngine;

public class CityPriorityManager : MonoBehaviour, IResetable
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

    private FunnelBase prioritizedZone;
    private bool priorityUsedToday = false;
    private int prioritizedZoneIndex = -1;

    [Header("Configuración Especial Día 5 (Fin del Juego)")]
    // Cambia esto a true desde el MachineManager al acabar el diálogo para activar el botón
    public bool isDay5CrisisActive = false;

    // Lee esto en un IF desde el MachineManager para saber si el jugador lo ha pulsado
    public bool hasBeenPressedDay5 = false;

    private Material buttonMaterial;
    private bool lightSet = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        // Cacheamos el material del mesh de forma segura para la luz del botón
        Renderer rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            buttonMaterial = rend.material;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    void Update()
    {
        // CONTROL DE LUZ ROJA DÍA 5: En cuanto se activa la crisis, se vuelve rojo emisivo
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (isDay5CrisisActive && !lightSet && buttonMaterial != null)
            {
                lightSet = true;
                Color rojoHDR = new Color(3f, 0f, 0f, 1f); // Rojo intenso URP
                buttonMaterial.EnableKeyword("_EMISSION");
                if (buttonMaterial.HasProperty("_EmissionColor"))
                {
                    buttonMaterial.SetColor("_EmissionColor", rojoHDR);
                }
                DynamicGI.SetEmissive(GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>(), rojoHDR);
            }
        }
    }

    public void ResetValues()
    {
        prioritizedZone = null;
        priorityUsedToday = false;
        prioritizedZoneIndex = -1;

        // Limpieza de banderas del Día 5
        isDay5CrisisActive = false;
        hasBeenPressedDay5 = false;
        lightSet = false;

        OnPriorityResetVisual();
    }

    protected virtual void OnPriorityResetVisual() { }

    // =================================================================
    // ACCIÓN DEL BOTÓN (Se ejecuta al pulsar)
    // =================================================================
    public void PrioritizeCurrentZone()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (!isDay5CrisisActive) return; // Seguridad: si no hay crisis, no hace nada

            hasBeenPressedDay5 = true;
            Debug.Log("<color=#FF0000><b>[BOTÓN APOCALIPSIS]</b></color> El jugador ha activado el botón en el Día 5.");
            return;
        }

        // --- COMPORTAMIENTO HABITUAL (Días 1 a 4) ---
        if (cityFunnel == null) return;
        FunnelBase currentZone = cityFunnel.GetCurrentZone();
        if (currentZone == null) return;

        prioritizedZone = currentZone;
        priorityUsedToday = true;
        prioritizedZoneIndex = cityFunnel.GetIdxCurrentZone();
    }

    // -------------------------
    // GETTERS COMPLEMENTARIOS
    // -------------------------
    public bool HasUsedPriorityToday() => priorityUsedToday;
    public FunnelBase GetPrioritizedZone() => prioritizedZone;
    public int GetIdxPrioritizedZone() => prioritizedZoneIndex;
    public bool IsZonePrioritized(FunnelBase zone) => prioritizedZone == zone;
}