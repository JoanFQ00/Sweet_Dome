using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LiquidPump : MonoBehaviour
{
    [Header("Capability")]
    [SerializeField] private float maxCapacity = 100, minCapacity = 0, currentCapacity;

    [Header("Funnels")]
    [SerializeField] private FunnelBase[] funnelArray;
    [SerializeField] private int initialIndex = 0;
    int funnelIndex = 0;

    [Header("Other")]
    [SerializeField] private float transferSpeed = 10f;
    [SerializeField] private Transform[] pumpPositions;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetPosition;

    public float CurrentCapacity => currentCapacity;

    void Start()
    {
        funnelIndex = initialIndex;
        targetPosition = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            Debug.Log("Ha llegado");
        }
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

    public void PumpAbsorbing()
    {
        float amount = transferSpeed * Time.deltaTime;

        float freeSpace = maxCapacity - currentCapacity;
        amount = Mathf.Min(amount, freeSpace);

        float extracted = funnelArray[funnelIndex].RemoveLiquid(amount);
        currentCapacity += extracted;
    }

    public void PumpExpulsing()
    {
        float amount = transferSpeed * Time.deltaTime;

        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        currentCapacity -= inserted;
    }
}