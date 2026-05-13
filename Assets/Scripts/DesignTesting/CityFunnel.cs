using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CityFunnel : FunnelBase
{
    [SerializeField] private List<FunnelBase> cityZones;
    int indexZone = 0;

    protected override void Start()
    {
        indexZone = 0;
    }

    protected override float GetInitialCapacity(Day day) 
    { 
        return 0f; 
    }

    public void GoRight()
    {
        if (cityZones.Count == 0) return;

        indexZone = (indexZone + 1) % cityZones.Count;

        OnNameUpdated?.Invoke(
            cityZones[indexZone].GetComponent<CityLabel>().label
        );
    }

    public void GoLeft()
    {
        if (cityZones.Count == 0) return;

        indexZone = (indexZone - 1 + cityZones.Count) % cityZones.Count;

        OnNameUpdated?.Invoke(
            cityZones[indexZone].GetComponent<CityLabel>().label
        );
    }

    public override float AddLiquid(float amount)
    {
        return cityZones[indexZone].AddLiquid(amount);
    }

    public override float RemoveLiquid(float amount)
    {
        return cityZones[indexZone].RemoveLiquid(amount);
    }

    public string GetInitialName() 
    {
        return cityZones[indexZone].GetComponent<CityLabel>().label;
    }
}