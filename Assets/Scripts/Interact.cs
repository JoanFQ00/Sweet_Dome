using System;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IInteractable
{
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

    public static event Action<bool, bool> CanInteract;

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
        bool isInteracting =
            GameManager.Instance.currentState == GameManager.GameState.Interacting;

        if (isInteracting)
        {
            CanInteract?.Invoke(false, true);
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
                    if (!baseAct.act.alwaysAccesible &&
                        baseAct.act.hourToInteract != DayTime.Instance.GetCurrentHour())
                    {
                        interactable = null;
                        ClearHighlight();

                        CanInteract?.Invoke(false, false);
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
                HL_Interact newHL = hit.collider.GetComponent<HL_Interact>();

                if (currentHL != newHL)
                {
                    ClearHighlight();
                    currentHL = newHL;
                    currentHL?.LightenColor();
                }

                CanInteract?.Invoke(true, false);
            }
            else
            {
                ClearHighlight();

                CanInteract?.Invoke(false, false);
            }
        }
        else
        {
            ClearHighlight();
            interactable = null;

            CanInteract?.Invoke(false, false);
        }
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (interactable != null &&
            GameManager.Instance.currentState == GameManager.GameState.Gameplay)
        {
            interactable.Interact();
        }
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