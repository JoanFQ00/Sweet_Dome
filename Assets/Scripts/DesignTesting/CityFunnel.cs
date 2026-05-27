//using NUnit.Framework;
//using System;
using System.Collections.Generic;
//using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.Port;

public class CityFunnel : FunnelBase
{
    [SerializeField] private List<FunnelBase> cityZones;
    int indexZone = 0;

    protected override void Start()
    {
        //foreach (FunnelBase zone in cityZones)
        //    zone.OnCapacityUpdated += UpdateCapabilityInUI;

        indexZone = 0;
        base.Start();
    }

    //protected override void OnDisable()
    //{
    //    foreach (FunnelBase zone in cityZones)
    //        zone.OnCapacityUpdated -= UpdateCapabilityInUI;

    //    base.OnDisable();
    //}

    //void UpdateCapabilityInUI(float value)
    //{
    //    OnCapacityUpdated?.Invoke(value);
    //}

    //protected override float PutInitialCapacity(Day day) 
    //{ 
    //    return 0f; 
    //}

    public void GoRight()
    {
        if (cityZones.Count == 0) return;

        indexZone = (indexZone + 1) % cityZones.Count;

        OnCapacityUpdated?.Invoke(
            cityZones[indexZone].CurrentCapacity
        );

        OnNameUpdated?.Invoke(
            cityZones[indexZone].GetComponent<CityLabel>().label
        );
    }

    public void GoLeft()
    {
        if (cityZones.Count == 0) return;

        indexZone = (indexZone - 1 + cityZones.Count) % cityZones.Count;

        OnCapacityUpdated?.Invoke(
            cityZones[indexZone].CurrentCapacity
        );

        OnNameUpdated?.Invoke(
            cityZones[indexZone].GetComponent<CityLabel>().label
        );
    }

    //public override float AddLiquid(float amount)
    //{
    //    return cityZones[indexZone].AddLiquid(amount);
    //}

    //public override float RemoveLiquid(float amount)
    //{
    //    return cityZones[indexZone].RemoveLiquid(amount);
    //}

    public void ConfirmLiquid()
    {
        float added = cityZones[indexZone].AddLiquid(CurrentCapacity);
        UpdateCapacity(-added);
    }

    public float GetInitialValue() 
    {
        return cityZones[indexZone].CurrentCapacity;
    }
    
    public string GetInitialName() 
    {
        return cityZones[indexZone].GetComponent<CityLabel>().label;
    }

    public List<FunnelBase> GetCityZones()
    {
        return cityZones;
    }
}