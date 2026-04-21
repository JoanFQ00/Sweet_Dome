using UnityEngine;
using System;

public class BedInt : MonoBehaviour, IInteractable
{
    public static Action OnInteract;

    public void Interact() 
    {
        OnInteract?.Invoke();
    }
}
