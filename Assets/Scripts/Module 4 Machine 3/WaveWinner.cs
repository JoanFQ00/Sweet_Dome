using UnityEngine;
using System;
using FMODUnity;

public class WaveWinner : MonoBehaviour
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;

    [Header("Tolerancies")]
    public float amplitudeTolerance = 0.25f;
    public float frequencyTolerance = 0.25f;
    public float phaseTolerance = 0.3f;

    [Header("Time Message")]
    public float messageDuration = 3f;

    [Header("Ajustes de Seguridad del Puzzle")]
    [Tooltip("Margen de seguridad para no forzar la generación en los límites absolutos de los diales (Evita valores inalcanzables).")]
    [SerializeField] private float safetyMargin = 0.3f;

    private bool hasWon = false;
    private float timer = 0f;
    private bool showingMessage = false;
    public bool waveSolved = false;

    [Header("Event")]
    private bool inEvent;
    private float timeToEvent;
    private float eventTime;
    private float currentTimeEvent;
    private float eventStart_Delay;
    private bool delay_On;

    public static Action<bool> OnEventFinished;

    private void OnEnable()
    {
        MachineManager.WaveDispatch += EventLoader;
    }

    private void OnDisable()
    {
        MachineManager.WaveDispatch -= EventLoader;
    }

    void Update()
    {
        if (delay_On)
        {
            DelayTime();
        }
        if (inEvent)
        {
            EventTimer();

            if (!hasWon && IsMatching())
            {
                hasWon = true;
                waveSolved = true;
                RuntimeManager.PlayOneShot(AudioManager.Instance.waveSuccess);

                ShowWin();

                ResetEvent();

                OnEventFinished?.Invoke(true);
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
        if (diff > Mathf.PI)
        {
            diff = (Mathf.PI * 2f) - diff;
        }
        return diff;
    }

    void ShowWin()
    {
        if (codeSystem != null)
        {
            codeSystem.EnableCodeSystem();
        }
        showingMessage = true;
        timer = 0f;
    }

    void HideWinMessage()
    {
        showingMessage = false;
    }

    public void EventLoader(float _timeToEvent, float _eventTime)
    {
        timeToEvent = _timeToEvent;
        eventTime = _eventTime;
        eventStart_Delay = Time.time + timeToEvent;
        delay_On = true;
        currentTimeEvent = eventTime;
    }

    private void EventTimer()
    {
        currentTimeEvent -= Time.deltaTime;
        if (currentTimeEvent <= 0f)
        {
            currentTimeEvent = 0f;
            ResetEvent();
            RuntimeManager.PlayOneShot(AudioManager.Instance.waveFail);
            OnEventFinished?.Invoke(false);
        }
    }

    private void DelayTime()
    {
        if (Time.time >= eventStart_Delay)
        {
            delay_On = false;
            inEvent = true;
            hasWon = false;
            waveSolved = false;

            GenerateRandomTarget();
        }
    }

    // =================================================================
    // GENERACIÓN ALEATORIA BLINDADA Y GARANTIZADA
    // =================================================================
    private void GenerateRandomTarget()
    {
        // Valores por defecto de seguridad por si no se encuentran los diales
        float minAmp = 0.8f, maxAmp = 5f;
        float minFreq = 0.8f, maxFreq = 5f;
        float minPhase = 0f, maxPhase = 5f;

        // Buscamos los diales en la escena para leer sus rangos en tiempo real
        DialRotation[] dials = FindObjectsByType<DialRotation>(FindObjectsSortMode.None);

        foreach (DialRotation dial in dials)
        {
            if (dial.dialType == DialRotation.DialType.Amplitude)
            {
                // Leemos su máximo del inspector, y su mínimo real (que está condicionado por el safeMinimumValue)
                // Intentamos acceder de forma dinámica reflejando lo que calculamos en su script
                minAmp = dial.minValue;
                maxAmp = dial.maxValue;

                // Usamos un truco de reflexión para obtener su safeMinimumValue si está expuesto
                var field = dial.GetType().GetField("safeMinimumValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) minAmp = (float)field.GetValue(dial);
            }
            else if (dial.dialType == DialRotation.DialType.Frequency)
            {
                minFreq = dial.minValue;
                maxFreq = dial.maxValue;

                var field = dial.GetType().GetField("safeMinimumValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) minFreq = (float)field.GetValue(dial);
            }
            else if (dial.dialType == DialRotation.DialType.Phase)
            {
                minPhase = dial.minValue;
                maxPhase = dial.maxValue;
            }
        }

        // Aplicamos el margen de seguridad para no apurar en los extremos de la rosca
        float safeMinAmp = minAmp + safetyMargin;
        float safeMaxAmp = maxAmp - safetyMargin;

        float safeMinFreq = minFreq + safetyMargin;
        float safeMaxFreq = maxFreq - safetyMargin;

        float safeMinPhase = minPhase + safetyMargin;
        float safeMaxPhase = maxPhase - safetyMargin;

        // Aseguramos que por un margen muy grande no se inviertan los límites
        if (safeMinAmp >= safeMaxAmp) { safeMinAmp = minAmp; safeMaxAmp = maxAmp; }
        if (safeMinFreq >= safeMaxFreq) { safeMinFreq = minFreq; safeMaxFreq = maxFreq; }
        if (safeMinPhase >= safeMaxPhase) { safeMinPhase = minPhase; safeMaxPhase = maxPhase; }

        // Asignamos los valores finales calculados de forma segura
        targetRenderer.amplitude = UnityEngine.Random.Range(safeMinAmp, safeMaxAmp);
        targetRenderer.frequency = UnityEngine.Random.Range(safeMinFreq, safeMaxFreq);
        targetRenderer.phase = UnityEngine.Random.Range(safeMinPhase, safeMaxPhase);
    }

    void ResetEvent()
    {
        inEvent = false;
        delay_On = false;
        currentTimeEvent = 0f;
        timeToEvent = 0f;
        eventStart_Delay = 0f;
    }
}