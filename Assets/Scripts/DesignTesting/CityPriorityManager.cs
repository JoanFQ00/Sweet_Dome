using UnityEngine;

public class CityPriorityManager : MonoBehaviour, IResetable
{
    [Header("References")]
    [SerializeField] private CityFunnel cityFunnel;

    private FunnelBase prioritizedZone;
    private bool priorityUsedToday = false;
    private int prioritizedZoneIndex = -1;

    [Header("Configuración Especial Día 5 (Fin del Juego)")]
    public bool isDay5CrisisActive = false;

    public bool hasBeenPressedDay5 = false;

    private Material buttonMaterial;
    private bool lightSet = false;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

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
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (isDay5CrisisActive && !lightSet && buttonMaterial != null)
            {
                lightSet = true;

                Color colorBase = new Color(191f / 255f, 7f / 255f, 0f, 1f);
                Color rojoHDR = colorBase * 7.0f;

                buttonMaterial.EnableKeyword("_EMISSION");
                if (buttonMaterial.HasProperty("_EmissionColor"))
                {
                    buttonMaterial.SetColor("_EmissionColor", rojoHDR);
                }
                DynamicGI.SetEmissive(GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>(), rojoHDR);

                if (TryGetComponent(out ButtonEmissionController emissionController))
                {
                    emissionController.OverrideEmissionColor(rojoHDR);
                }
            }
        }
    }

    public void ResetValues()
    {
        prioritizedZone = null;
        priorityUsedToday = false;
        prioritizedZoneIndex = -1;

        isDay5CrisisActive = false;
        hasBeenPressedDay5 = false;
        lightSet = false;

        if (TryGetComponent(out ButtonEmissionController emissionController))
        {
            emissionController.ClearOverride();
        }

        OnPriorityResetVisual();
    }

    protected virtual void OnPriorityResetVisual() { }

    public void PrioritizeCurrentZone()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (!isDay5CrisisActive) return;

            hasBeenPressedDay5 = true;
            Debug.Log("<color=#FF0000><b>[BOTÓN APOCALIPSIS]</b></color> El jugador ha activado el botón en el Día 5.");
            return;
        }

        if (cityFunnel == null) return;
        FunnelBase currentZone = cityFunnel.GetCurrentZone();
        if (currentZone == null) return;

        prioritizedZone = currentZone;
        priorityUsedToday = true;
        prioritizedZoneIndex = cityFunnel.GetIdxCurrentZone();
    }

    public bool HasUsedPriorityToday() => priorityUsedToday;
    public FunnelBase GetPrioritizedZone() => prioritizedZone;
    public int GetIdxPrioritizedZone() => prioritizedZoneIndex;
    public bool IsZonePrioritized(FunnelBase zone) => prioritizedZone == zone;
}