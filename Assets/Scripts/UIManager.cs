using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject interactTxt;
    public TextMeshProUGUI timerText, dayText;
    public static UIManager Instance;
    public List<TextMeshProUGUI> objectivseTMP;

    void Start()
    {
        interactTxt.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.NewDay += OnDayChanged;
        Interact.CanInteract += ChangeInteractVisibility;
        DecManager.OnActionsChanged += ChangeHudObjectives;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
        Interact.CanInteract -= ChangeInteractVisibility;
        DecManager.OnActionsChanged -= ChangeHudObjectives;
    }

    void ChangeInteractVisibility(bool visibility, bool interacting) 
    {
        if (interacting)
        {
            interactTxt.SetActive(false);
        }
        else 
        {
            interactTxt.SetActive(visibility);
        }
    }

    void OnDayChanged()
    {
        dayText.text = $"{GameManager.Instance.currentDay.ToString().ToUpper()}";
    }

    void ChangeHudObjectives(List<NDec_Act> actionList) 
    {
        for (int i = 0; i < objectivseTMP.Count ; i++) 
        {
            objectivseTMP[i].text = actionList[i].actionName;
        }
    }
}