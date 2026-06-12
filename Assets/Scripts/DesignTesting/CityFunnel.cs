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

        // Registrar este objeto en el GameManager central para el reinicio limpio de días
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        UpdateSelectedZoneUI();
    }

    // Método obligatorio de la interfaz IResetable para limpiar los días
    // Añadimos 'new' para indicarle a C# que oculte el método heredado a propósito
    public new void ResetValues()
    {
        // Forzamos a que cada zona de la ciudad resetee su líquido al del nuevo día
        for (int i = 0; i < cityZones.Count; i++)
        {
            if (cityZones[i] != null && cityZones[i].funnel != null)
            {
                cityZones[i].funnel.ResetValues();
            }
        }

        // También nos reseteamos a nosotros mismos (como FunnelBase)
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
            float modifiedAmount = energy.GetModifiedAmount(originalAmount);

            zone.funnel.AddLiquid(modifiedAmount);
            UpdateCapacity(-originalAmount);
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

    // =================================================================
    // CONTROL DE FLUJO GLOBAL (PAUSA DE MÁQUINA)
    // =================================================================

    /// <summary>
    /// Activa o desactiva por completo el paso del tiempo y energía en todas las zonas de la ciudad.
    /// </summary>
    /// <param name="pause">True para congelar la energía, False para reanudarla.</param>
    public void SetGlobalEnergyFlowPause(bool pause)
    {
        Debug.Log($"<color=#FFFF00><b>[CONTROL MÁQUINA]</b></color> Pausa global de fluidos cambiada a: PAUSADO = {pause}");

        // Modificamos la propiedad estática de la base. Afecta a todo el juego instantáneamente.
        CapabilityBase.IsGlobalFlowPaused = pause;
    }
}