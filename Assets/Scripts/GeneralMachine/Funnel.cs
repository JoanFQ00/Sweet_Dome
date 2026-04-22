using UnityEngine;

public class Funnel : MonoBehaviour
{
    public enum FunnelType{ CityFunnel, BlenderFunnel, DomeFunnel}
    public FunnelType type;
    public TubeController tubeCont;

    [Header("Capacity")]
    public bool canContainLiquid = true;
    public float maxCapacity = 100f;
    public float startCapcity = 25.0f;

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

    public void Fill(float amount) 
    {
        currentCapacity += amount;

        currentCapacity = Mathf.Max(currentCapacity, maxCapacity);
        Debug.Log(currentCapacity);
    }

    public void Empty(float amount) 
    {
        currentCapacity -= amount;

        currentCapacity = Mathf.Min(currentCapacity, 0);
    }
}
