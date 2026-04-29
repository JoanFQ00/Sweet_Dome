using UnityEngine;
using System.Collections.Generic;

public enum ActionTypes { ReadBook, Eat, WatchTV, TurnCompleted, TurnFailed }

public class DecManager : MonoBehaviour
{
    public List<Dec_Act> dayActions;
    private List<Dec_Act> originalDayActions;

    public List<string> actionsPerformed = new List<string>();

    private void Start()
    {
        originalDayActions = new List<Dec_Act>(dayActions);
    }

    private void OnEnable()
    {
        Dec_Act.OnExecuted += CheckExecutedAction;
    }

    private void OnDisable()
    {
        Dec_Act.OnExecuted -= CheckExecutedAction;
    }

    private void CheckExecutedAction(string actionName, ActionTypes actionType) 
    {
        Dec_Act actToRemove = null;
        foreach (Dec_Act act in dayActions) 
        {
            if (act.actionType == actionType) 
            {
                actToRemove = act;
                actionsPerformed.Add(actionName);
                break;
            }
        }

        if (actToRemove != null) 
        {
            dayActions.Remove(actToRemove);
        }
    }

    private void Update()
    {
        if (dayActions.Count <= 0) 
        {
            Debug.Log("All day actions performed Go To Work");
            ResetDayActions();
        }
    }

    public void ResetDayActions() 
    {
        dayActions = new List<Dec_Act>(originalDayActions);
    }
}
