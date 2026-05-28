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
    private IInteractable interactable;
    private HL_Interact currentHL;

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
    }

    void TryInteract()
    {
        Ray interactRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(interactRay, out hit, interactDistance, interactLayer))
        {
            if (hit.collider.TryGetComponent(out BaseAction baseAct))
            {
                if (baseAct.act != null)
                {
                    if (baseAct.act.hourToInteract != DayTime.Instance.GetCurrentHour())
                    {
                        interactable = null;
                        ClearHighlight();
                        CanInteract?.Invoke(false, interacting);
                        return;
                    }
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
                HL_Interact newHL = hit.collider?.GetComponent<HL_Interact>();
                if (currentHL != newHL)
                {
                    ClearHighlight();
                    currentHL = newHL;
                    currentHL?.LightenColor();
                }
                CanInteract?.Invoke(true, interacting);
            }
            else
            {
                ClearHighlight(); 
                CanInteract?.Invoke(false, interacting);
            }
        }
        else
        {
            ClearHighlight();
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

    void ClearHighlight()
    {
        if (currentHL != null)
        {
            currentHL.ResetColor();
        }

        currentHL = null;
    }
}
