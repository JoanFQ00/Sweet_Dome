using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class MainButt : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onPress;

    [Header("Audio")]
    [SerializeField] private EventReference pressSound;

    public void Press()
    {
        RuntimeManager.PlayOneShotAttached(pressSound, gameObject);

        onPress?.Invoke();
    }
}