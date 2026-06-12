using UnityEngine;

public class Day1ButtonValidator : ButtonValidator
{
    public override bool CanPress()
    {
        // Si el GameManager no está listo, por seguridad bloqueamos el botón
        if (GameManager.Instance == null) return false;

        // Si el día actual es el Día 1, rechazamos la pulsación
        if (GameManager.Instance.currentDay == Day.Day1)
        {
            Debug.Log($"<color=yellow><b>[Validator]</b></color> Botón de cambio de zona bloqueado: Es el Día 1.");
            return false;
        }

        // Cualquier otro día (Día 2 en adelante) se puede pulsar libremente
        return true;
    }
}