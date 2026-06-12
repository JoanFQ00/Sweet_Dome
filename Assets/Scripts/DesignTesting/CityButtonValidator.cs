using UnityEngine;

public class CityButtonValidator : ButtonValidator
{
    [SerializeField] private CityFunnel cityFunnel;

    public override bool CanPress()
    {
        return cityFunnel.HasLiquid();
    }
}