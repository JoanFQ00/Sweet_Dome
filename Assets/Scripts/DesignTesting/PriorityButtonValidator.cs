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

            // El botón SOLO será pulsable si tú activas la crisis desde el MachineManager
            // y si el jugador no lo ha pulsado ya.
            return priorityManager.isDay5CrisisActive && !priorityManager.hasBeenPressedDay5;
        }

        // --- COMPORTAMIENTO HABITUAL (Días 1 a 4) ---
        if (GameManager.Instance.currentDay == Day.Day1)
        {
            return false;
        }

        if (priorityManager == null) return false;

        return !priorityManager.HasUsedPriorityToday();
    }
}