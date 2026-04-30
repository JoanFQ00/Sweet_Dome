using UnityEngine;

public abstract class BaseAction : MonoBehaviour, IInteractable
{
    public Dec_Act act;

    protected bool actionCompleted = false;
    protected bool actionTransfered = false;

    protected int currentLayer = -1;

    protected virtual void Update()
    {
        if (actionTransfered) return;

        CheckHour();
        OnUpdateAction();
    }

    public abstract void Interact();

    protected virtual void OnUpdateAction() { }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.StopTimeCount(false);
        actionCompleted = true;
        actionTransfered = true;
    }

    protected virtual void OnActionFailed()
    {
        act.ActionFailed();
        actionTransfered = true;
    }

    protected void CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (act.stopsTime && hour >= act.minIntHour && !actionCompleted)
        {
            DayTime.Instance.StopTimeCount(true);
        }

        if (hour < act.minIntHour || hour >= act.maxIntHour)
            newLayer = LayerMask.NameToLayer("Default");
        else
            newLayer = LayerMask.NameToLayer("Interactable");

        if (hour >= act.maxIntHour && !actionCompleted)
        {
            OnActionFailed();
        }

        if (newLayer != currentLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }
}