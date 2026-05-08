using UnityEngine;
using UnityEngine.Events;
using System;

public class SkipTimeButt : MonoBehaviour, IInteractable
{
    public UnityEvent onPress;
    public static Action<bool> OnFirstDayButt;

    public void Interact()
    {
        if (GameManager.Instance.currentDay != Day.Day1) 
            onPress?.Invoke();
        else 
            OnFirstDayButt?.Invoke(true);
    }
}