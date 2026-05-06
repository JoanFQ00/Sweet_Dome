using UnityEngine;
using System.Collections.Generic;

public enum ActionTypes { ReadBook, Eat, WatchTV, TurnCompleted, TurnFailed }

public class DecManager : MonoBehaviour
{
    public List<NDec_Act> dayActions;
    private List<NDec_Act> originalDayActions;

    public List<NDec_Act> actionsPerformed = new List<NDec_Act>();
    public List<NDec_Act> actionsNotPerformed = new List<NDec_Act>();

    private void Start()
    {
        originalDayActions = new List<NDec_Act>(dayActions);
    }

    private void OnEnable()
    {
        NDec_Act.OnExecuted += CheckExecutedAction;
        DayTime.OnTimeSkiped += TimeSkiped;
    }

    private void OnDisable()
    {
        NDec_Act.OnExecuted -= CheckExecutedAction;
        DayTime.OnTimeSkiped -= TimeSkiped;
    }

    private void Update()
    {
        if (dayActions.Count <= 0)
        {
            Debug.Log("All day actions performed Go To Work");
            ResetDayActions();
        }
    }

    private void CheckExecutedAction(NDec_Act action, ActionTypes actionType) 
    {
        NDec_Act actToRemove = null;
        foreach (NDec_Act act in dayActions) 
        {
            if (act == null) continue;

            if (act == action)
            {
                actToRemove = act;
                actionsPerformed.Add(act);
                break;
            }
        }

        if (actToRemove != null) 
        {
            dayActions.Remove(actToRemove);
        }
    }

    private void TimeSkiped(int currentHour)
    {
        List<NDec_Act> actsToRemove = new List<NDec_Act>();

        foreach (NDec_Act act in dayActions)
        {
            if (currentHour > act.hourToInteract)
            {
                actsToRemove.Add(act);
            }
        }

        foreach (NDec_Act act in actsToRemove)
        {
            SkippedAction(act);
        }
    }

    private void SkippedAction(NDec_Act s_Action)
    {
        dayActions.Remove(s_Action);
        actionsNotPerformed.Add(s_Action);
    }

    public void ResetDayActions() 
    {
        dayActions = new List<NDec_Act>(originalDayActions);
    }
}
