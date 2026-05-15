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
    public Transform readTransform;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private bool debugMode = true;
    private IInteractable interactable;

    bool interacting;

    public static event Action<bool,bool> CanInteract;

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
        if (interacting)
        {
            CanInteract?.Invoke(true, interacting);
            return;
        }

        TryInteract();

        if (Input.GetKeyDown(KeyCode.L))
            debugMode = !debugMode;
    }

    void TryInteract() 
    {
        Ray interactRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(interactRay, out hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent(out BaseAction baseAct))
            {
                if (baseAct.act.hourToInteract != DayTime.Instance.GetCurrentHour())
                {
                    interactable = null;
                    CanInteract?.Invoke(false, interacting);
                    return;
                }
            }
            if (hit.collider.TryGetComponent(out RobotBehaviour robo)) 
            {
                if (GameManager.Instance.dayManager.watchTvTriggered) 
                {
                    robo.dialogueSys.ChargeDialogues(robo.currentRandomDialogue);
                }
            }

            interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                CanInteract?.Invoke(true, interacting);
            }
            else
            {
                CanInteract?.Invoke(false, interacting);
            }
        }
        else
        {
            interactable = null;
            CanInteract?.Invoke(false, interacting);
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx) 
    {
        if (interactable != null)
        {
            interacting = true;
            interactable.Interact();
        }
    }

    public void ChangeInteracting(bool _interacting) 
    {
        interacting = _interacting;
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        Gizmos.color = Color.purple;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawRay(ray.origin, ray.direction * interactDistance);   
    }
}
