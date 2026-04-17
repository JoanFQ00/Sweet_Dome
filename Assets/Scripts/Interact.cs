using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable{
    public void Interact();
}

public class Interact : MonoBehaviour
{
    public float interactDistance = 2.5f;
    public LayerMask interactLayer;
    public Camera playerCamera;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private bool debugMode = true;
    private IInteractable interactable;

    public static event Action<bool> CanInteract;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        interactAction.started += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.started -= OnInteract;
    }

    void Update()
    {
        CheckForInteract();

        if (Input.GetKeyDown(KeyCode.L))
            debugMode = !debugMode;
    }

    void CheckForInteract() 
    {
        Ray interactRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(interactRay, out hit, interactDistance, interactLayer))
        { 
            interactable = hit.collider.GetComponent<IInteractable>();
            CanInteract?.Invoke(true);
        }
        else
        {
            interactable = null;
            CanInteract?.Invoke(false);
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx) 
    {
        TryInteract(interactable);
    }

    private void TryInteract(IInteractable interactable) 
    {
        if (interactable == null) return;

        interactable.Interact();    
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        Gizmos.color = Color.purple;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawRay(ray.origin, ray.direction * interactDistance);   
    }
}
