//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour, IInteractable
{
    public NDec_Act act;

    protected bool actionCompleted = false;
    protected bool actionTransfered = false;
    protected bool canInteract = false;

    protected int currentLayer = -1;

    protected virtual void OnEnable()
    {
        GameManager.OnChangeDay += ResetActionState;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnChangeDay -= ResetActionState;
    }

    protected virtual void Update()
    {
        if (actionTransfered) return;

        if (act != null) 
        {
            CheckHour();
        }

        OnUpdateAction();
    }

    public void CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (act.alwaysAccesible || hour == act.hourToInteract)
        {
            newLayer = LayerMask.NameToLayer("Interactable");
        }
        else
        {
            newLayer = LayerMask.NameToLayer("Default");
        }

        if (currentLayer != newLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }

    public abstract void Interact();

    protected virtual void OnUpdateAction() { }

    public void ResetActionState(Day c_Day)
    {
        actionCompleted = false;
        actionTransfered = false;
    }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
        actionCompleted = true;
        actionTransfered = true;
    }  
}