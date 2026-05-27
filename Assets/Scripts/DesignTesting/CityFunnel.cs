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

    private void OnDisable()
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

        float added = zone.funnel.AddLiquid(CurrentCapacity);
        UpdateCapacity(-added);
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

    public List<CityZoneData> GetCityZonesData()
    {
        return cityZones;
    }
}