using UnityEngine;

public class MachineManager : MonoBehaviour
{
    [Header("Actions")]
    public NDec_Act act;

    [Header("minigames States")]
    public bool radarCompleted = false;
    public bool codeCompleted = false;
    public bool temperatureCompleted = false;

    bool actionCompleted = false;
    bool actionTransfered = false;

    public void CompleteRadar()
    {
        if (radarCompleted)
            return;

        radarCompleted = true;

        CheckAllMinigames();
    }

    public void CompleteCode()
    {
        if (codeCompleted)
            return;

        codeCompleted = true;

        CheckAllMinigames();
    }

    public void CompleteTemperature()
    {
        if (temperatureCompleted)
            return;

        temperatureCompleted = true;

        CheckAllMinigames();
    }

    void CheckAllMinigames()
    {
        if (radarCompleted && codeCompleted && temperatureCompleted)
        {
            OnActionExecuted();
        }
    }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
        actionCompleted = true;
        actionTransfered = true;
    }
}
