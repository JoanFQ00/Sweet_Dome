using UnityEngine;
using System.Collections.Generic;
using System;

public enum ActionTypes { ReadBook, Eat, WatchTV, TurnCompleted, TurnFailed }

public class DecManager : MonoBehaviour
{
    public List<NDec_Act> dayActions;
    private List<NDec_Act> originalDayActions;
    [SerializeField] List<NDec_Act> starterDayActions;

    public List<NDec_Act> actionsPerformed = new List<NDec_Act>();
    public List<NDec_Act> actionsNotPerformed = new List<NDec_Act>();

    public static Action<List<NDec_Act>> OnActionsChanged;

    private void Awake()
    {
        originalDayActions = new List<NDec_Act>();
        dayActions ??= new List<NDec_Act>();
        actionsPerformed ??= new List<NDec_Act>();
        actionsNotPerformed ??= new List<NDec_Act>();
    }

    private void Start()
    {
        foreach (NDec_Act act in dayActions)
        {
        originalDayActions.Add(Instantiate(act));
        }
        ResetToInitialState();  
    }

    private void OnEnable()
    {
        NDec_Act.OnExecuted += CheckExecutedAction;
        DayTime.OnTimeSkiped += TimeSkiped;
        GameManager.OnChangeDay += CleanPerformedActions;
        LevelManager.OnLevelLoaded += LevelLoaded;
    }

    private void OnDisable()
    {
        NDec_Act.OnExecuted -= CheckExecutedAction;
        DayTime.OnTimeSkiped -= TimeSkiped;
        GameManager.OnChangeDay -= CleanPerformedActions;
        LevelManager.OnLevelLoaded -= LevelLoaded;
    }

    private void LevelLoaded()
    {
        originalDayActions.Clear();
        dayActions.Clear();

        foreach (NDec_Act act in starterDayActions)
        {
            originalDayActions.Add(act);
        }

        ResetDayActions();
    }

    private void CheckExecutedAction(NDec_Act action, ActionTypes actionType)
    {
        NDec_Act actToRemove = null;

        foreach (NDec_Act act in dayActions)
        {
            if (act == null) continue;

            if (act.ID == action.ID)
            {
                actToRemove = act;
                if (actionType == ActionTypes.WatchTV && 
                    GameManager.Instance.currentDay == Day.Day2) 
                {
                    GameManager.Instance.dayManager.afterTvDay2 = true;
                }
                actionsPerformed.Add(act);
                break;
            }
        }

        if (actToRemove != action) 
        {
            actionsPerformed.Add(action);
        }

        if (actToRemove != null)
        {
            dayActions.Remove(actToRemove);
            OnActionsChanged?.Invoke(dayActions);
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
        HashSet<int> baseIds = new HashSet<int>();
        foreach (NDec_Act act in originalDayActions)
        {
            baseIds.Add(act.ID);
        }

        List<NDec_Act> accionesParaConservar = new List<NDec_Act>();
        foreach (var act in dayActions)
        {
            if (!baseIds.Contains(act.ID))
            {
                accionesParaConservar.Add(act);
            }
        }

        dayActions.Clear();

        foreach (NDec_Act act in originalDayActions)
        {
            dayActions.Add(Instantiate(act));
        }

        dayActions.AddRange(accionesParaConservar);

        actionsPerformed.RemoveAll(a => baseIds.Contains(a.ID));
        actionsNotPerformed.Clear();

        ActionsChanged();
    }

    public void CleanPerformedActions(Day c_Day)
    {
        HashSet<int> originalIds = new HashSet<int>();

        foreach (var act in originalDayActions)
        {
            originalIds.Add(act.ID);
        }

        actionsPerformed.RemoveAll(action => originalIds.Contains(action.ID));

        OnActionsChanged?.Invoke(dayActions);
    }

    void ResetToInitialState() 
    {
        dayActions.Clear();

        foreach (NDec_Act act in originalDayActions)
        {
            dayActions.Add(act);
        }

        actionsPerformed.Clear();
        actionsNotPerformed.Clear();
        ActionsChanged();
    }

    public void ActionsChanged() 
    {
        OnActionsChanged?.Invoke(dayActions);
    }
}
