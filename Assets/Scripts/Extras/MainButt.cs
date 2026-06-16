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

    [Header("Animation Clips")]
    [SerializeField] protected AnimationClip pressClip;
    [SerializeField] protected AnimationClip releaseClip;

    [Header("Time Reaction Speed")]
    [SerializeField] protected float pressSpeedMultiplier = 1.0f;
    [SerializeField] protected float releaseSpeedMultiplier = 3.0f;

    [Header("Validation")]
    [SerializeField] protected ButtonValidator validator;

    protected Animation legacyAnimation;
    protected bool wasLastPressValid { get; private set; }

    protected VisualButtonState currentVisualState = VisualButtonState.Up;
    protected bool isBeingHeld = false;

    public ButtonValidator Validator => validator;

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
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterResetable(this);
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterResetable(this);
    }

    protected virtual void Update()
    {
        if (!isBeingHeld)
            EvaluateValidatorState();
    }

    private void EvaluateValidatorState()
    {
        if (validator == null) return;

        bool canPressNow = validator.CanPress();

        if (!canPressNow && currentVisualState == VisualButtonState.Up)
        {
            AnimatePressVisualPure(pressSpeedMultiplier);
        }
        else if (canPressNow && currentVisualState == VisualButtonState.Down)
        {
            AnimateReleaseVisualPure(releaseSpeedMultiplier);
        }
    }

    public virtual void Press()
    {
        wasLastPressValid = false;
        isBeingHeld = true;

        bool isPlayerSeated = true;

        if (!isPlayerSeated || (validator != null && !validator.CanPress()))
            return;

        wasLastPressValid = true;

        if (!pressSound.IsNull)
        {
            RuntimeManager.PlayOneShotAttached(pressSound, gameObject);
        }

        if (currentVisualState == VisualButtonState.Up)
            AnimatePressVisualPure(pressSpeedMultiplier);

        onPress?.Invoke();
    }

    public virtual void HoldUpdate()
    {
        isBeingHeld = true;
    }

    public virtual void Release()
    {
        isBeingHeld = false;

        if (!wasLastPressValid) return;

        bool canPressNow = (validator == null) || validator.CanPress();

        if (canPressNow)
        {
            AnimateReleaseVisualPure(releaseSpeedMultiplier);

            if (!releaseSound.IsNull)
            {
                RuntimeManager.PlayOneShotAttached(releaseSound, gameObject);
            }
        }
        else
        {
            currentVisualState = VisualButtonState.Down;
        }

        wasLastPressValid = false;
    }

    public virtual void ResetValues()
    {
        AnimateReleaseVisualPure(releaseSpeedMultiplier);
    }

    protected void AnimatePressVisualPure(float speed)
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

    protected void AnimateReleaseVisualPure(float speed)
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