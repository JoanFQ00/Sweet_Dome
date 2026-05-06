using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Dec_Act", menuName = "Scriptable Objects/Dec_Act")]
public class Dec_Act : ScriptableObject
{
    [Header("Basic Info")]
    public string actionName;
    public ActionTypes actionType;

    [Header("Time Zones")]
    public bool stopsTime;
    public int passTimeToHour;
    public int passTimeToMinute;

    public static Action<Dec_Act, ActionTypes> OnExecuted;

    public void ActionExecuted()
    {
        OnExecuted?.Invoke(this, actionType);
    }
}
