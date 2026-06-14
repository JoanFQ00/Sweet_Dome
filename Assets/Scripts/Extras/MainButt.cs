using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

[RequireComponent(typeof(Animation))]
public class MainButt : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPress;

    [Header("Audio")]
    [SerializeField] protected EventReference pressSound;
    [SerializeField] protected EventReference releaseSound;

    [Header("Legacy Animation Clips")]
    [Tooltip("Arrastra aquí el archivo de animación (.anim) de cuando el botón baja.")]
    [SerializeField] protected AnimationClip pressClip;
    [Tooltip("Arrastra aquí el archivo de animación (.anim) de cuando el botón sube.")]
    [SerializeField] protected AnimationClip releaseClip;

    [Header("Velocidades de Respuesta")]
    [Tooltip("Multiplicador de velocidad para la animación de BAJADA (Press).")]
    [SerializeField] protected float pressSpeedMultiplier = 1.0f;
    [Tooltip("Multiplicador de velocidad para la animación de SUBIDA (Release) al pulsar rápido.")]
    [SerializeField] protected float releaseSpeedMultiplier = 3.0f;

    [Header("Validation (Optional)")]
    [Tooltip("Arrastra aquí un script que herede de ButtonValidator si este botón tiene restricciones de uso.")]
    [SerializeField] protected ButtonValidator validator;

    protected Animation legacyAnimation;
    protected bool wasLastPressValid { get; private set; }

    // Cambiado a virtual para que los hijos puedan extenderlo sin pisarlo
    protected virtual void Awake()
    {
        legacyAnimation = GetComponent<Animation>();

        // REGISTRO DINÁMICO: Inyectamos los clips arrastrados dentro del componente Animation
        if (legacyAnimation != null)
        {
            if (pressClip != null)
            {
                legacyAnimation.AddClip(pressClip, pressClip.name);
            }
            if (releaseClip != null)
            {
                legacyAnimation.AddClip(releaseClip, releaseClip.name);
            }
        }
    }

    /// <summary>
    /// Se ejecuta inmediatamente en el MouseDown (Click inicial).
    /// </summary>
    public virtual void Press()
    {
        wasLastPressValid = false;

        bool isPlayerSeated = true; // Reemplazar por tu lógica real de la silla

        if (!isPlayerSeated || (validator != null && !validator.CanPress()))
        {
            return;
        }

        wasLastPressValid = true;

        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        // Control visual de bajada
        if (legacyAnimation != null && pressClip != null)
        {
            legacyAnimation.Stop();

            // APLICAMOS EL MULTIPLICADOR DESDE EL INSPECTOR
            if (legacyAnimation[pressClip.name] != null)
            {
                legacyAnimation[pressClip.name].speed = pressSpeedMultiplier;
            }

            legacyAnimation.Play(pressClip.name);
        }

        // Acción lógica instantánea
        onPress?.Invoke();
    }

    public virtual void HoldUpdate()
    {
        // No requiere lógica para botones simples
    }

    /// <summary>
    /// Se ejecuta inmediatamente en el MouseUp (Al soltar el click).
    /// </summary>
    public virtual void Release()
    {
        if (!wasLastPressValid) return;

        // Interrupción fulminante + Aceleración de subida
        if (legacyAnimation != null && releaseClip != null)
        {
            legacyAnimation.Stop();

            if (legacyAnimation[releaseClip.name] != null)
            {
                legacyAnimation[releaseClip.name].speed = releaseSpeedMultiplier;
            }

            legacyAnimation.Play(releaseClip.name);
        }

        if (!releaseSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(releaseSound, gameObject);
        }

        wasLastPressValid = false;
    }
}