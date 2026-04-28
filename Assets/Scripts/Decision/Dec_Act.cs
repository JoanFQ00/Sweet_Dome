using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

[CreateAssetMenu(fileName = "Dec_Act", menuName = "Scriptable Objects/Dec_Act")]
public class Dec_Act : ScriptableObject
{
    public string actionName;
    public ActionTypes actionType;

    public static Action<string, ActionTypes> OnExecuted;

    public void Execute() 
    {
        OnExecuted?.Invoke(actionName, actionType);
    }
}
