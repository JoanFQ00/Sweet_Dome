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

        CheckHour();

        OnUpdateAction();
    }

    public void CheckHour() 
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (hour != act.hourToInteract)
            newLayer = LayerMask.NameToLayer("Default");
        else
            newLayer = LayerMask.NameToLayer("Interactable");

        if (currentLayer != newLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }

    public abstract void Interact();

    protected virtual void OnUpdateAction() { }

    public void ResetActionState()
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