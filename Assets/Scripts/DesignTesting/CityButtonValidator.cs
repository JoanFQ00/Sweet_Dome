using UnityEngine;

public class CityButtonValidator : ButtonValidator
{
    [SerializeField] private CityFunnel cityFunnel;

    public override bool CanPress()
    {
        if (cityFunnel == null) return false;

        float currentCapacity = cityFunnel.CurrentCapacity;
        int visualCapacity = Mathf.RoundToInt(currentCapacity);

        return visualCapacity >= 1;
    }
}