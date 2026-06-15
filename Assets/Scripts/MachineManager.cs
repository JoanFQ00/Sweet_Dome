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
        NODE_01_D1,
        NODE_02_D1,
        NODE_03_D1,
        NODE_04_D1,
        NODE_05_D1,
        NODE_06_D1,
    }

    private MachineStoryNode currentNode = MachineStoryNode.NODE_01_D1;
    DayManager dayManager;

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
                        if (currentCountdown >= 20f)
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
                        if (currentCountdown >= 20f)
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
                        if (currentCountdown >= 20f)
                        {
                            GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.AngryAngryDay1Presi); //12
                            ResetCountdowns();
                            onCountdown = false;
                        }
                        break;
                }
                break;

            case Day.Day2:
            break;

            case Day.Day3:
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
    }
}