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

    [Header("Visual Effects (Particles)")]
    [SerializeField] private ParticleSystem absorbParticles;
    [SerializeField] private ParticleSystem expelParticles;

    private ActionState currentActionState = ActionState.Idle;
    private bool wasActionCalledThisFrame = false;

    private int framesWithoutAction = 0;
    private const int FRAMES_TO_STOP = 2;

    private AudioSource activeAbsorbSource;
    private AudioSource activeExpelSource;

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

        StopAllEffects();
        currentActionState = ActionState.Idle;
        framesWithoutAction = 0;

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
        // Si no se llama a la acción en este frame, aumentamos el contador de frames vacíos.
        if (!wasActionCalledThisFrame)
        {
            framesWithoutAction++;

            // Solo si pasan 2 frames seguidos sin recibir input de la palanca, apagamos los efectos.
            // Esto absorbe el "ruido" o los saltos de 1 frame de la palanca al moverla sutilmente.
            if (framesWithoutAction >= FRAMES_TO_STOP && currentActionState != ActionState.Idle)
            {
                StopAllEffects();
                currentActionState = ActionState.Idle;
            }
        }
        else
        {
            // Si la acción sigue activa, el contador se mantiene a cero.
            framesWithoutAction = 0;
        }

        wasActionCalledThisFrame = false;
    }

    public void ShiftRight() => MoveTowardsIndexIncreasing();
    public void ShiftLeft() => MoveTowardsIndexDecreasing();

    private void MoveTowardsIndexIncreasing()
    {
        if (funnelIndex + 1 < funnelArray.Length && funnelIndex + 1 < pumpPositions.Length)
        {
            funnelIndex++;
            targetGlobalPosition = pumpPositions[funnelIndex].position;

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.rotateWheelSFX != null)
            {
                SoundManagerOwn.Instance.PlaySFX(SoundManagerOwn.Instance.rotateWheelSFX, false);
            }
        }
    }

    private void MoveTowardsIndexDecreasing()
    {
        if (funnelIndex - 1 >= 0)
        {
            funnelIndex--;
            targetGlobalPosition = pumpPositions[funnelIndex].position;

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.rotateWheelSFX != null)
            {
                SoundManagerOwn.Instance.PlaySFX(SoundManagerOwn.Instance.rotateWheelSFX, false);
            }
        }
    }

    public void Absorbing()
    {
        if (funnelArray == null || funnelIndex >= funnelArray.Length || funnelArray[funnelIndex] == null) return;

        if (currentCapacity >= maxCapacity)
        {
            return;
        }

        wasActionCalledThisFrame = true;

        if (currentActionState != ActionState.Absorbing)
        {
            StopAllEffects();
            currentActionState = ActionState.Absorbing;

            if (absorbParticles != null && !absorbParticles.isPlaying) absorbParticles.Play();

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.absorbSFX != null)
            {
                activeAbsorbSource = SoundManagerOwn.Instance.Create3DAudioSource(
                    SoundManagerOwn.Instance.absorbSFX,
                    transform.position,
                    true
                );
                if (activeAbsorbSource != null) activeAbsorbSource.Play();
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

        if (currentCapacity <= 0f)
        {
            return;
        }

        wasActionCalledThisFrame = true;

        if (currentActionState != ActionState.Expelling)
        {
            StopAllEffects();
            currentActionState = ActionState.Expelling;

            if (expelParticles != null && !expelParticles.isPlaying) expelParticles.Play();

            if (SoundManagerOwn.Instance != null && SoundManagerOwn.Instance.expelSFX != null)
            {
                activeExpelSource = SoundManagerOwn.Instance.Create3DAudioSource(
                    SoundManagerOwn.Instance.expelSFX,
                    transform.position,
                    true
                );
                if (activeExpelSource != null) activeExpelSource.Play();
            }
        }

        float amount = transferSpeed * Time.deltaTime;
        amount = Mathf.Min(amount, currentCapacity);
        float inserted = funnelArray[funnelIndex].AddLiquid(amount);

        UpdateCapacity(-inserted);
    }

    private void StopAllEffects()
    {
        if (absorbParticles != null && absorbParticles.isPlaying) absorbParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (expelParticles != null && expelParticles.isPlaying) expelParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

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