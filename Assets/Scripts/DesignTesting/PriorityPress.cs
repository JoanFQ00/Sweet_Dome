using UnityEngine;
using System;

public class PriorityPress : MainButt
{
    [ColorUsage(true, true)]
    public Color activeEmissionColor = Color.white;

    private CityPriorityManager cityPriorityManager;

    // Acción estática global para que cualquier LED de la escena la escuche
    public static Action<Color, int> OnPressedButton;

    // SOLUCIÓN: Cambiado a protected override y llamamos a base.Awake()
    protected override void Awake()
    {
        // 1. Obligamos a ejecutar primero el Awake del padre para buscar la animación y registrar los clips
        base.Awake();

        // 2. Buscamos el componente específico de este botón hijo
        cityPriorityManager = GetComponent<CityPriorityManager>();
    }

    public override void Press()
    {
        // 1. Ejecuta el código original del padre (Animación y Raycast amigable)
        base.Press();

        // 2. Comprobamos si el padre dio el visto bueno a la pulsación.
        if (!wasLastPressValid)
            return;

        // 3. Si pasa el filtro, lanzamos el evento con total seguridad
        if (cityPriorityManager != null)
        {
            OnPressedButton?.Invoke(activeEmissionColor, cityPriorityManager.GetIdxPrioritizedZone());
        }
    }
}