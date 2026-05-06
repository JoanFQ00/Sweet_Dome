using UnityEngine;
using System;

public class BedInt : MonoBehaviour, IInteractable
{
    public static Action OnInteract;

    public void Interact() 
    {
        if (DayTime.Instance.CanSleepInBed())
        {
            OnInteract?.Invoke();
            DayTime.Instance.ResetDayTime();
        }
    }
}
