using UnityEngine;
using System;

public class BedInt : MonoBehaviour, IInteractable
{
    public static Action OnInteract;

    void Update() 
    {
        gameObject.layer = LayerMask.NameToLayer
            (DayTime.Instance.CanSleepInBed() ? "Interactable" : "Default");
    }

    public void Interact() 
    {
        OnInteract?.Invoke();
        DayTime.Instance.ResetDayTime();

        GameManager.Instance.playerController.
        GetComponent<Interact>().ChangeInteracting(false);
    }
}
