using UnityEngine;
using System.Collections.Generic;

public class LiquidPump : CapabilityBase, IResetable
{
    public enum PumpType
    {
        Purified,
        Raw
    }

    [Header("Initial Values")]
    [SerializeField] private List<float> initialValues;

    [Header("Pump Configuration")]
    [SerializeField] private PumpType pumpType = PumpType.Purified;

    [Header("Funnels & Mapping")]
    [SerializeField] private FunnelBase[] funnelArray;

    [SerializeField] private int initialIndex = 0;
    private int funnelIndex;

    [Header("Other")]
    [SerializeField] private float transferSpeed = 10f;
    [SerializeField] private Transform[] pumpPositions;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetGlobalPosition;

    public int CurrentFunnelIndex => funnelIndex;

    public int InitialFunnelIndex => initialIndex;

    protected override void Start()
    {
        initialValues = new List<float> { 0f, 0f, 0f, 0f, 0f };

        InitializePositions();
        base.Start();
    }

    private void InitializePositions()
    {
        funnelIndex = initialIndex;

        if (pumpPositions != null && funnelIndex < pumpPositions.Length && pumpPositions[funnelIndex] != null)
        {
            transform.position = pumpPositions[funnelIndex].position;
            targetGlobalPosition = pumpPositions[funnelIndex].position;
        }
        else
        {
            targetGlobalPosition = transform.position;
        }
    }

    public new void ResetValues()
    {
        currentCapacity = PutInitialCapacity(GameManager.Instance.currentDay);
        CapacityUpdated();
        OnStartDayFinished();

        InitializePositions();
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
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetGlobalPosition,
            moveSpeed * Time.deltaTime
        );
    }

    public void ShiftRight()
    {
        MoveTowardsIndexIncreasing();
    }

    public void ShiftLeft()
    {
        MoveTowardsIndexDecreasing();
    }

    private void MoveTowardsIndexIncreasing()
    {
        if (funnelIndex + 1 < funnelArray.Length && funnelIndex + 1 < pumpPositions.Length)
        {
            funnelIndex++;
            targetGlobalPosition = pumpPositions[funnelIndex].position;
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.pumpMove, gameObject);
        }
    }

    private void MoveTowardsIndexDecreasing()
    {
        if (funnelIndex - 1 >= 0)
        {
            funnelIndex--;
            targetGlobalPosition = pumpPositions[funnelIndex].position;
            AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.pumpMove, gameObject);
        }
    }

    public void Absorbing()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        float amount = transferSpeed * Time.deltaTime;
        float freeSpace = maxCapacity - currentCapacity;
        amount = Mathf.Min(amount, freeSpace);

        float extracted = funnelArray[funnelIndex].RemoveLiquid(amount);

        UpdateCapacity(extracted);
    }

    public void Expelling()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        float amount = transferSpeed * Time.deltaTime;
        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        UpdateCapacity(-inserted);
    }
}