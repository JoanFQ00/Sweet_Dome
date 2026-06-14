using UnityEngine;

public class CityButtonValidator : ButtonValidator
{
    [SerializeField] private CityFunnel cityFunnel;

    public override bool CanPress()
    {
        if (cityFunnel == null) return false;

        // Leemos la capacidad real float (ej: 0.3f o 1.7f)
        float currentCapacity = cityFunnel.CurrentCapacity;

        // Aplicamos el mismo redondeo que usa tu UI en la cabina.
        // NOTA: Si tu UI usa truncado (hacia abajo), cambia 'Mathf.RoundToInt' por 'Mathf.FloorToInt'
        int visualCapacity = Mathf.RoundToInt(currentCapacity);

        // El botón SOLO será pulsable si el jugador ve un número mayor o igual a 1
        return visualCapacity >= 1;
    }
}