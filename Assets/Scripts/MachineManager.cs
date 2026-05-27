using FMODUnity;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MachineManager : MonoBehaviour
{
    [Header("Actions")]
    public NDec_Act act;

    [Header("Zone Compatment")]
    [SerializeField] private CityFunnel cityFunnel;

    [Header("Day 1 Goals")]
    [SerializeField] private List<float> cityValuesD1;

    [Header("Day 2 Goals")]
    [SerializeField] private List<float> cityValuesD2;

    [Header("Minigames States")]
    public bool radarCompleted = false;
    public bool codeCompleted = false;
    public bool temperatureCompleted = false;

    [Header("EventDispatcher")]
    public static Action<float,float, int> RadarDispatch; // TimeToEvent, EventTime, QuantityOfObj

    //[Header("EventTimers")]
    //public float timeToRadar = 7.5f;

    void Update()
    {
        if (DayTime.Instance.currentHour >= 15)
            if (isDayGoalCompleted())
                OnActionExecuted();

        
    }

    private void OnEnable()
    {
        PresiSys.OnDialogueTerminated += CheckTerminatedDialogue;
    }

    private void OnDisable()
    {
        PresiSys.OnDialogueTerminated -= CheckTerminatedDialogue;
    }

    public bool isDayGoalCompleted()
    {
        bool isCompleted = true;

        for (int i = 0; i < cityFunnel.GetCityZones().Count; i++)
        {
            if (cityFunnel.GetCityZones()[i].CurrentCapacity < cityValuesD1[i])
            {
                isCompleted = false;
                break;
            }
        }

        return isCompleted;
    }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
    }

    void CheckTerminatedDialogue() 
    {
        if (GameManager.Instance.presiSpeaker.dialogueSys.id == (int)PresiDialogue.ExplainMachineDay1) 
        {
            RadarDispatch?.Invoke(1, 25, 2);
        }
    }
}