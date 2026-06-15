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
    [SerializeField] List<float> cityValuesD1;
    [SerializeField] float FirstCDDay1 = 20f;
    bool day1CDCompleted;
    bool day2CDCompleted;

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
    public static Action<float,float, int> RadarDispatch; // TimeToEvent, EventTime, QuantityOfObj

    [Header("EventDispatcher")]
    public static Action<float, float> WaveDispatch;
    private bool waveEventLaunched = false;

    //[Header("EventTimers")]
    //public float timeToRadar = 7.5f;

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

        if (DayTime.Instance.currentHour >= 21)
            return;

        //if (DayTime.Instance.currentHour >= 15 && !waveEventLaunched)
        //{
        //    waveEventLaunched = true;
        //    WaveDispatch?.Invoke(0f, 60f);
        //}

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

        UpdateText();
    }

    private void OnDisable()
    {
        GameManager.NewDay -= UpdateText;
        GameManager.NewDay -= ResetWaveEvent;
        MiniGameTest.OnMachineEntered -= MachineEntered;
        PresiSys.OnDialogueTerminated -= CheckTerminatedDialogue;
        RadarMinigame.OnEventFinished -= RadarFinished;
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
            // Despausamos el flujo para que empiece la simulación
            cityFunnel.SetGlobalEnergyFlowPause(false);
        }
        else
        {
            currentCountdown = 0f;
            onCountdown = false;
            // Pausamos el flujo de energía porque el jugador se ha levantado de la silla
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
        NODE_09_D3,
        NODE_10_D3,

        //DAY4

        //DAY5
    }

    private MachineStoryNode currentNode = MachineStoryNode.NODE_01_D1;

    DayManager dayManager;
    CityPriorityManager priorityManager;
    RadarMinigame radarMinigame;

    void UpdateCountdown()
    {
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
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryFunDay1Presi); //10
                            ResetCountdowns();
                            onCountdown = false;
                        }
                    break;

                    case MachineStoryNode.NODE_03_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsDay1Presi); //07
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_02_D1;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SemiAngryFunDay1Presi); //09
                            ResetCountdowns();
                            onCountdown = false;
                            currentNode = MachineStoryNode.NODE_04_D1;
                        }
                    break;

                    case MachineStoryNode.NODE_04_D1:
                        if (IsCityStable())
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.CongratsAgainDay1Presi); //11
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        else if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryAngryDay1Presi); //12
                            ResetCountdowns();
                            onCountdown = false;
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
                                    else if (priorityManager.GetIdxPrioritizedZone() == 0)
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
                                        else if (onlyFailureZone == 1) //R3
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
                        if (currentCountdown >= 10f)
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
                            onCountdown = false;
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
                        if (currentCountdown >= 30f)
                        {
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
            break;

            case Day.Day5:
            break;

            default:
            break;
        }
    }

    void ResetCountdowns()
    {
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
            onCountdown = true;
        }
        
        //N04
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.AngryAngryDay1Presi) //12
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.SleepDay1Presi); //13
        }

        //FINAL DAY1
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay1Presi) //13
        {
            //SE PUEDE LEVANTAR DE LA SILLA, SON LA 21:00
            // <-------------------------------------------------------- JOAN: POSAR CINEMATICA DE RECARGA
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
            Shake.Instance.TriggerShake(2f);
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
            GameManager.Instance.ExecuteFinishAnimation(5, 0);
        }

        //FINAL DAY2
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay2Presi) //37
        {
            //SE PUEDE LEVANTAR DE LA SILLA, SON LA 21:00
            // <-------------------------------------------------------- JOAN: POSAR CINEMATICA DE RECARGA
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
            GameManager.Instance.ExecuteFinishAnimation(5, 0);
        }

        //FINAL DAY3
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.SleepDay3Presi) //67
        {
            //SE PUEDE LEVANTAR DE LA SILLA, SON LA 21:00
            // <-------------------------------------------------------- JOAN: POSAR CINEMATICA DE RECARGA
        }
    }

    bool radarResult = false;
    private void RadarFinished(bool exit)
    {
        radarResult = exit;
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
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay1); //05
        }

        else if (GameManager.Instance.currentDay == Day.Day2)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay2); //26
        }

        else if (GameManager.Instance.currentDay == Day.Day3)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay3);
        }

        else if (GameManager.Instance.currentDay == Day.Day4)
        {
            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.ExplainMachineDay4);
        }
    }
}