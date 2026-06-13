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

        // Regresa false el Día 1 (Bloqueado)
        if (GameManager.Instance.currentDay == Day.Day1)
        {
            return false;
        }

        if (priorityManager == null) return false;

        return !priorityManager.HasUsedPriorityToday();
    }
}