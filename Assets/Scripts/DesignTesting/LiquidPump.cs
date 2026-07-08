using UnityEngine;
using System.Collections.Generic;

public class LiquidPump : CapabilityBase, IResetable
{
    public enum PumpType
    {
        Purified,
        Raw
    }

    private enum ActionState
    {
        Idle,
        Absorbing,
        Expelling
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

    [Header("Visual Effects Mapped (2 Tubos por Surtidor)")]
    [SerializeField] private ParticleSystem[] absorbParticlesArray;
    [SerializeField] private ParticleSystem[] expelParticlesArray;

    private ActionState currentActionState = ActionState.Idle;

    private float lastFrameCapacity;
    private bool wasLiquidExchangedThisFrame = false;
    private bool isActionActiveThisFrame = false;

    private AudioSource activeAbsorbSource;
    private AudioSource activeExpelSource;

    public int CurrentFunnelIndex => funnelIndex;
    public int InitialFunnelIndex => initialIndex;

    protected override void Start()
    {
        initialValues = new List<float> { 0f, 0f, 0f, 0f, 0f };

        InitializePositions();
        lastFrameCapacity = currentCapacity;
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

        ResetActiveFunnelSpeed();
        StopAllEffects();
        currentActionState = ActionState.Idle;

        lastFrameCapacity = currentCapacity;
        wasLiquidExchangedThisFrame = false;
        isActionActiveThisFrame = false;

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

        if (activeAbsorbSource != null) activeAbsorbSource.transform.position = transform.position;
        if (activeExpelSource != null) activeExpelSource.transform.position = transform.position;
    }

    void LateUpdate()
    {
        if (Mathf.Abs(currentCapacity - lastFrameCapacity) > 0.0001f)
        {
            wasLiquidExchangedThisFrame = true;
        }

        // Si el líquido no cambia o la palanca se suelta, pasamos a Idle y limpiamos
        if ((!wasLiquidExchangedThisFrame || !isActionActiveThisFrame) && currentActionState != ActionState.Idle)
        {
            ResetActiveFunnelSpeed();
            StopAllEffects();
            currentActionState = ActionState.Idle;
        }

        lastFrameCapacity = currentCapacity;
        wasLiquidExchangedThisFrame = false;
        isActionActiveThisFrame = false;
    }

    public void ShiftRight() => MoveTowardsIndexIncreasing();
    public void ShiftLeft() => MoveTowardsIndexDecreasing();

    private void MoveTowardsIndexIncreasing()
    {
        ResetActiveFunnelSpeed();
        StopAllEffects();

        if (funnelIndex + 1 < funnelArray.Length && funnelIndex + 1 < pumpPositions.Length)
        {
            funnelIndex++;
            targetGlobalPosition = pumpPositions[funnelIndex].position;
        }
    }

    private void MoveTowardsIndexDecreasing()
    {
        ResetActiveFunnelSpeed();
        StopAllEffects();

        if (funnelIndex - 1 >= 0)
        {
            funnelIndex--;
            targetGlobalPosition = pumpPositions[funnelIndex].position;
        }
    }

    public void Absorbing()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        if (funnelArray[funnelIndex].CurrentCapacity <= 0f || currentCapacity >= maxCapacity)
        {
            ResetActiveFunnelSpeed();
            StopAllEffects();
            return;
        }

        isActionActiveThisFrame = true;

        if (currentActionState != ActionState.Absorbing)
        {
            // En transiciones rápidas milimétricas, no llamamos a StopAllEffects completo para no romper el AudioSource,
            // simplemente cambiamos el estado y forzamos el Play limpio del sistema de partículas.
            currentActionState = ActionState.Absorbing;

            if (absorbParticlesArray != null && funnelIndex < absorbParticlesArray.Length)
            {
                ParticleSystem activeAbsorbPs = absorbParticlesArray[funnelIndex];
                if (activeAbsorbPs != null)
                {
                    activeAbsorbPs.Clear();
                    if (!activeAbsorbPs.isPlaying) activeAbsorbPs.Play();
                }
            }

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.absorbSFX != null)
            {
                if (activeAbsorbSource == null)
                {
                    activeAbsorbSource = SoundManagerOwn.Instance.Create3DAudioSource(
                        SoundManagerOwn.Instance.absorbSFX,
                        transform.position,
                        true
                    );
                    if (activeAbsorbSource != null) activeAbsorbSource.Play();
                }
            }

            funnelArray[funnelIndex].SetWaveSpeed(6.0f);
        }
        else
        {
            if (absorbParticlesArray != null && funnelIndex < absorbParticlesArray.Length)
            {
                ParticleSystem activeAbsorbPs = absorbParticlesArray[funnelIndex];
                if (activeAbsorbPs != null && !activeAbsorbPs.isPlaying) activeAbsorbPs.Play();
            }
        }

        float amount = transferSpeed * Time.deltaTime;
        float freeSpace = maxCapacity - currentCapacity;
        amount = Mathf.Min(amount, freeSpace);

        float extracted = funnelArray[funnelIndex].RemoveLiquid(amount);
        UpdateCapacity(extracted);
    }

    public void Expelling()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        if (currentCapacity <= 0f || funnelArray[funnelIndex].CurrentCapacity >= funnelArray[funnelIndex].MaxCapacity)
        {
            ResetActiveFunnelSpeed();
            StopAllEffects();
            return;
        }

        isActionActiveThisFrame = true;

        if (currentActionState != ActionState.Expelling)
        {
            currentActionState = ActionState.Expelling;

            if (expelParticlesArray != null && funnelIndex < expelParticlesArray.Length)
            {
                ParticleSystem activeExpelPs = expelParticlesArray[funnelIndex];
                if (activeExpelPs != null)
                {
                    activeExpelPs.Clear();
                    if (!activeExpelPs.isPlaying) activeExpelPs.Play();
                }
            }

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.expelSFX != null)
            {
                if (activeExpelSource == null)
                {
                    activeExpelSource = SoundManagerOwn.Instance.Create3DAudioSource(
                        SoundManagerOwn.Instance.expelSFX,
                        transform.position,
                        true
                    );
                    if (activeExpelSource != null) activeExpelSource.Play();
                }
            }

            funnelArray[funnelIndex].SetWaveSpeed(6.0f);
        }
        else
        {
            if (expelParticlesArray != null && funnelIndex < expelParticlesArray.Length)
            {
                ParticleSystem activeExpelPs = expelParticlesArray[funnelIndex];
                if (activeExpelPs != null && !activeExpelPs.isPlaying) activeExpelPs.Play();
            }
        }

        float amount = transferSpeed * Time.deltaTime;
        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        UpdateCapacity(-inserted);
    }

    private void ResetActiveFunnelSpeed()
    {
        if (funnelArray != null && funnelIndex < funnelArray.Length && funnelArray[funnelIndex] != null)
        {
            funnelArray[funnelIndex].SetWaveSpeed(1.3f);
        }
    }

    private void StopAllEffects()
    {
        if (absorbParticlesArray != null)
        {
            foreach (ParticleSystem ps in absorbParticlesArray)
            {
                if (ps != null && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (expelParticlesArray != null)
        {
            foreach (ParticleSystem ps in expelParticlesArray)
            {
                if (ps != null && ps.isPlaying) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        if (activeAbsorbSource != null)
        {
            activeAbsorbSource.Stop();
            Destroy(activeAbsorbSource.gameObject);
            activeAbsorbSource = null;
        }

        if (activeExpelSource != null)
        {
            activeExpelSource.Stop();
            Destroy(activeExpelSource.gameObject);
            activeExpelSource = null;
        }
    }

    protected override void OnDestroy()
    {
        StopAllEffects();
        base.OnDestroy();
    }
}