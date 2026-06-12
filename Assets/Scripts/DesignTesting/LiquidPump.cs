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
    [Tooltip("Coloca los tubos DE IZQUIERDA A DERECHA (Elemento 0 = Izquierda, Elemento 1 = Derecha)")]
    [SerializeField] private FunnelBase[] funnelArray;

    [Tooltip("El índice por defecto al arrancar el día (0 para el tubo izquierdo de su zona, 1 para el derecho)")]
    [SerializeField] private int initialIndex = 0;
    private int funnelIndex;

    [Header("Other")]
    [SerializeField] private float transferSpeed = 10f;
    [Tooltip("Coloca los Waypoints DE IZQUIERDA A DERECHA (Elemento 0 = Izquierda, Elemento 1 = Derecha)")]
    [SerializeField] private Transform[] pumpPositions;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector3 targetGlobalPosition;

    protected override void Start()
    {
        initialValues = new List<float> { 0f, 0f, 0f, 0f, 0f };

        // Inicializamos los waypoints en el frame 1
        InitializePositions();

        // Ejecuta el Start de CapabilityBase que lo registra en el manager de forma segura
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
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.liquidAbsorb, transform.position);
    }

    public void Expelling()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        float amount = transferSpeed * Time.deltaTime;
        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        UpdateCapacity(-inserted);
        AudioManager.Instance.PlayOneShot(AudioManager.Instance.liquidExpel, transform.position);
    }
}