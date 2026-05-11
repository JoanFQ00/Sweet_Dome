using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NDec_Act", menuName = "Scriptable Objects/NDec_Act")]
public class NDec_Act : ScriptableObject
{
    [Header("Basic Info")]
    public string actionName;
    public int ID;
    public ActionTypes actionType;

    [Header("Time Zones")]
    public bool stopsTime;
    public int hourToInteract;
    public int passTimeToHour;
    public int passTimeToMinute;

    public static Action<NDec_Act, ActionTypes> OnExecuted;

    public void ActionExecuted()
    {
        OnExecuted?.Invoke(this, actionType);
    }
}
