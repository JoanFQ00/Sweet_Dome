using System.Collections.Generic;
using UnityEngine;

public class CityFunnel : FunnelBase, IResetable
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        UpdateSelectedZoneUI();
    }

    public new void ResetValues()
    {
        indexZone = 0;

        for (int i = 0; i < cityZones.Count; i++)
        {
            if (cityZones[i] != null && cityZones[i].funnel != null)
            {
                cityZones[i].funnel.ResetValues();
            }
        }

        currentCapacity = PutInitialCapacity(GameManager.Instance.currentDay);
        CapacityUpdated();
        OnStartDayFinished();

        UpdateSelectedZoneUI();
    }

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

    private void UpdateSelectedZoneUI()
    {
        var zones = GetUnlockedZones();

        if (zones.Count == 0)
            return;

        var zone = zones[indexZone];

        if (zone == null || zone.funnel == null)
            return;

        CapacityUpdated();

        CityLabel label = zone.funnel.GetComponent<CityLabel>();
        if (label != null)
            OnNameUpdated?.Invoke(label.label);
    }

    public void GoRight()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        indexZone = (indexZone + 1) % zones.Count;
    }

    public void GoLeft()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        indexZone = (indexZone - 1 + zones.Count) % zones.Count;
    }

    public bool HasLiquid()
    {
        return Mathf.RoundToInt(CurrentCapacity) >= 1;
    }

    public void ConfirmLiquid()
    {
        var zones = GetUnlockedZones();
        if (zones.Count == 0) return;

        var zone = zones[indexZone];
        if (zone == null || zone.funnel == null) return;

        CityZoneEnergy energy = zone.funnel.GetComponent<CityZoneEnergy>();

        if (energy != null)
        {
            float visualAmountToSend = Mathf.RoundToInt(CurrentCapacity);
            if (visualAmountToSend < 1f) return;
            float modifiedAmount = energy.GetModifiedAmount(visualAmountToSend);
            zone.funnel.AddLiquid(modifiedAmount);
            UpdateCapacity(-visualAmountToSend);
        }
    }

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
        if (zones.Count == 0) return null;

        return zones[indexZone].funnel;
    }
    
    public int GetIdxCurrentZone()
    {
        return indexZone;
    }

    public List<CityZoneData> GetCityZonesData()
    {
        return cityZones;
    }

    public void TriggerCrackInZone(int zoneIndex, float duration, float severityMultiplier)
    {
        if (zoneIndex < 0 || zoneIndex >= cityZones.Count) return;

        var zoneData = cityZones[zoneIndex];
        if (zoneData != null && zoneData.funnel != null)
        {
            CityZoneEnergy energyComponent = zoneData.funnel.GetComponent<CityZoneEnergy>();
            if (energyComponent != null)
            {
                energyComponent.TriggerCrackEvent(duration, severityMultiplier);
            }
        }
    }

    public void DrainLiquidFromZoneImmediate(int zoneIndex, float amountToRemove)
    {
        if (zoneIndex < 0 || zoneIndex >= cityZones.Count) return;

        var zoneData = cityZones[zoneIndex];
        if (zoneData != null && zoneData.funnel != null)
        {
            zoneData.funnel.RemoveLiquid(amountToRemove);
        }
    }

    public void ChangeLiquidInZone(int zoneIndex, float targetValue, float duration)
    {
        if (zoneIndex < 0 || zoneIndex >= cityZones.Count) return;

        var zoneData = cityZones[zoneIndex];
        if (zoneData != null && zoneData.funnel != null)
        {
            CityZoneEnergy energyComponent = zoneData.funnel.GetComponent<CityZoneEnergy>();
            if (energyComponent != null)
            {
                energyComponent.ChangeLiquidDirectly(targetValue, duration);
            }
        }
    }

    public void SetGlobalEnergyFlowPause(bool pause)
    {
        CapabilityBase.IsGlobalFlowPaused = pause;
    }
}