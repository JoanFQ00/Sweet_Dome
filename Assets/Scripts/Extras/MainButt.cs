using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class MainButt : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPress;

    [Header("Audio")]
    [SerializeField] protected EventReference pressSound;

    [Header("Animation")]
    [SerializeField] protected Animator targetAnimator;
    [SerializeField] protected string animationStateName;

    [Header("Validation (Optional)")]
    [Tooltip("Arrastra aquí un script que herede de ButtonValidator si este botón tiene restricciones de uso.")]
    [SerializeField] protected ButtonValidator validator;

    // Flag protegido para que los scripts hijos puedan leer si la pulsación fue legítima
    protected bool wasLastPressValid { get; private set; }

    // Volvemos al método 'void' original para que tu Raycast no tenga problemas
    public virtual void Press()
    {
        // Reseteamos el estado al iniciar la pulsación
        wasLastPressValid = false;

        bool isPlayerSeated = true; // Cambiar por tu lógica real de interacción de silla

        if (!isPlayerSeated)
        {
            Debug.Log($"<color=orange><b>[MainButt]</b></color> Intento de pulsar {gameObject.name}, pero el jugador NO está sentado.");
            return;
        }

        if (validator != null && !validator.CanPress())
        {
            Debug.Log($"<color=yellow><b>[MainButt]</b></color> Botón {gameObject.name} bloqueado por validación interna.");
            return;
        }

        // Si el código llega aquí, significa que la pulsación es 100% válida
        wasLastPressValid = true;

        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        if (targetAnimator != null)
        {
            targetAnimator.Play(animationStateName, 0, 0f);
        }

        onPress?.Invoke();
    }
}