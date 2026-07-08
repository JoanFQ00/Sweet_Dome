using FMODUnity;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class MachineManager : MonoBehaviour
{
    [Header("Actions")]
    public NDec_Act act;

    [Header("Zone Compatment")]
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

    private MachineStoryNode currentNode = MachineStoryNode.NODE_01_D1;

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

        //DEBUG DAY 4
        //if (Input.GetKeyDown(KeyCode.Q))
        //    dayManager.D53 = true;
        //if (Input.GetKeyDown(KeyCode.R))
        //    dayManager.D52 = true;
        //if (Input.GetKeyDown(KeyCode.J))
        //    dayManager.D51 = true;

        //DEBUG DAY 5
        if (Input.GetKeyDown(KeyCode.Q))
            dayManager.D84 = true;
        if (Input.GetKeyDown(KeyCode.R))
            dayManager.D81 = true;

        if (Input.GetKeyDown(KeyCode.V))
            waveMinigame.StartWaveEvent(20f);
        
        if (Input.GetKeyDown(KeyCode.F))
            GameManager.Instance.ExecuteFinishAnimation(13, 3);

        if (DayTime.Instance.currentHour >= 21)
            return;

        if (onCountdown)
        {
            UpdateCountdown();
        }

        //if (DayTime.Instance.currentHour >= 15)
        //{
        //    CheckCompletedToday();
        //}
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

    //void CheckCompletedToday()
    //{
    //    if (GameManager.Instance.currentDay == Day.Day1)
    //    {

    //    }
    //}

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
        //Debug.LogWarning("HOLA");
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

    public enum MachineStoryNode
    {
        //DAY1
        NODE_01_D1 = 0,
        NODE_02_D1,
        NODE_03_D1,
        NODE_04_D1,

        //DAY2
        NODE_01_D2,
        NODE_02_D2,

        //DAY3
        NODE_01_D3,
        NODE_02_D3,
        NODE_03_D3,
        NODE_04_D3,
        NODE_05_D3,
        NODE_06_D3,
        NODE_07_D3,
        NODE_08_D3,

        //DAY4
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

        //DAY5
        NODE_01_D5,
        NODE_02_D5,
        NODE_03_D5,
    }

    IEnumerator CountdownRoutine(float time)
    {
        yield return new WaitForSeconds(time);
        onCountdown = true;
    }

    void UpdateCountdown()
    {
        //Debug.Log(IsCityStable());
        switch (GameManager.Instance.currentDay)
        {
            case Day.Day1:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay1Presi); //07
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ForcesDay1Dorothy); //08
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D1;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsAgainDay1Presi); //11
                            ResetCountdowns();
                            onCountdown = false;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryFunDay1Presi); //10
                            ResetCountdowns();
                            onCountdown = false;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        break;

                    case MachineStoryNode.NODE_03_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay1Presi); //07
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryFunDay1Presi); //09
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D1;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        break;

                    case MachineStoryNode.NODE_04_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsAgainDay1Presi); //11
                            ResetCountdowns();
                            onCountdown = false;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryAngryDay1Presi); //12
                            ResetCountdowns();
                            onCountdown = false;
                            cityFunnel.SetGlobalEnergyFlowPause(true);
                        }
                        break;

                    default:
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
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CrackAtSchoolDay2Presi); //28
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D1:
                        if (currentCountdown >= 90f)
                        {
                            cityFunnel.SetGlobalEnergyFlowPause(true);

                            if (IsCityStable())
                            {
                                if (priorityManager.HasUsedPriorityToday())
                                {
                                    if (priorityManager.GetIdxPrioritizedZone() == 1)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodHappySchoolDay2Dorothy); //32
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodSadDay2Dorothy); //33
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                }
                                else
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoodSadDay2Dorothy); //33
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

                                        if (onlyFailureZone == 0) //R2
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Dorothy); //30
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else //R3
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryDay2Dorothy); //31
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    default: //R1
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.VeryAngryDay2Dorothy); //29
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }
                break;

            case Day.Day3:
                currentCountdown += Time.deltaTime;

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D3:
                        if (currentCountdown >= 20f)
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
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FirstCongratsDay3Presi); //50
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D3;
                        }
                        else
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FirstAngryDay3Presi); //51
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D3:
                        if (currentCountdown >= 20f)
                        {
                            Shake.Instance.TriggerShake(1f);
                            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZPriorDay3Presi); //52
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_04_D3:
                        if (currentCountdown >= 20f)
                        {
                            radarMinigame.StartRadarEvent(30f, RadarEventType.SpaceShip, 2);
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_05_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_05_D3:
                        Shake.Instance.TriggerShake(2f);
                        cityFunnel.DrainLiquidFromZoneImmediate(2, 20f);
                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.HDownDay3Dorothy); //54
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
                            currentNode = MachineStoryNode.NODE_08_D3;
                        }
                        else //R5
                        {
                            cityFunnel.ChangeLiquidInZone(0, 0f, 2f);
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SecondAngryDay3Presi); //55
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_08_D3;
                        }
                        break;

                    case MachineStoryNode.NODE_08_D3:
                        if (currentCountdown >= 50f)
                        {
                            cityFunnel.SetGlobalEnergyFlowPause(true);

                            if (IsCityStable()) //R11 & D42
                            {
                                dayManager.D42 = true;

                                if (priorityManager.HasUsedPriorityToday())
                                {
                                    if (priorityManager.GetIdxPrioritizedZone() == 2)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ThxDay3Dorothy); //60
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else if (priorityManager.GetIdxPrioritizedZone() == 0)
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SadDay3Dorothy); //61
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                }
                                else
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SadDay3Dorothy); //61
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

                                        if (onlyFailureZone == 0) //R8
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZ0Day3Dorothy); //58
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else if (onlyFailureZone == 2) //R9
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.H0Day3Dorothy); //59
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else //R10
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    case 2:
                                        if (failedZones.Contains(0) && failedZones.Contains(2)) //R7
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.Two0AngryDay3Dorothy); //57
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        else //R10
                                        {
                                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
                                            ResetCountdowns();
                                            onCountdown = false;
                                        }
                                        break;

                                    case 4: //R6
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.All0AngryhDay3Dorothy); //56
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;

                                    default: //R10
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
                                        ResetCountdowns();
                                        onCountdown = false;
                                        break;
                                }
                            }
                        }
                        break;

                    default:
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
                            //onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D4:
                        if (currentCountdown >= 40f)
                        {
                            cityFunnel.TriggerCrackInZone(0, 40f, 3f);
                            ResetCountdowns();
                            //onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D4:
                        if (currentCountdown >= 40f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D3ay4); //81
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D4;
                        }
                        break;

                    case MachineStoryNode.NODE_04_D4:
                        if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D4ay4); //82
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
                        //else R12

                        waveMinigame.StartWaveEvent(50f); //!
                        ResetCountdowns();
                        onCountdown = false;
                        currentNode = MachineStoryNode.NODE_09_D4;
                        break;

                    case MachineStoryNode.NODE_09_D4:
                        if (waveCompleted) //R13
                        {
                            dayManager.D61 = true;
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D7ay4); //85
                        }
                        else
                        {
                            dayManager.D62 = true;
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D8ay4); //86
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
                        cityFunnel.SetGlobalEnergyFlowPause(true);

                        if (IsCityStable()) //R14 & D72
                        {
                            dayManager.D72 = true;

                            if (priorityManager.HasUsedPriorityToday())
                            {
                                if (priorityManager.GetIdxPrioritizedZone() == 1)
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D15Day4); //93
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                                else if (priorityManager.GetIdxPrioritizedZone() == 3)
                                {
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D16Day4); //94
                                    ResetCountdowns();
                                    onCountdown = false;
                                }
                            }
                            else
                            {
                                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D16Day4); //94
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

                                    if (onlyFailureZone == 1) //R18
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D14Day4); //92
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else if (onlyFailureZone == 3) //R17
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D13Day4); //91
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else //R19
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    break;

                                case 2:
                                    if (failedZones.Contains(1) && failedZones.Contains(3)) //R16
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D12ay4); //90
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    else //R19
                                    {
                                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
                                        ResetCountdowns();
                                        onCountdown = false;
                                    }
                                    break;

                                case 4: //R15
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D11ay4); //89
                                    ResetCountdowns();
                                    onCountdown = false;
                                    break;

                                default: //R19
                                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
                                    ResetCountdowns();
                                    onCountdown = false;
                                    break;
                            }
                        }
                        break;

                    default:
                        break;
                }
                break;

            case Day.Day5:
                currentCountdown += Time.deltaTime;
                cityFunnel.SetGlobalEnergyFlowPause(true); //!

                switch (currentNode)
                {
                    case MachineStoryNode.NODE_01_D5:
                        if (dayManager.D84)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D7ay5); //122
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D0ay5); //115
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;

                    case MachineStoryNode.NODE_02_D5:
                        if (dayManager.D81 && dayManager.hasReadDorothyBook4.IsMet(GameManager.Instance.decManager) && dayManager.hasReadDorothyBook3.IsMet(GameManager.Instance.decManager) && dayManager.dorothyBook1Readed.IsMet(GameManager.Instance.decManager) && dayManager.dorothyBook2Readed.IsMet(GameManager.Instance.decManager))
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D1ay5); //116
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_03_D5;
                        }
                        else if (priorityManager.hasBeenPressedDay5)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D3ay5); //118
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (currentCountdown >= 30f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D2ay5); //117
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;

                    case MachineStoryNode.NODE_03_D5:
                        if (currentCountdown >= 30f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D2ay5); //117
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (codeSystem.isSecretCodeCorrectDay5)
                        {
                            codeSystem.ForceDisable();
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D4ay5); //119
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (priorityManager.hasBeenPressedDay5)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D3ay5); //118
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;
                }
                break;

            default:
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

        countdownList.Clear();
        countdownList.Add(FirstCDDay1);

        cityFunnel.SetGlobalEnergyFlowPause(true);
        WaveDispatch?.Invoke(0f, 60f);
    }

    void CheckTerminatedDialogue()
    {
        //DAY1
        //N00
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay1) //06
        {
            currentNode = MachineStoryNode.NODE_01_D1;
            onCountdown = true;
        }

        //N01
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsDay1Presi) //07
        {
            Shake.Instance.TriggerShake(1f);
            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
            cityFunnel.SetGlobalEnergyFlowPause(false);
            cityFunnel.ChangeLiquidInZone(0, cityValuesD1[0] - 10f, 1f);
            StartCoroutine(CountdownRoutine(1.1f));
        }

        //N01
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ForcesDay1Dorothy) //08
        {
            onCountdown = true;
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryFunDay1Presi) //10
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi); //13
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsAgainDay1Presi) //11
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi); //13
        }

        //N03
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryFunDay1Presi) //09
        {
            Shake.Instance.TriggerShake(1f);
            cityFunnel.TriggerCrackInZone(0, 20f, 3f);
            cityFunnel.SetGlobalEnergyFlowPause(false);
            cityFunnel.ChangeLiquidInZone(0, cityValuesD1[0] - 10f, 1f);
            StartCoroutine(CountdownRoutine(1.1f));
        }

        //N04
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryAngryDay1Presi) //12
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi); //13
        }

        //FINAL DAY1
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay1Presi) //13
        {
            FinishWorkingDay();
        }


        //DAY2
        //N00
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay2) //26
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.PleaseSchoolDay2Presi); //27
        }

        //N00
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.PleaseSchoolDay2Presi) //27
        {
            currentNode = MachineStoryNode.NODE_01_D2;
            onCountdown = true;
        }

        //N01
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CrackAtSchoolDay2Presi) //28
        {
            Shake.Instance.TriggerShake(1f);
            cityFunnel.DrainLiquidFromZoneImmediate(1, 30f);
            onCountdown = true;
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.GoodHappySchoolDay2Dorothy) //32
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay2Presi); //36
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.GoodSadDay2Dorothy) //32
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay2Presi); //36
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.CongratsDay2Presi) //32
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay2Presi); //37
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryDay2Dorothy) //30
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Presi); //35
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryDay2Dorothy) //31
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryDay2Presi); //35
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SemiAngryDay2Presi) //35
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay2Presi); //37
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.VeryAngryDay2Dorothy) //29
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.BadFinalDay2Presi); //34
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.BadFinalDay2Presi) //34
        {
            //FINAL 5
            GameManager.Instance.ExecuteFinishAnimation(15, 2);
        }

        //FINAL DAY2
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay2Presi) //37
        {
            FinishWorkingDay();
        }

        //DAY3
        //N00
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay3) //48
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.LyingToMatthewDay3Dorothy); //49
        }

        //N01
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.LyingToMatthewDay3Dorothy) //49
        {
            currentNode = MachineStoryNode.NODE_01_D3;
            onCountdown = true;
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FirstCongratsDay3Presi) //50
        {
            onCountdown = true;
        }

        //N02
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FirstAngryDay3Presi) //51
        {
            onCountdown = true;
        }

        //N03
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZPriorDay3Presi) //52
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.HPriorDay3Dorothy); //53
        }

        //N03
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.HPriorDay3Dorothy) //53
        {
            onCountdown = true;
        }

        //N05
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.HDownDay3Dorothy) //54
        {
            onCountdown = true;
        }

        //N07
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SecondAngryDay3Presi) //55
        {
            onCountdown = true;
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ThxDay3Dorothy) //60
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.FinalCongratsDay3Presi); //66
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SadDay3Dorothy) //61
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.VeryAngryDay2Dorothy); //66
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.FinalCongratsDay3Presi) //66
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.Two0AngryDay3Dorothy) //57
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.Two0Day3Presi); //63
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.Two0Day3Presi) //63
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZ0Day3Dorothy) //58
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.UZ0Day3Presi); //64
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.UZ0Day3Presi) //64
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.H0Day3Dorothy) //59
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.H0Day3Presi); //65
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.H0Day3Presi) //65
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay3Presi); //67
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.All0AngryhDay3Dorothy) //56
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.F5Day3Presi); //62
        }

        //N08
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.F5Day3Presi) //62
        {
            //FINAL 5
            GameManager.Instance.ExecuteFinishAnimation(15, 2);
        }

        //FINAL DAY3
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay3Presi) //67
        {
            FinishWorkingDay();
        }

        //DAY4
        //N00
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4) //77
        {
            if (dayManager.D53 || dayManager.D52)
            {
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D0ay4); //78
            }
            else
            {
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D1ay4); //79
            }

            currentNode = MachineStoryNode.NODE_01_D4;
            onCountdown = true;
        }

        //if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4) //78
        //{
        //    currentNode = MachineStoryNode.NODE_01_D4;
        //    onCountdown = true;
        //}

        //if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay4) //79
        //{
        //    currentNode = MachineStoryNode.NODE_01_D4;
        //    onCountdown = true;
        //}

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D3ay4) //81
        {
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D4ay4) //82
        {
            cityValuesD4[0] = 80f;
            TMP_text.text =
                    "Zona Urbana: " + cityValuesD4[0] + "\n" +
                    "Escuela: " + cityValuesD4[1] + "\n" +
                    "Hospital: " + cityValuesD4[2] + "\n" +
                    "Residencia Presidencial: " + cityValuesD4[3];

            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D7ay4) //85
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D9ay4); //87
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D9ay4) //87
        {
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D8ay4) //86
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D10ay4); //88
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D10ay4) //88
        {
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D11ay4) //89
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D17Day4); //95
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D17Day4) //95
        {
            //FINAL 5
            GameManager.Instance.ExecuteFinishAnimation(15, 2);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D12ay4) //90
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D18Day4); //96
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D18Day4) //96
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D13Day4) //91
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D19Day4); //97
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D19Day4) //97
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D14Day4) //92
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D20Day4); //98
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D20Day4) //98
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D15Day4) //93
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D21Day4); //99
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D21Day4) //99
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D16Day4) //94
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D22Day4); //100
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D22Day4) //100
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D23Day4); //101
        }

        //FINAL DAY4
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D23Day4) //101
        {
            FinishWorkingDay();
        }

        ////DAY5
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D7ay5) //122
        {
            //FINAL 4
            GameManager.Instance.ExecuteFinishAnimation(14, 3);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D0ay5) //115
        {
            priorityManager.isDay5CrisisActive = true;
            currentNode = MachineStoryNode.NODE_02_D5;
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D1ay5) //116
        {
            codeSystem.EnableCodeSystem(null);
            onCountdown = true;
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D2ay5) //117
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D5ay5); //120
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D3ay5) //118
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.D6ay5); //121
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D6ay5) //121
        {
            //FINAL 3
            Shake.Instance.TriggerShake(3f);
            GameManager.Instance.ExecuteFinishAnimation(13, 5);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D5ay5) //120
        {
            //FINAL 2
            GameManager.Instance.ExecuteFinishAnimation(12, 3);
        }

        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.D4ay5) //119
        {
            //FINAL 1
            Shake.Instance.TriggerShake(3f);
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

        MiniGameTest sceneChair = UnityEngine.Object.FindAnyObjectByType<MiniGameTest>();
        if (sceneChair != null)
        {
            sceneChair.ForceExitMiniGameFromSystem();
        }

        OnFinishedDay?.Invoke("GoToSleep");
    }

    private void RadarFinished(bool exit)
    {
        radarResult = exit;
        onCountdown = true;
    }

    private void WaveFinished(bool success)
    {
        waveCompleted = success;
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
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay1); //06
        }

        else if (GameManager.Instance.currentDay == Day.Day2)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay2); //26
        }

        else if (GameManager.Instance.currentDay == Day.Day3)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay3); //48
        }

        else if (GameManager.Instance.currentDay == Day.Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay4); //77
        }

        else if (GameManager.Instance.currentDay == Day.Day5)
        {
            currentNode = MachineStoryNode.NODE_01_D5;
            codeSystem.ForceDisable();
            cityFunnel.SetGlobalEnergyFlowPause(true);
            onCountdown = true;
        }
    }
}