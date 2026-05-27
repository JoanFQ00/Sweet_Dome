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

    void Update()
    {
        if (DayTime.Instance.currentHour >= 15)
            if (isDayGoalCompleted())
                OnActionExecuted();

        if (Input.GetKeyDown(KeyCode.L))
        {
            OnActionExecuted();
        }
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
}