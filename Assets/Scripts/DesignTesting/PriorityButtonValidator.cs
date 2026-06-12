using UnityEngine;

public class PriorityButtonValidator : ButtonValidator
{
    CityPriorityManager priorityManager;

    private void Start()
    {
        priorityManager = GetComponent<CityPriorityManager>();
    }

    public override bool CanPress()
    {
        if (priorityManager == null) return false;

        // El botón SOLO se puede pulsar si NO se ha usado la prioridad hoy
        return !priorityManager.HasUsedPriorityToday();
    }
}