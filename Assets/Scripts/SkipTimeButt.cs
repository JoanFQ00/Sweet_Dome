using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem.Android;

public class SkipTimeButt : MonoBehaviour, IInteractable
{
    public UnityEvent onPress;
    public static Action<bool> OnFirstDayButt;

    public void Interact()
    {
        if (GameManager.Instance.currentDay == Day.Day3)
            onPress?.Invoke();
        else if (GameManager.Instance.currentDay == Day.Day1)
        {
            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day2) 
        {
            if (GameManager.Instance.dayTime.currentHour < 10)
            {
                GameManager.Instance.dayTime.SetCurrentTime(10, 00);
            }
            else if (GameManager.Instance.dayTime.currentHour < 11)
            {
                GameManager.Instance.dayTime.SetCurrentTime(11, 30);
            }
            else if (GameManager.Instance.dayTime.currentHour == 11) 
            {
                GameManager.Instance.dayTime.SetCurrentTime(15, 0);
            }

            OnFirstDayButt?.Invoke(true);
        }

        GameManager.Instance.playerController.
        GetComponent<Interact>().ChangeInteracting(false);
    }
}