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

    public void Fill() 
    {
        currentCapacity += tubeCont.outSpeed *Time.deltaTime;

        currentCapacity = Mathf.Max(currentCapacity, maxCapacity);
    }

    public void Empty() 
    {
        currentCapacity -= tubeCont.inSpeed * Time.deltaTime;

        currentCapacity = Mathf.Min(currentCapacity, 0);
    }
}
