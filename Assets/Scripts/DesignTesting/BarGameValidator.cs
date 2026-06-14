using UnityEngine;

public class BarGameValidator : ButtonValidator
{
    [Header("Referencias del Minijuego")]
    [Tooltip("Arrastra aquí el script BarMachineGame que gestiona la barra móvil.")]
    [SerializeField] private BarMachineGame barGameSource;

    public override bool CanPress()
    {
        // Seguridad por si la referencia no está asignada en el inspector
        if (barGameSource == null) return false;

        // Devuelve true solo si el jugador está sentado, el juego no ha terminado y la barra está activa
        return barGameSource.IsGameActive;
    }
}