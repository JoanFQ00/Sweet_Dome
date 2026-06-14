using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

[RequireComponent(typeof(Animation))]
public class MainButt : MonoBehaviour, IResetable
{
    public enum VisualButtonState
    {
        Up,
        Down
    }

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

    // Rastreadores de estado físico internos
    protected VisualButtonState currentVisualState = VisualButtonState.Up;
    protected bool isBeingHeld = false;

    // Añade esto dentro de MainButt.cs para que el cursor pueda consultar el validador
    public ButtonValidator Validator => validator; // Cambia 'validator' por el nombre real de tu variable si es distinto

    protected virtual void Awake()
    {
        legacyAnimation = GetComponent<Animation>();

        if (legacyAnimation != null)
        {
            if (pressClip != null) legacyAnimation.AddClip(pressClip, pressClip.name);
            if (releaseClip != null) legacyAnimation.AddClip(releaseClip, releaseClip.name);
        }
    }

    protected virtual void Start()
    {
        // Registrar el botón en el GameManager al iniciar
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // Desregistrar de forma segura para evitar fallos de memoria
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    protected virtual void Update()
    {
        // Control automatizado si NO se está interactuando físicamente con él
        if (!isBeingHeld)
        {
            EvaluateValidatorState();
        }
    }

    /// <summary>
    /// Comprueba frame a frame si el botón debe subir o bajarse solo según su disponibilidad.
    /// </summary>
    private void EvaluateValidatorState()
    {
        if (validator == null) return;

        bool canPressNow = validator.CanPress();

        // CASO A: Deja de estar disponible y está arriba -> Se hunde solo hacia adentro
        if (!canPressNow && currentVisualState == VisualButtonState.Up)
        {
            AnimatePress(pressSpeedMultiplier);
        }
        // CASO B: Estaba bloqueado (abajo) pero ahora vuelve a estar disponible -> Sube solo
        else if (canPressNow && currentVisualState == VisualButtonState.Down)
        {
            AnimateRelease(releaseSpeedMultiplier);
        }
    }

    /// <summary>
    /// Se ejecuta inmediatamente en el MouseDown (Click inicial).
    /// </summary>
    public virtual void Press()
    {
        wasLastPressValid = false;
        isBeingHeld = true; // El jugador está interactuando con él

        bool isPlayerSeated = true; // Reemplazar por tu lógica real de la silla

        if (!isPlayerSeated || (validator != null && !validator.CanPress()))
        {
            return;
        }

        wasLastPressValid = true;

        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        // Si el botón ya estaba abajo visualmente por lógica del juego, no reiniciamos la animación
        if (currentVisualState == VisualButtonState.Up)
        {
            AnimatePress(pressSpeedMultiplier);
        }

        onPress?.Invoke();
    }

    /// <summary>
    /// Se ejecuta frame a frame mientras mantienes pulsado.
    /// </summary>
    public virtual void HoldUpdate()
    {
        // Mantenemos el flag activo mientras el ratón siga haciendo presión
        isBeingHeld = true;
    }

    /// <summary>
    /// Se ejecuta inmediatamente en el MouseUp (Al soltar el click).
    /// </summary>
    public virtual void Release()
    {
        isBeingHeld = false; // El jugador suelta el botón

        if (!wasLastPressValid) return;

        // REGRESIÓN INTELIGENTE: Miramos si el validador cambió a FALSE *mientras* lo manteníamos pulsado
        bool canPressNow = (validator == null) || validator.CanPress();

        if (canPressNow)
        {
            // Si sigue estando disponible al soltarlo, sube con normalidad
            AnimateRelease(releaseSpeedMultiplier);

            if (!releaseSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(releaseSound, gameObject);
            }
        }
        else
        {
            // EXCEPCIÓN CONSEGUIDA: Si dejó de estar disponible en mitad del click,
            // NO ejecuta la animación de subida (Release) y se queda bloqueado abajo.
            currentVisualState = VisualButtonState.Down;
        }

        wasLastPressValid = false;
    }

    // =================================================================
    // INTERFAZ IRESETABLE: CAMBIO DE DÍA
    // =================================================================
    public virtual void ResetValues()
    {
        // Al empezar un nuevo día, forzamos que ejecute la animación de Release (Subida)
        AnimateRelease(releaseSpeedMultiplier);
    }

    // =================================================================
    // MÉTODOS AUXILIARES DE ANIMACIÓN
    // =================================================================
    protected void AnimatePress(float speed)
    {
        if (legacyAnimation != null && pressClip != null)
        {
            legacyAnimation.Stop();
            if (legacyAnimation[pressClip.name] != null)
            {
                legacyAnimation[pressClip.name].speed = speed;
            }
            legacyAnimation.Play(pressClip.name);
            currentVisualState = VisualButtonState.Down;
        }
    }

    protected void AnimateRelease(float speed)
    {
        if (legacyAnimation != null && releaseClip != null)
        {
            legacyAnimation.Stop();
            if (legacyAnimation[releaseClip.name] != null)
            {
                legacyAnimation[releaseClip.name].speed = speed;
            }
            legacyAnimation.Play(releaseClip.name);
            currentVisualState = VisualButtonState.Up;
        }
    }
}