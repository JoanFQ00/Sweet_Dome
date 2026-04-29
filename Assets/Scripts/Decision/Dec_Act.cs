using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Dec_Act", menuName = "Scriptable Objects/Dec_Act")]
public class Dec_Act : ScriptableObject
{
    [Header("Basic Info")]
    public string actionName;
    public ActionTypes actionType;

    [Header("Time Zones")]
    public int minIntHour;
    public int maxIntHour;

    public static Action<string, ActionTypes> OnExecuted;

    public void ActionExecuted() 
    {
        OnExecuted?.Invoke(actionName, actionType);
    }

    public void ActionFailed() 
    {
        OnExecuted?.Invoke("F_" + actionName, actionType);
    }
}
