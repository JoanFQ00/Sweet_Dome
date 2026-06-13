using UnityEngine;
using System;
using System.Collections;
using FMODUnity;

public class WaveWinner : MonoBehaviour, IResetable
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;

    [Header("Objetos a Desactivar")]
    public GameObject waveVisualsContainer;

    [Header("Tolerancies")]
    public float amplitudeTolerance = 0.18f;
    public float frequencyTolerance = 0.18f;
    public float phaseTolerance = 0.22f;

    [Header("Ajustes de Seguridad del Puzzle")]
    [SerializeField] private float safetyMargin = 0.3f;
    [SerializeField] private float snapDuration = 1.5f;

    // Cambiado a público para que tus botones (ButtonKeyboard) puedan leerlo bien
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
        if (Input.GetKeyDown(KeyCode.V))
            StartWaveEvent(40f);
        // REGLA: El temporizador SOLO descuenta tiempo si el evento está activo 
        // Y el jugador todavía NO ha sintonizado con éxito la onda (evita que se apague en mitad del código)
        if (inEvent && !waveSolved && !isSnapping)
        {
            EventTimer();

            if (IsMatching())
            {
                StartCoroutine(SnapWaveRoutine());
            }
        }
    }

    private void HandleChairInteraction(bool entered)
    {
        if (!entered) ResetValues();
    }

    // =================================================================
    // PASO 1 Y 2: CORRUTINA DE ACOPLE PERFECTO Y ENTREGA DE CÓDIGO
    // =================================================================
    private IEnumerator SnapWaveRoutine()
    {
        isSnapping = true;
        OnDialsLockRequested?.Invoke(); // Congelamos las ruedas del jugador

        float elapsedTime = 0f;
        float startAmp = playerRenderer.amplitude;
        float startFreq = playerRenderer.frequency;
        float startPhase = playerRenderer.phase;

        while (elapsedTime < snapDuration)
        {
            elapsedTime += Time.deltaTime;
            float percent = elapsedTime / snapDuration;
            percent = percent * percent * (3f - 2f * percent); // Suavizado matemático

            playerRenderer.amplitude = Mathf.Lerp(startAmp, targetRenderer.amplitude, percent);
            playerRenderer.frequency = Mathf.Lerp(startFreq, targetRenderer.frequency, percent);
            playerRenderer.phase = Mathf.Lerp(startPhase, targetRenderer.phase, percent);

            yield return null;
        }

        playerRenderer.amplitude = targetRenderer.amplitude;
        playerRenderer.frequency = targetRenderer.frequency;
        playerRenderer.phase = targetRenderer.phase;

        isSnapping = false;
        waveSolved = true; // <-- PASO 2: ¡Aquí tus botones ButtonKeyboard ya dan "true"!
        hasWon = true;

        RuntimeManager.PlayOneShot(AudioManager.Instance.waveSuccess);

        // PASO 3: Se permite poner el código activando el sistema
        if (codeSystem != null)
        {
            codeSystem.EnableCodeSystem(this);
        }
    }

    public void StartWaveEvent(float duration)
    {
        if (inEvent) return;

        inEvent = true;
        hasWon = false;
        waveSolved = false;
        isSnapping = false;
        currentEventTime = duration;

        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(true);

        OnHardwareResetRequested?.Invoke();
        GenerateRandomTarget();
    }

    public void EventLoader(float _timeToEvent, float _eventTime) => StartWaveEvent(_eventTime);

    private void EventTimer()
    {
        currentEventTime -= Time.deltaTime;
        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;
            RuntimeManager.PlayOneShot(AudioManager.Instance.waveFail);

            // Si el tiempo general del evento llega a cero antes de sintonizar, da fallo y apaga todo
            NotifyEventResult(false);
        }
    }

    // PASO 4: El CodeSystem llama aquí para finiquitar el evento tras procesar el código
    public void NotifyEventResult(bool success)
    {
        OnEventFinished?.Invoke(success);
        ResetValues(); // Limpieza absoluta de la pantalla al terminar el código (bien o mal)
    }

    public void ResetValues()
    {
        StopAllCoroutines();
        inEvent = false;
        isSnapping = false;
        waveSolved = false;
        hasWon = false;
        currentEventTime = 0f;

        // Se apaga todo (LineRenderers incluidos)
        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(false);
        OnHardwareResetRequested?.Invoke();

        if (codeSystem != null && codeSystem.IsCodeSystemActive())
        {
            codeSystem.ForceDisable();
        }
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