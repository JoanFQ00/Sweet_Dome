using UnityEngine;

public class BarGameValidator : ButtonValidator
{
    [Header("Referencias del Minijuego")]
    [Tooltip("Arrastra aquí el script BarMachineGame que gestiona la barra móvil.")]
    [SerializeField] private BarMachineGame barGameSource;

    public override bool CanPress()
    {
        if (barGameSource == null) return false;

        return barGameSource.IsGameActive;
    }
}