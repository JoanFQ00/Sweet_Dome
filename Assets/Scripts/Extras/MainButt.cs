using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class MainButt : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPress;

    [Header("Audio")]
    [SerializeField] private EventReference pressSound;

    [Header("Animation")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private string animationStateName;

    [Header("Validation (Optional)")]
    [Tooltip("Arrastra aquí un script que herede de ButtonValidator si este botón tiene restricciones de uso.")]
    [SerializeField] private ButtonValidator validator;

    public void Press()
    {
        // =================================================================
        // REGLA DE SEGURIDAD GLOBAL: CONTROL DE LA SILLA
        // =================================================================
        // CAMBIA ESTA CONDICIÓN por la variable real de tu juego (ej: ChairInteraction.Instance.isSeated)
        bool isPlayerSeated = true;

        if (!isPlayerSeated)
        {
            Debug.Log($"<color=orange><b>[MainButt]</b></color> Intento de pulsar {gameObject.name}, pero el jugador NO está sentado.");
            return; // Bloqueo inmediato para TODOS los botones del juego
        }

        // =================================================================
        // VALIDACIÓN ESPECÍFICA (Prioridades, niveles de tanques, etc.)
        // =================================================================
        if (validator != null && !validator.CanPress())
        {
            Debug.Log($"<color=yellow><b>[MainButt]</b></color> Botón {gameObject.name} bloqueado por validación interna.");
            return;
        }

        // Si pasa ambas condiciones, el botón se activa físicamente
        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        if (targetAnimator != null)
        {
            targetAnimator.Play(animationStateName, 0, 0f);
        }

        onPress?.Invoke();
    }
}