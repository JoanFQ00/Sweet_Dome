using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("minigames States")]
    public bool radarCompleted = false;
    public bool codeCompleted = false;
    public bool temperatureCompleted = false;

    bool actionCompleted = false;
    bool actionTransfered = false;

    void Update()
    {
        if (DayTime.Instance.currentHour >= 15)
            if (isDayGoalCompleted())
                OnActionExecuted();
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

    //public void CompleteRadar()
    //{
    //    if (radarCompleted)
    //        return;

    //    radarCompleted = true;

    //    CheckAllMinigames();
    //}

    //public void CompleteCode()
    //{
    //    if (codeCompleted)
    //        return;

    //    codeCompleted = true;

    //    CheckAllMinigames();
    //}

    //public void CompleteTemperature()
    //{
    //    if (temperatureCompleted)
    //        return;

    //    temperatureCompleted = true;

    //    CheckAllMinigames();
    //}

    //void CheckAllMinigames()
    //{
    //    if (radarCompleted && codeCompleted && temperatureCompleted)
    //    {
    //        OnActionExecuted();
    //    }
    //}

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
        actionCompleted = true;
        actionTransfered = true;
    }
}
