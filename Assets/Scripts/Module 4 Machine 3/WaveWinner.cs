using UnityEngine;
using System;
using System.Collections;
using FMODUnity;

public class WaveWinner : MonoBehaviour, IResetable
{
    [Header("References")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;

    [Header("Wave Visual Container")]
    public GameObject waveVisualsContainer;

    [Header("Tolerances")]
    public float amplitudeTolerance = 0.18f;
    public float frequencyTolerance = 0.18f;
    public float phaseTolerance = 0.22f;

    [Header("Puzzle safety")]
    [SerializeField] private float safetyMargin = 0.3f;
    [SerializeField] private float snapDuration = 1.5f;

    [HideInInspector] public bool hasWon = false;
    [HideInInspector] public bool waveSolved = false;

    [Header("Event State")]
    private bool inEvent = false;
    private float currentEventTime;
    private bool isSnapping = false;

    public static Action<bool> OnEventFinished;
    public static Action OnHardwareResetRequested;
    public static Action OnDialsLockRequested;

    private void Awake()
    {
        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(false);
    }

    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.RegisterResetable(this);
        ResetValues();

        CheckAndForceDisableDay5();
    }

    private void OnEnable()
    {
        MachineManager.WaveDispatch += EventLoader;
        MiniGameTest.OnMachineEntered += HandleChairInteraction;
    }

    private void OnDisable()
    {
        MachineManager.WaveDispatch -= EventLoader;
        MiniGameTest.OnMachineEntered -= HandleChairInteraction;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);
    }

    void Update()
    {
        if (CheckAndForceDisableDay5()) return;

        if (inEvent && !isSnapping)
        {
            EventTimer();

            if (!waveSolved && IsMatching())
            {
                StartCoroutine(SnapWaveRoutine());
            }
        }
    }

    private bool CheckAndForceDisableDay5()
    {
        if (GameManager.Instance != null && GameManager.Instance.presiSpeaker != null &&
            GameManager.Instance.presiSpeaker.dialogueSys != null &&
            GameManager.Instance.presiSpeaker.dialogueSys.id == 116)
        {
            if (waveVisualsContainer != null && waveVisualsContainer.activeSelf)
                waveVisualsContainer.SetActive(false);

            if (playerRenderer != null && playerRenderer.gameObject.activeSelf)
                playerRenderer.gameObject.SetActive(false);

            if (targetRenderer != null && targetRenderer.gameObject.activeSelf)
                targetRenderer.gameObject.SetActive(false);

            inEvent = false;
            return true;
        }
        return false;
    }

    private void HandleChairInteraction(bool entered)
    {
        if (!entered) ResetValues();
        else CheckAndForceDisableDay5();
    }

    private IEnumerator SnapWaveRoutine()
    {
        isSnapping = true;
        OnDialsLockRequested?.Invoke();

        float elapsedTime = 0f;
        float startAmp = playerRenderer.amplitude;
        float startFreq = playerRenderer.frequency;
        float startPhase = playerRenderer.phase;

        while (elapsedTime < snapDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / snapDuration;
            percent = percent * percent * (3f - 2f * percent);

            playerRenderer.amplitude = Mathf.Lerp(startAmp, targetRenderer.amplitude, percent);
            playerRenderer.frequency = Mathf.Lerp(startFreq, targetRenderer.frequency, percent);
            playerRenderer.phase = Mathf.Lerp(startPhase, targetRenderer.phase, percent);

            yield return null;
        }

        playerRenderer.amplitude = targetRenderer.amplitude;
        playerRenderer.frequency = targetRenderer.frequency;
        playerRenderer.phase = targetRenderer.phase;

        isSnapping = false;
        waveSolved = true;
        hasWon = true;

        if (AudioManager.Instance != null)
            RuntimeManager.PlayOneShot(AudioManager.Instance.waveSuccess);

        if (codeSystem != null)
        {
            codeSystem.EnableCodeSystem(this);
        }
    }

    public void StartWaveEvent(float duration)
    {
        if (CheckAndForceDisableDay5()) return;
        if (inEvent) return;

        inEvent = true;
        hasWon = false;
        waveSolved = false;
        isSnapping = false;
        currentEventTime = duration;

        if (playerRenderer != null) playerRenderer.gameObject.SetActive(true);
        if (targetRenderer != null) targetRenderer.gameObject.SetActive(true);
        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(true);

        OnHardwareResetRequested?.Invoke();
        GenerateRandomTarget();
    }

    public void EventLoader(float _timeToEvent, float _eventTime) => StartWaveEvent(_eventTime);

    private void EventTimer()
    {
        // El tiempo de la onda no se detiene, sigue drenando segundos mientras se introduce el teclado.

        currentEventTime -= Time.deltaTime;
        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;

            if (AudioManager.Instance != null)
                RuntimeManager.PlayOneShot(AudioManager.Instance.waveFail);

            if (codeSystem != null && codeSystem.IsCodeSystemActive())
            {
                codeSystem.ForceTimeoutFail();
            }
            else
            {
                NotifyEventResult(false);
            }
        }
    }

    public void NotifyEventResult(bool success)
    {
        OnEventFinished?.Invoke(success);

        StopAllCoroutines();
        inEvent = false;
        isSnapping = false;
        waveSolved = false;
        hasWon = false;
        currentEventTime = 0f;

        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(false);
        OnHardwareResetRequested?.Invoke();

        CheckAndForceDisableDay5();
    }

    public void ResetValues()
    {
        StopAllCoroutines();
        inEvent = false;
        isSnapping = false;
        waveSolved = false;
        hasWon = false;
        currentEventTime = 0f;

        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(false);
        OnHardwareResetRequested?.Invoke();

        if (codeSystem != null && codeSystem.IsCodeSystemActive())
        {
            codeSystem.ForceDisable();
        }

        CheckAndForceDisableDay5();
    }

    bool IsMatching()
    {
        if (playerRenderer == null || targetRenderer == null) return false;

        float ampDiff = Mathf.Abs(playerRenderer.amplitude - targetRenderer.amplitude);
        float freqDiff = Mathf.Abs(playerRenderer.frequency - targetRenderer.frequency);
        float phaseDiff = PhaseDifference(playerRenderer.phase, targetRenderer.phase);

        return ampDiff <= amplitudeTolerance && freqDiff <= frequencyTolerance && phaseDiff <= phaseTolerance;
    }

    float PhaseDifference(float a, float b)
    {
        float diff = Mathf.Abs(a - b) % (Mathf.PI * 2f);
        if (diff > Mathf.PI) diff = (Mathf.PI * 2f) - diff;
        return diff;
    }

    private void GenerateRandomTarget()
    {
        float minAmp = 0.8f, maxAmp = 5f;
        float minFreq = 0.8f, maxFreq = 5f;
        float minPhase = 0f, maxPhase = 5f;

        DialRotation[] dials = FindObjectsByType<DialRotation>(FindObjectsSortMode.None);

        foreach (DialRotation dial in dials)
        {
            if (dial.dialType == DialRotation.DialType.Amplitude)
            {
                minAmp = dial.minValue; maxAmp = dial.maxValue;
                var field = dial.GetType().GetField("safeMinimumValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) minAmp = (float)field.GetValue(dial);
            }
            else if (dial.dialType == DialRotation.DialType.Frequency)
            {
                minFreq = dial.minValue; maxFreq = dial.maxValue;
                var field = dial.GetType().GetField("safeMinimumValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) minFreq = (float)field.GetValue(dial);
            }
            else if (dial.dialType == DialRotation.DialType.Phase)
            {
                minPhase = dial.minValue; maxPhase = dial.maxValue;
            }
        }

        float safeMinAmp = minAmp + safetyMargin; float safeMaxAmp = maxAmp - safetyMargin;
        float safeMinFreq = minFreq + safetyMargin; float safeMaxFreq = maxFreq - safetyMargin;
        float safeMinPhase = minPhase + safetyMargin; float safeMaxPhase = maxPhase - safetyMargin;

        if (safeMinAmp >= safeMaxAmp) { safeMinAmp = minAmp; safeMaxAmp = maxAmp; }
        if (safeMinFreq >= safeMaxFreq) { safeMinFreq = minFreq; safeMaxFreq = maxFreq; }
        if (safeMinPhase >= safeMaxPhase) { safeMinPhase = minPhase; safeMaxPhase = maxPhase; }

        targetRenderer.amplitude = UnityEngine.Random.Range(safeMinAmp, safeMaxAmp);
        targetRenderer.frequency = UnityEngine.Random.Range(safeMinFreq, safeMaxFreq);
        targetRenderer.phase = UnityEngine.Random.Range(safeMinPhase, safeMaxPhase);
    }
}