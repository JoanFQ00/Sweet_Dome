using UnityEngine;
using System;
using static UnityEngine.GraphicsBuffer;
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
        if (currentTimeEvent <=0f)
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

    private void GenerateRandomTarget()
    {
        targetRenderer.amplitude = UnityEngine.Random.Range(1f, 5f);
        targetRenderer.frequency = UnityEngine.Random.Range(1f, 5f);
        targetRenderer.phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
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