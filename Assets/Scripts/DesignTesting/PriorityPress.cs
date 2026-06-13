using UnityEngine;
using System;

public class PriorityPress : MainButt
{
    [ColorUsage(true, true)]
    public Color activeEmissionColor = Color.white;

    private CityPriorityManager cityPriorityManager;

    // Acción estática global para que cualquier LED de la escena la escuche
    public static Action<Color, int> OnPressedButton;

    private void Awake()
    {
        cityPriorityManager = GetComponent<CityPriorityManager>();
    }

    public override void Press()
    {
        // 1. Ejecuta el código original del padre (Raycast amigable)
        base.Press();

        // 2. Comprobamos si el padre dio el visto bueno a la pulsación.
        // Si el flag es falso (el jugador no estaba sentado o falló el validador), detenemos el script aquí.
        if (!wasLastPressValid)
            return;

        // 3. Si pasa el filtro, lanzamos el evento con total seguridad
        if (cityPriorityManager != null)
        {
            OnPressedButton?.Invoke(activeEmissionColor, cityPriorityManager.GetIdxPrioritizedZone());
        }
    }
}