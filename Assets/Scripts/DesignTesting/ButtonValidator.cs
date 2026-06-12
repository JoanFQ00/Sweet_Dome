using UnityEngine;

public abstract class ButtonValidator : MonoBehaviour
{
    /// <summary>
    /// Devuelve true si el botón se puede pulsar en este momento.
    /// </summary>
    public abstract bool CanPress();
}