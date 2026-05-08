using UnityEngine;

public class Funnel : MonoBehaviour
{
    public enum FunnelType
    { 
        StorageProcessed,
        CityFunnel, 
        BlenderFunnel, 
        DomeFunnel,
        Purified
    }
    public FunnelType type;
    public TubeController tubeCont;
    public LiquidType typeOfLiquid;

    [Header("Capacity")]
    public bool canContainLiquid = true;
    public float maxCapacity = 100f;
    public float startCapcity = 25f;

    private float currentCapacity;

    void Start()
    {
        currentCapacity = startCapcity;
        tubeCont = FindAnyObjectByType<TubeController>();
    }

    private void Update()
    {
        switch (type) 
        {
            case FunnelType.CityFunnel:

                break;

            case FunnelType.BlenderFunnel:

                break;

            case FunnelType.DomeFunnel:

                break;
        }
    }

    public bool CanFill(float amount)
    {
        return currentCapacity + amount <= maxCapacity;
    }

    public void Fill(float amount) 
    {
        currentCapacity += amount;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);
    }

    public bool CanEmpty(float amount)
    {
        return currentCapacity - amount <= maxCapacity;
    }

    public void Empty(float amount) 
    {
        currentCapacity -= amount;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);
    }

    public float GetPercentage()
    {
        return currentCapacity / maxCapacity;
    }

    public float GetCurrentCapacity()
    {
        return currentCapacity;
    }
}
