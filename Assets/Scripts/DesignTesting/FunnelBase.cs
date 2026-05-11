using UnityEngine;
using System.Collections.Generic;

public class FunnelBase : MonoBehaviour
{
    [Header("Capability")]
    [SerializeField] private float maxCapacity = 100, minCapacity = 0, currentCapacity;

    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    public float CurrentCapacity => currentCapacity;

    void Start()
    {
        //BORRAR DESPUES
        initialValues = new List<float> { 20f, 40f, 60f, 80f, 100f };

        StartDay();
    }

    public void StartDay()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                currentCapacity = initialValues[0];
                break;

            case Day.Day2:
                currentCapacity = initialValues[1];
                break;

            case Day.Day3:
                currentCapacity = initialValues[2];
                break;

            case Day.Day4:
                currentCapacity = initialValues[3];
                break;

            default: //Day5
                currentCapacity = initialValues[4];
                break;
        }
    }

    public float AddLiquid(float amount)
    {
        float spaceLeft = maxCapacity - currentCapacity;
        float added = Mathf.Min(amount, spaceLeft);

        currentCapacity += added;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);

        return added;
    }

    public float RemoveLiquid(float amount)
    {
        float removable = Mathf.Min(amount, currentCapacity);

        currentCapacity -= removable;
        currentCapacity = Mathf.Clamp(currentCapacity, 0f, maxCapacity);

        return removable;
    }
}