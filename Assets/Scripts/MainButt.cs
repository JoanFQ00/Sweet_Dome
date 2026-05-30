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

    public void Press()
    {
        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        if (targetAnimator != null)
        {
            targetAnimator.Play(animationStateName, 0, 0f);
        }

        onPress?.Invoke();
    }
}