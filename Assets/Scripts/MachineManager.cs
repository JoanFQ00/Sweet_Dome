using FMODUnity;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;

public class MachineManager : MonoBehaviour
{
    [Header("Actions")]
    public NDec_Act act;

    [Header("Zone Compatment")]
    [SerializeField] private CityFunnel cityFunnel;

    [Header("Day 1 Goals")]
    [SerializeField] private List<float> cityValuesD1;
    [SerializeField] private float startCountdownDay1 = 15f;

    [Header("Day 2 Goals")]
    [SerializeField] private List<float> cityValuesD2;

    [Header("Day 3 Goals")]
    [SerializeField] private List<float> cityValuesD3;

    [Header("Day 4 Goals")]
    [SerializeField] private List<float> cityValuesD4;

    [Header("Day 5 Goals")]
    [SerializeField] private List<float> cityValuesD5;

    [Header("UpdateGoals")]
    [SerializeField] private TextMeshProUGUI TMP_text;

    [Header("Completion Flags")]
    [HideInInspector]
    bool completedDay1;
    bool completedDay2;
    bool completedDay3;
    bool completedDay4;

    List<float> countdownList = new List<float>();
    private float currentCountdown;

    [Header("Minigames States")]
    public bool radarCompleted = false;
    public bool codeCompleted = false;
    public bool temperatureCompleted = false;

    [Header("EventDispatcher")]
    bool onCountdown = false;
    public static Action<float,float, int> RadarDispatch; // TimeToEvent, EventTime, QuantityOfObj

    [Header("EventDispatcher")]
    public static Action<float, float> WaveDispatch;
    private bool waveEventLaunched = false;

    //[Header("EventTimers")]
    //public float timeToRadar = 7.5f;

    private void Start()
    {
        currentCountdown = 0;
        countdownList.Add(startCountdownDay1);
    }

    void Update()
    {
        if (DayTime.Instance.currentHour >= 15 && !waveEventLaunched)
        {
            waveEventLaunched = true;
            WaveDispatch?.Invoke(0f, 60f);
        }

        if (onCountdown) 
        {
            UpdateCountdown();
        }

        if (DayTime.Instance.currentHour >= 15)
            if (IsDayGoalCompleted())
                OnActionExecuted();
    }

    private void OnEnable()
    {
        GameManager.NewDay += UpdateText;
        GameManager.NewDay += ResetWaveEvent;
        PresiSys.OnDialogueTerminated += CheckTerminatedDialogue;
        MiniGameTest.OnMachineEntered += MachineEntered;
        UpdateText();
    }

    private void OnDisable()
    {
        GameManager.NewDay -= UpdateText;
        GameManager.NewDay -= ResetWaveEvent;
        MiniGameTest.OnMachineEntered -= MachineEntered;
        PresiSys.OnDialogueTerminated -= CheckTerminatedDialogue;
    }

    private void UpdateText()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                TMP_text.text = 
                    "Zona Urbana: " + cityValuesD1[0] +"\n"+
                    "Escuela: " + cityValuesD1[1] + "\n" +
                    "Hospital: " + cityValuesD1[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD1[3];
                break;
            case Day.Day2:
                TMP_text.text =
                    "Zona Urbana: " + cityValuesD2[0] + "\n" +
                    "Escuela: " + cityValuesD2[1] + "\n" +
                    "Hospital: " + cityValuesD2[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD2[3];
                break;
            case Day.Day3:
                TMP_text.text =
                    "Zona Urbana: " + cityValuesD3[0] + "\n" +
                    "Escuela: " + cityValuesD3[1] + "\n" +
                    "Hospital: " + cityValuesD3[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD3[3];
                break;
            case Day.Day4:
                TMP_text.text =
                    "Zona Urbana: " + cityValuesD4[0] + "\n" +
                    "Escuela: " + cityValuesD4[1] + "\n" +
                    "Hospital: " + cityValuesD4[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD4[3];
                break;
            case Day.Day5:
                TMP_text.text =
                    "Zona Urbana: " + cityValuesD5[0] + "\n" +
                    "Escuela: " + cityValuesD5[1] + "\n" +
                    "Hospital: " + cityValuesD5[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD5[3];
                break;
            default:
                break;
        }
    }

    private void ResetWaveEvent()
    {
        waveEventLaunched = false;
    }

    public bool IsDayGoalCompleted()
    {
        var values = GetCurrentDayValues();

        for (int i = 0; i < cityFunnel.GetCityZones().Count; i++)
        {
            if (cityFunnel.GetCityZones()[i].CurrentCapacity < values[i])
            {
                return false;
            }
        }

        return true;
    }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
    }

    private void MachineEntered(bool entered) 
    {
        if (entered)
        {
            onCountdown = true;
        }
        else 
        {
            ResetCountdowns();
            onCountdown = false;
        }
    }

    void UpdateCountdown() 
    {
        if (GameManager.Instance.currentDay == Day.Day1 && !completedDay1) 
        {
            currentCountdown += Time.deltaTime;

            if (currentCountdown >= startCountdownDay1)
            {
                GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.MachineOmitedDay1);
                GameManager.Instance.presiSpeaker.PresiSpeaks();
                ResetCountdowns();
                onCountdown = false;
            }
        }
    }

    void ResetCountdowns()
    {
        for (int i = 0; i < countdownList.Count; i++)
        {
            countdownList[i] = 0f;
        }
    }

    void CheckTerminatedDialogue() 
    {
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay1) 
        {
            RadarDispatch?.Invoke(5, 60, 3);
        }
    }

    private List<float> GetCurrentDayValues()
    {
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1: return cityValuesD1;
            case Day.Day2: return cityValuesD2;
            case Day.Day3: return cityValuesD3;
            case Day.Day4: return cityValuesD4;
            case Day.Day5: return cityValuesD5;
            default: return cityValuesD1;
        }
    }
}