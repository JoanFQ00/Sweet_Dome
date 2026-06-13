using UnityEngine;
using System;
using FMODUnity;

public class WaveWinner : MonoBehaviour, IResetable
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;

    [Header("Objetos a Desactivar")]
    [Tooltip("El objeto padre que contiene los LineRenderers de las ondas para apagarlos por completo fuera de los eventos.")]
    public GameObject waveVisualsContainer;

    [Header("Tolerancies (Ajustadas)")]
    public float amplitudeTolerance = 0.18f;
    public float frequencyTolerance = 0.18f;
    public float phaseTolerance = 0.22f;

    [Header("Time Message")]
    public float messageDuration = 3f;

    [Header("Ajustes de Seguridad del Puzzle")]
    [SerializeField] private float safetyMargin = 0.3f;

    private bool hasWon = false;
    private float timer = 0f;
    private bool showingMessage = false;
    public bool waveSolved = false;

    [Header("Event State")]
    private bool inEvent = false;
    private float currentEventTime;

    public static Action<bool> OnEventFinished;
    public static Action OnHardwareResetRequested;

    // BLINDAJE DE ARRANQUE: Forzamos el apagado en el Awake para ganarle la carrera a Unity
    private void Awake()
    {
        if (waveVisualsContainer != null)
        {
            waveVisualsContainer.SetActive(false);
        }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }

        // Aseguramos la limpieza de estados inicial
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            StartWaveEvent(45f);

        if (inEvent)
        {
            EventTimer();

            if (!hasWon && IsMatching())
            {
                hasWon = true;
                waveSolved = true;
                RuntimeManager.PlayOneShot(AudioManager.Instance.waveSuccess);

                ShowWin();
                OnEventFinished?.Invoke(true);
                ResetValues();
            }
        }

        if (showingMessage)
        {
            timer += Time.deltaTime;
            if (timer >= messageDuration)
            {
                HideWinMessage();
            }
        }
    }

    private void HandleChairInteraction(bool entered)
    {
        if (!entered)
        {
            ResetValues();
        }
    }

    public void StartWaveEvent(float duration)
    {
        if (inEvent)
        {
            Debug.LogWarning("[ONDA] Intento de sobrelapar evento rechazado. Ya hay un evento activo.");
            return;
        }

        inEvent = true;
        hasWon = false;
        waveSolved = false;
        currentEventTime = duration;

        // Encendemos la pantalla al comenzar
        if (waveVisualsContainer != null) waveVisualsContainer.SetActive(true);

        OnHardwareResetRequested?.Invoke();
        GenerateRandomTarget();

        Debug.Log($"<color=cyan><b>[ONDA]</b></color> Evento iniciado. Tiempo: {duration}s");
    }

    public void EventLoader(float _timeToEvent, float _eventTime)
    {
        StartWaveEvent(_eventTime);
    }

    private void EventTimer()
    {
        currentEventTime -= Time.deltaTime;
        if (currentEventTime <= 0f)
        {
            currentEventTime = 0f;
            RuntimeManager.PlayOneShot(AudioManager.Instance.waveFail);

            OnEventFinished?.Invoke(false);
            ResetValues();
        }
    }

    // =================================================================
    // INTERFAZ DE RESETEO (SE LLAMA AL CAMBIAR DE DÍA O FIN DE EVENTO)
    // =================================================================
    public void ResetValues()
    {
        inEvent = false;
        currentEventTime = 0f;

        // Apagamos la pantalla y los LineRenderers de forma fulminante
        if (waveVisualsContainer != null)
        {
            waveVisualsContainer.SetActive(false);
        }

        OnHardwareResetRequested?.Invoke();
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

    void ShowWin()
    {
        if (codeSystem != null) codeSystem.EnableCodeSystem();
        showingMessage = true;
        timer = 0f;
    }

    void HideWinMessage() => showingMessage = false;

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