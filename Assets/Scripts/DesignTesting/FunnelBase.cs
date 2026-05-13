using UnityEngine;
using System.Collections.Generic;

public class FunnelBase : CapabilityBase
{
    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    protected override void Start()
    {
        initialValues = new List<float> { 50f, 60f, 70f, 80f, 90f }; // Example values for each day
        base.Start();
    }

    protected override float PutInitialCapacity(Day day)
    {
        return day switch
        {
            Day.Day1 => initialValues[0],
            Day.Day2 => initialValues[1],
            Day.Day3 => initialValues[2],
            Day.Day4 => initialValues[3],
            _ => initialValues[4]
        };
    }

    public virtual float AddLiquid(float amount)
    {
        float spaceLeft = maxCapacity - currentCapacity;
        float added = Mathf.Min(amount, spaceLeft);

        UpdateCapacity(added);

        return added;
    }

    public virtual float RemoveLiquid(float amount)
    {
        float removable = Mathf.Min(amount, currentCapacity);

        UpdateCapacity(-removable);

        return removable;
    }
}