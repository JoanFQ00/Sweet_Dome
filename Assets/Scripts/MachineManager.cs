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

    [Header("Zone Compartment")]
    [SerializeField] private CityFunnel cityFunnel;

    [Header("Day 1 Goals")]
    [SerializeField] List<float> cityValuesD1;
    [SerializeField] float FirstCDDay1 = 20f;
    [SerializeField] private float secondCountDay1 = 20f;

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
    [HideInInspector] bool completedDay1;
    [HideInInspector] bool completedDay2;
    [HideInInspector] bool completedDay3;
    [HideInInspector] bool completedDay4;

    List<float> countdownList = new List<float>();
    [SerializeField] private float currentCountdown;

    [Header("Minigames States")]
    public bool radarCompleted = false;
    public bool codeCompleted = false;
    public bool temperatureCompleted = false;

    [Header("EventDispatcher")]
    bool onCountdown = false;
    public static Action<float, float, int> RadarDispatch;

    [Header("EventDispatcher")]
    public static Action<float, float> WaveDispatch;
    private bool waveEventLaunched = false;

    public bool waveCompleted = false;
    public static event Action<string> OnFinishedDay;
    bool radarResult = false;

    DayManager dayManager;
    [SerializeField] WaveWinner waveMinigame;
    [SerializeField] CityPriorityManager priorityManager;
    [SerializeField] RadarMinigame radarMinigame;
    [SerializeField] CodeSystem codeSystem;

    private void Start()
    {
        currentCountdown = 0;
        countdownList.Add(FirstCDDay1);
        OnNewDayReset();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (DayTime.Instance != null)
            {
                DayTime.Instance.SetCurrentTime(15, 0);
            }
            MiniGameTest.OnMachineEntered?.Invoke(true);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            FinishWorkingDay();
            return;
        }

        if (DayTime.Instance.currentHour >= 21)
            return;

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (radarMinigame != null)
            {
                radarMinigame.StartRadarEvent(30f, RadarEventType.SpaceShip, 3);
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (radarMinigame != null)
            {
                radarMinigame.StartRadarEvent(30f, RadarEventType.Meteorite, 3);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (waveMinigame != null)
            {
                waveMinigame.StartWaveEvent(40f);
            }
        }
    }

    private void OnEnable()
    {
        GameManager.NewDay += UpdateText;
        GameManager.NewDay += OnNewDayReset;
        PresiSys.OnDialogueTerminated += CheckTerminatedDialogue;
        MiniGameTest.OnMachineEntered += MachineEntered;
        RadarMinigame.OnEventFinished += RadarFinished;
        WaveWinner.OnEventFinished += WaveFinished;
        LevelManager.OnLevelLoaded += LevelLoaded;

        UpdateText();
    }

    private void OnDisable()
    {
        GameManager.NewDay -= UpdateText;
        GameManager.NewDay -= ResetWaveEvent;
        MiniGameTest.OnMachineEntered -= MachineEntered;
        PresiSys.OnDialogueTerminated -= CheckTerminatedDialogue;
        RadarMinigame.OnEventFinished -= RadarFinished;
        WaveWinner.OnEventFinished -= WaveFinished;
        LevelManager.OnLevelLoaded -= LevelLoaded;
    }

    private void LevelLoaded()
    {
        dayManager = GameManager.Instance.dayManager;
    }

    private void UpdateText()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                TMP_text.text =
                    "Zona Urbana: " + cityValuesD1[0] + "\n" +
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
        }
    }

    private void ResetWaveEvent()
    {
        waveEventLaunched = false;
    }

    void CheckCompletedToday()
    {
    }

    bool IsCityStable()
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
            BeginDayInstructions();
            cityFunnel.SetGlobalEnergyFlowPause(false);
        }
        else
        {
            currentCountdown = 0f;
            onCountdown = false;
            cityFunnel.SetGlobalEnergyFlowPause(true);
        }
    }

    void ResetCountdowns()
    {
        currentCountdown = 0f;
        for (int i = 0; i < countdownList.Count; i++)
        {
            countdownList[i] = 0f;
        }
    }

    private void OnNewDayReset()
    {
        currentCountdown = 0f;
        onCountdown = false;

        countdownList.Clear();
        countdownList.Add(FirstCDDay1);

        cityFunnel.SetGlobalEnergyFlowPause(true);
        WaveDispatch?.Invoke(0f, 60f);
    }

    void CheckTerminatedDialogue()
    {
    }

    public void FinishWorkingDay()
    {
        onCountdown = false;
        currentCountdown = 0f;

        if (cityFunnel != null)
        {
            cityFunnel.SetGlobalEnergyFlowPause(true);
        }

        if (DayTime.Instance != null)
        {
            DayTime.Instance.SetCurrentTime(21, 0);
        }

        MiniGameTest.OnMachineEntered?.Invoke(false);
        OnFinishedDay?.Invoke("GoToSleep");
    }

    private void RadarFinished(bool exit)
    {
        radarResult = exit;
    }

    private void WaveFinished(bool success)
    {
        waveCompleted = success;
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

    void BeginDayInstructions()
    {
    }
}