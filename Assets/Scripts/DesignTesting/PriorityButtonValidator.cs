using UnityEngine;

public class PriorityButtonValidator : ButtonValidator
{
    private CityPriorityManager priorityManager;

    private void Awake()
    {
        priorityManager = GetComponent<CityPriorityManager>();
    }

    public override bool CanPress()
    {
        if (GameManager.Instance == null) return false;

        if (GameManager.Instance.currentDay == Day.Day5)
        {
            if (priorityManager == null) return false;

            return priorityManager.isDay5CrisisActive && !priorityManager.hasBeenPressedDay5;
        }

        if (GameManager.Instance.currentDay == Day.Day1)
        {
            return false;
        }

        if (priorityManager == null) return false;

        return !priorityManager.HasUsedPriorityToday();
    }
}