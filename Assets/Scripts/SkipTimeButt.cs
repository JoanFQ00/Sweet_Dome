using UnityEngine;
using UnityEngine.Events;

public class SkipTimeButt : MonoBehaviour, IInteractable
{
    public UnityEvent onPress;

    public void Interact()
    {
        onPress?.Invoke();
    }
}