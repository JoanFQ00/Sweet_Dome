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
    [HideInInspector]
    bool completedDay1;
    bool completedDay2;
    bool completedDay3;
    bool completedDay4;

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

    public enum MachineStoryNode
    {
        // DAY 1
        NODE_01_D1 = 0,
        NODE_02_D1,
        NODE_03_D1,
        NODE_04_D1,

        // DAY 2
        NODE_01_D2,
        NODE_02_D2,

        // DAY 3
        NODE_01_D3,
        NODE_02_D3,
        NODE_03_D3,
        NODE_04_D3,
        NODE_05_D3,
        NODE_06_D3,
        NODE_07_D3,
        NODE_08_D3,

        // DAY 4
        NODE_01_D4,
        NODE_02_D4,
        NODE_03_D4,
        NODE_04_D4,
        NODE_05_D4,
        NODE_06_D4,
        NODE_07_D4,
        NODE_08_D4,
        NODE_09_D4,
        NODE_10_D4,
        NODE_11_D4,

        // DAY 5
        NODE_01_D5,
        NODE_02_D5,
        NODE_03_D5,
    }

    private MachineStoryNode currentNode = MachineStoryNode.NODE_01_D1;

    private bool justChangedNode = false;

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
        if (Input.GetKeyDown(KeyCode.M))
            cityFunnel.SetGlobalEnergyFlowPause(false);

        if (Input.GetKeyDown(KeyCode.N))
            cityFunnel.SetGlobalEnergyFlowPause(true);

        if (Input.GetKeyDown(KeyCode.Y))
        {
            FinishWorkingDay();
        }

        if (DayTime.Instance.currentHour >= 21)
            return;

        if (onCountdown)
        {
            UpdateCountdown();
        }

        if (DayTime.Instance.currentHour >= 15)
        {
            CheckCompletedToday();
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
            default:
                break;
        }
    }

    private void ResetWaveEvent()
    {
        waveEventLaunched = false;
    }

    void CheckCompletedToday()
    {
        if (GameManager.Instance.currentDay == Day.Day1)
        {

        }
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

    void UpdateCountdown()
    {
        if (justChangedNode)
        {
            justChangedNode = false;
            return;
        }

        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ForcesDay1Dorothy);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D1;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsAgainDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryFunDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryFunDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D1;
                        }
                        break;

                    case MachineStoryNode.NODE_04_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsAgainDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryAngryDay1Presi);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;
                }
                break;

            case Day.Day2:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D2:
                        if (currentCountdown >= 30f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CrackAtSchoolDay2Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D1:
                        if (currentCountdown >= 90f)
                        {
                            if (IsCityStable())
                            {
                                if (priorityManager.HasUsedPriorityToday())
                                {
                                    if (priorityManager.GetIdxPrioritizedZone() == 1)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodHappySchoolDay2Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodSadDay2Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                }
                                else
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodSadDay2Dorothy);
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                            }
                            else
                            {
                                List<float> requiredValues = GetCurrentDayValues();
                                var cityZones = cityFunnel.GetCityZones();
                                List<int> failedZones = new List<int>();

                                for (int i = 0; i < cityZones.Count; i++)
                                {
                                    if (cityZones[i].CurrentCapacity < requiredValues[i])
                                    {
                                        failedZones.Add(i);
                                    }
                                }

                                int totalFailures = failedZones.Count;

                                switch (totalFailures)
                                {
                                    case 1:
                                        int onlyFailureZone = failedZones[0];

                                        if (onlyFailureZone == 0)
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Dorothy);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryDay2Dorothy);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    default:
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.VeryAngryDay2Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;
                                }
                            }
                        }
                        break;
                }
                break;

            case Day.Day3:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D3:
                        if (currentCountdown >= 10f)
                        {
                            radarMinigame.StartRadarEvent(30f, RadarEventType.SpaceShip, 1);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D3:
                        if (radarResult)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FirstCongratsDay3Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D3;
                        }
                        else
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FirstAngryDay3Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D3:
                        if (currentCountdown >= 10f)
                        {
                            Shake.Instance.TriggerShake(1f);
                            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZPriorDay3Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_04_D3:
                        if (currentCountdown >= 10f)
                        {
                            radarMinigame.StartRadarEvent(30f, RadarEventType.SpaceShip, 2);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_05_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_05_D3:
                        Shake.Instance.TriggerShake(2f);
                        cityFunnel.DrainLiquidFromZoneImmediate(2, 40f);
                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.HDownDay3Dorothy);
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_06_D3;
                        break;

                    case MachineStoryNode.NODE_06_D3:
                        if (currentCountdown >= 40f)
                        {
                            radarMinigame.StartRadarEvent(60f, RadarEventType.Meteorite, 3);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_07_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_07_D3:
                        if (radarResult)
                        {
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_08_D3;
                        }
                        else
                        {
                            cityFunnel.ChangeLiquidInZone(0, 0f, 2f);
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SecondAngryDay3Presi);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_08_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_08_D3:
                        if (currentCountdown >= 30f)
                        {
                            if (IsCityStable())
                            {
                                dayManager.D42 = true;

                                if (priorityManager.HasUsedPriorityToday())
                                {
                                    if (priorityManager.GetIdxPrioritizedZone() == 2)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ThxDay3Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else if (priorityManager.GetIdxPrioritizedZone() == 0)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SadDay3Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                }
                                else
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SadDay3Dorothy);
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                            }
                            else
                            {
                                dayManager.D41 = true;
                                List<float> requiredValues = GetCurrentDayValues();
                                var cityZones = cityFunnel.GetCityZones();
                                List<int> failedZones = new List<int>();

                                for (int i = 0; i < cityZones.Count; i++)
                                {
                                    if (cityZones[i].CurrentCapacity < requiredValues[i])
                                    {
                                        failedZones.Add(i);
                                    }
                                }

                                int totalFailures = failedZones.Count;

                                switch (totalFailures)
                                {
                                    case 1:
                                        int onlyFailureZone = failedZones[0];

                                        if (onlyFailureZone == 0)
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZ0Day3Dorothy);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else if (onlyFailureZone == 2)
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.H0Day3Dorothy);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    case 2:
                                        if (failedZones.Contains(0) && failedZones.Contains(2))
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.Two0AngryDay3Dorothy);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    case 4:
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.All0AngryhDay3Dorothy);
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;

                                    default:
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;
                                }
                            }
                        }
                        break;
                }
                break;

            case Day.Day4:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D4:
                        if (currentCountdown >= 20f)
                        {
                            waveMinigame.StartWaveEvent(20f);
                            ResetCountdowns();
                            currentNode = MachineStoryNode.NODE_02_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D4:
                        if (currentCountdown >= 40f)
                        {
                            cityFunnel.TriggerCrackInZone(0, 40f, 3f);
                            ResetCountdowns();
                            currentNode = MachineStoryNode.NODE_03_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D4:
                        if (currentCountdown >= 40f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D3ay4);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_04_D4:
                        if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D4ay4);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_05_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_05_D4:
                        if (currentCountdown >= 20f)
                        {
                            cityFunnel.DrainLiquidFromZoneImmediate(3, 40f);
                            radarMinigame.StartRadarEvent(120f, RadarEventType.SpaceShip, 2);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_06_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_06_D4:
                        waveMinigame.StartWaveEvent(30f);
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_07_D4;
                        break;

                    case MachineStoryNode.NODE_07_D4:
                        cityFunnel.TriggerCrackInZone(1, 30f, 3f);
                        radarMinigame.StartRadarEvent(50f, RadarEventType.Meteorite, 4);
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_08_D4;
                        break;

                    case MachineStoryNode.NODE_08_D4:
                        if (!radarResult)
                        {
                            cityFunnel.ChangeLiquidInZone(0, 0f, 2f);
                        }
                        waveMinigame.StartWaveEvent(50f);
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_09_D4;
                        break;

                    case MachineStoryNode.NODE_09_D4:
                        if (waveCompleted)
                        {
                            dayManager.D61 = true;
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D7ay4);
                        }
                        else
                        {
                            dayManager.D62 = true;
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D8ay4);
                        }
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_10_D4;
                        break;

                    case MachineStoryNode.NODE_10_D4:
                        if (currentCountdown >= 20f)
                        {
                            radarMinigame.StartRadarEvent(90f, RadarEventType.SpaceShip, 3);
                            cityFunnel.DrainLiquidFromZoneImmediate(2, 20f);
                            cityFunnel.TriggerCrackInZone(3, 50f, 3f);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_11_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_11_D4:
                        if (IsCityStable())
                        {
                            dayManager.D72 = true;

                            if (priorityManager.HasUsedPriorityToday())
                            {
                                if (priorityManager.GetIdxPrioritizedZone() == 1)
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D15Day4);
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                                else if (priorityManager.GetIdxPrioritizedZone() == 3)
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D16Day4);
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                            }
                            else
                            {
                                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D16Day4);
                                ResetCountdowns();
                                onCountdown = false;
                            }
                        }
                        else
                        {
                            dayManager.D71 = true;
                            List<float> requiredValues = GetCurrentDayValues();
                            var cityZones = cityFunnel.GetCityZones();
                            List<int> failedZones = new List<int>();

                            for (int i = 0; i < cityZones.Count; i++)
                            {
                                if (cityZones[i].CurrentCapacity < requiredValues[i])
                                {
                                    failedZones.Add(i);
                                }
                            }

                            int totalFailures = failedZones.Count;

                            switch (totalFailures)
                            {
                                case 1:
                                    int onlyFailureZone = failedZones[0];

                                    if (onlyFailureZone == 1)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D14Day4);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else if (onlyFailureZone == 3)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D13Day4);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    break;

                                case 2:
                                    if (failedZones.Contains(1) && failedZones.Contains(3))
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D12ay4);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    break;

                                case 4:
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D11ay4);
                                    ResetCountdowns();
                                    onCountdown = false;
                                    break;

                                default:
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
                                    ResetCountdowns();
                                    onCountdown = false;
                                    break;
                            }
                        }
                        break;
                }
                break;

            case Day.Day5:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D5:
                        if (dayManager.D84)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D7ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D0ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D5:
                        if (dayManager.D81 && dayManager.hasReadDorothyBook4.IsMet(GameManager.Instance.decManager) && dayManager.hasReadDorothyBook3.IsMet(GameManager.Instance.decManager) && dayManager.dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && dayManager.dorothyBook2Readed.IsMet(GameManager.Instance.decManager))
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D1ay5);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D5;
                        }
                        else if (priorityManager.hasBeenPressedDay5)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D3ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (currentCountdown >= 15f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D2ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D5:
                        if (currentCountdown >= 15f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D5ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (codeSystem.isSecretCodeCorrectDay5)
                        {
                            codeSystem.ForceDisable();
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D4ay5);
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;
                }
                break;
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
        justChangedNode = false;

        countdownList.Clear();
        countdownList.Add(FirstCDDay1);

        cityFunnel.SetGlobalEnergyFlowPause(true);
        WaveDispatch?.Invoke(0f, 60f);
    }

    void CheckTerminatedDialogue()
    {
        //DAY1
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay1)
        {
            currentNode = MachineStoryNode.NODE_01_D1;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsDay1Presi)
        {
            Shake.Instance.TriggerShake(1f);
            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ForcesDay1Dorothy)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryFunDay1Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsAgainDay1Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryFunDay1Presi)
        {
            Shake.Instance.TriggerShake(1f);
            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryAngryDay1Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay1Presi)
        {
            FinishWorkingDay();
        }

        //DAY2
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay2)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.PleaseSchoolDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.PleaseSchoolDay2Presi)
        {
            currentNode = MachineStoryNode.NODE_01_D2;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CrackAtSchoolDay2Presi)
        {
            Shake.Instance.TriggerShake(2f);
            cityFunnel.DrainLiquidFromZoneImmediate(1, 30f);
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.GoodHappySchoolDay2Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.GoodSadDay2Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsDay2Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryDay2Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryDay2Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryDay2Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.VeryAngryDay2Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.BadFinalDay2Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.BadFinalDay2Presi)
        {
            GameManager.Instance.ExecuteFinishAnimation(15, 0);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay2Presi)
        {
            FinishWorkingDay();
        }

        //DAY3
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay3)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.LyingToMatthewDay3Dorothy);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.LyingToMatthewDay3Dorothy)
        {
            currentNode = MachineStoryNode.NODE_01_D3;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FirstCongratsDay3Presi)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FirstAngryDay3Presi)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZPriorDay3Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.HPriorDay3Dorothy);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.HPriorDay3Dorothy)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.HDownDay3Dorothy)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SecondAngryDay3Presi)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ThxDay3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FinalCongratsDay3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SadDay3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.VeryAngryDay2Dorothy);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FinalCongratsDay3Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.Two0AngryDay3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.Two0Day3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.Two0Day3Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZ0Day3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZ0Day3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZ0Day3Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.H0Day3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.H0Day3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.H0Day3Presi)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.All0AngryhDay3Dorothy)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.F5Day3Presi);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.F5Day3Presi)
        {
            GameManager.Instance.ExecuteFinishAnimation(15, 0);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay3Presi)
        {
            FinishWorkingDay();
        }

        //DAY4
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4)
        {
            if (dayManager.D53 || dayManager.D52)
            {
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D0ay4);
            }
            else
            {
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D1ay4);
            }
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4)
        {
            currentNode = MachineStoryNode.NODE_01_D4;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4)
        {
            currentNode = MachineStoryNode.NODE_01_D4;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D3ay4)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D4ay4)
        {
            cityValuesD4[0] = 80f;
            UpdateText();
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D7ay4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D9ay4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D9ay4)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D8ay4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D10ay4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D10ay4)
        {
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D11ay4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D17Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D17Day4)
        {
            GameManager.Instance.ExecuteFinishAnimation(15, 0);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D12ay4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D18Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D18Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D13Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D19Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D19Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D14Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D20Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D20Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D15Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D21Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D21Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D16Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D22Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D22Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D23Day4)
        {
            FinishWorkingDay();
        }

        //DAY5
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D7ay5)
        {
            GameManager.Instance.ExecuteFinishAnimation(14, 0);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D0ay5)
        {
            priorityManager.isDay5CrisisActive = true;
            currentNode = MachineStoryNode.NODE_02_D5;
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D1ay5)
        {
            codeSystem.ForceDisable();
            codeSystem.EnableCodeSystem(null);
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D2ay5)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D5ay5);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D3ay5)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D6ay5);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D6ay5)
        {
            //FINAL 3
            Shake.Instance.TriggerShake(5f);
            GameManager.Instance.ExecuteFinishAnimation(13, 5);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D5ay5)
        {
            //FINAL 2
            GameManager.Instance.ExecuteFinishAnimation(12, 0);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D4ay5)
        {
            //FINAL 1
            Shake.Instance.TriggerShake(5f);
            GameManager.Instance.ExecuteFinishAnimation(11, 5);
        }
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
        justChangedNode = true;
        onCountdown = true;
    }

    private void WaveFinished(bool success)
    {
        waveCompleted = success;
        justChangedNode = true;
        onCountdown = true;
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
        if (GameManager.Instance.currentDay == Day.Day1)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay1);
        }
        else if (GameManager.Instance.currentDay == Day.Day2)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay2);
        }
        else if (GameManager.Instance.currentDay == Day.Day3)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay3);
        }
        else if (GameManager.Instance.currentDay == Day.Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay4);
        }
        else if (GameManager.Instance.currentDay == Day.Day5)
        {
            codeSystem.ForceDisable();
            cityFunnel.SetGlobalEnergyFlowPause(true);
            ResetCountdowns();
            justChangedNode = true;
            onCountdown = true;
        }
    }
}