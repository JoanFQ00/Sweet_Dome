using System.Collections.Generic;
using UnityEngine;

public class CityFunnel : FunnelBase
{
    [System.Serializable]
    public class CityZoneData
    {
        public FunnelBase funnel;
        public int unlockDay;
    }

    [Header("City Zones")]
    [SerializeField] private List<CityZoneData> cityZones = new List<CityZoneData>();

    private int indexZone = 0;

    private void OnEnable()
    {
        GameManager.NewDay += OnDayChanged;
    }

    protected override void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
    }

    protected override void Start()
    {
        indexZone = 0;
        base.Start();

        UpdateSelectedZoneUI();
    }

    // -------------------------
    // CORE: zonas desbloqueadas
    // -------------------------
    private List<CityZoneData> GetUnlockedZones()
    {
        int currentDay = (int)GameManager.Instance.currentDay;

        List<CityZoneData> result = new List<CityZoneData>();

        for (int i = 0; i < cityZones.Count; i++)
        {
            if (cityZones[i].funnel == null)
                continue;

            if (cityZones[i].unlockDay <= currentDay)
                result.Add(cityZones[i]);
        }

        return result;
    }

    // -------------------------
    // EVENTO día cambiado
    // -------------------------
    private void OnDayChanged()
    {
        ClampIndex();
        UpdateSelectedZoneUI();
    }

    private void ClampIndex()
    {
        var zones = GetUnlockedZones();

        if (zones.Count == 0)
        {
            indexZone = 0;
            return;
        }

        if (indexZone >= zones.Count)
            indexZone = zones.Count - 1;

        if (indexZone < 0)
            indexZone = 0;
    }

    // -------------------------
    // UI UPDATE
    // -------------------------
    private void UpdateSelectedZoneUI()
    {
        var zones = GetUnlockedZones();

        if (zones.Count == 0)
            return;

        var zone = zones[indexZone];

        if (zone == null || zone.funnel == null)
            return;

        OnCapacityUpdated?.Invoke(zone.funnel.CurrentCapacity);

        CityLabel label = zone.funnel.GetComponent<CityLabel>();
        if (label != null)
            OnNameUpdated?.Invoke(label.label);
    }

    // -------------------------
    // INPUT
    // -------------------------
    public void GoRight()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        indexZone = (indexZone + 1) % zones.Count;
        //UpdateSelectedZoneUI();
    }

    public void GoLeft()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        indexZone = (indexZone - 1 + zones.Count) % zones.Count;
        //UpdateSelectedZoneUI();
    }

    // -------------------------
    // ACTION
    // -------------------------
    public void ConfirmLiquid()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        var zone = zones[indexZone];
        if (zone == null || zone.funnel == null) return;

        CityZoneEnergy energy = zone.funnel.GetComponent<CityZoneEnergy>();

        if (energy != null)
        {
            float originalAmount = CurrentCapacity;

            float modifiedAmount =
                energy.GetModifiedAmount(originalAmount);

            zone.funnel.AddLiquid(modifiedAmount);

            // SOLO restas lo original
            UpdateCapacity(-originalAmount);
        }
    }

    // -------------------------
    // INITIAL UI HELPERS
    // -------------------------
    public float GetInitialValue()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return 0f;

        return zones[indexZone].funnel.CurrentCapacity;
    }

    public string GetInitialName()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return "";

        CityLabel label = zones[indexZone].funnel.GetComponent<CityLabel>();
        return label != null ? label.label : "";
    }

    public List<FunnelBase> GetCityZones()
    {
        List<FunnelBase> result = new();

        foreach (var z in cityZones)
            result.Add(z.funnel);

        return result;
    }

    public FunnelBase GetCurrentZone()
    {
        var zones = GetUnlockedZones();

        if (zones.Count == 0)
            return null;

        return zones[indexZone].funnel;
    }

    public List<CityZoneData> GetCityZonesData()
    {
        return cityZones;
    }


    // =================================================================
    // ENTRADAS PÚBLICAS PARA EL ÁRBOL DE DECISIONES (PUENTE / FACHADA)
    // =================================================================

    /// <summary>
    /// Dispara el evento de grieta en una ciudad específica buscando por su índice en la lista global.
    /// </summary>
    /// <param name="zoneIndex">0: Zona Urbana, 1: Escuela, 2: Hospital, 3: Residencia Presidencial</param>
    /// <param name="duration">Duración en segundos del evento.</param>
    /// <param name="severityMultiplier">Multiplicador de velocidad de drenaje.</param>
    public void TriggerCrackInZone(int zoneIndex, float duration, float severityMultiplier)
    {
        Debug.Log($"<color=#FF0055><b>[ÁRBOL DE DECISIONES]</b></color> Llamando a TriggerCrackInZone -> Índice: {zoneIndex} | Duración: {duration}s | Fuerza: x{severityMultiplier}");

        if (zoneIndex < 0 || zoneIndex >= cityZones.Count)
        {
            Debug.LogError($"[CityFunnel] Índice de zona inválido: {zoneIndex}. No se puede aplicar la grieta.");
            return;
        }

        var zoneData = cityZones[zoneIndex];

        if (zoneData != null && zoneData.funnel != null)
        {
            CityZoneEnergy energyComponent = zoneData.funnel.GetComponent<CityZoneEnergy>();

            if (energyComponent != null)
            {
                energyComponent.TriggerCrackEvent(duration, severityMultiplier);
            }
            else
            {
                Debug.LogWarning($"[CityFunnel] El objeto {zoneData.funnel.name} no contiene un componente CityZoneEnergy.");
            }
        }
    }

    /// <summary>
    /// Modifica de forma directa y suavizada (Lerp por Update) el líquido de una zona mediante su índice.
    /// </summary>
    /// <param name="zoneIndex">0: Zona Urbana, 1: Escuela, 2: Hospital, 3: Residencia Presidencial</param>
    /// <param name="targetValue">Cantidad de líquido objetivo.</param>
    /// <param name="duration">Tiempo en segundos que tardará en llegar a ese valor.</param>
    public void ChangeLiquidInZone(int zoneIndex, float targetValue, float duration)
    {
        Debug.Log($"<color=#00FFCC><b>[ÁRBOL DE DECISIONES]</b></color> Llamando a ChangeLiquidInZone -> Índice: {zoneIndex} | Target: {targetValue} | Tiempo: {duration}s");
        
        if (zoneIndex < 0 || zoneIndex >= cityZones.Count)
        {
            Debug.LogError($"[CityFunnel] Índice de zona inválido: {zoneIndex}. No se puede cambiar el líquido.");
            return;
        }

        var zoneData = cityZones[zoneIndex];

        if (zoneData != null && zoneData.funnel != null)
        {
            CityZoneEnergy energyComponent = zoneData.funnel.GetComponent<CityZoneEnergy>();

            if (energyComponent != null)
            {
                energyComponent.ChangeLiquidDirectly(targetValue, duration);
            }
            else
            {
                Debug.LogWarning($"[CityFunnel] El objeto {zoneData.funnel.name} no contiene un componente CityZoneEnergy.");
            }
        }
    }
}