using UnityEngine;

public class Funnel : MonoBehaviour
{
    public enum CompartmentType
    {
        StorageProcessed,
        City,
        Dome,
        StorageRaw,
        Purifier
    }

    [Header("Type")]
    public CompartmentType type;

    [Header("Liquid")]
    public LiquidType acceptedLiquid;

    [Header("Capacity")]
    public float maxCapacity = 100f;
    public float startCapacity = 25f;

    private float currentCapacity;

    private void Start()
    {
        currentCapacity = startCapacity;
    }

    public bool CanFill(float amount)
    {
        return currentCapacity + amount <= maxCapacity;
    }

    public bool CanEmpty(float amount)
    {
        return currentCapacity - amount >= 0f;
    }

    public void Fill(float amount)
    {
        currentCapacity += amount;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);
    }

    public void Empty(float amount)
    {
        currentCapacity -= amount;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);
    }

    public float GetCurrentCapacity()
    {
        return currentCapacity;
    }

    public float GetPercentage()
    {
        return currentCapacity / maxCapacity;
    }
}