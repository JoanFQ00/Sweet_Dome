using UnityEngine;
using System.Collections.Generic;

public class LiquidPump : CapabilityBase
{
    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    [Header("Funnels")]
    [SerializeField] private FunnelBase[] funnelArray;
    [SerializeField] private int initialIndex = 0;
    private int funnelIndex;

    [Header("Other")]
    [SerializeField] private float transferSpeed = 10f;
    [SerializeField] private Transform[] pumpPositions;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetPosition;

    protected override void Start()
    {
        initialValues = new List<float> { 0f, 0f, 0f, 0f, 0f }; // Example values for each day

        funnelIndex = initialIndex;
        targetPosition = transform.localPosition;

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

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    public void GoRight()
    {
        if (funnelIndex + 1 < funnelArray.Length)
        {
            funnelIndex++;
            targetPosition = pumpPositions[funnelIndex].localPosition;
        }
    }

    public void GoLeft()
    {
        if (funnelIndex - 1 >= 0)
        {
            funnelIndex--;
            targetPosition = pumpPositions[funnelIndex].localPosition;
        }
    }

    public void Absorbing()
    {
        float amount = transferSpeed * Time.deltaTime;

        float freeSpace = maxCapacity - currentCapacity;
        amount = Mathf.Min(amount, freeSpace);

        float extracted = funnelArray[funnelIndex].RemoveLiquid(amount);

        UpdateCapacity(extracted);
    }

    public void Expelling()
    {
        float amount = transferSpeed * Time.deltaTime;

        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        UpdateCapacity(-inserted);
    }
}