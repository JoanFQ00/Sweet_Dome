using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Action_Interactable : BaseAction
{
    [Header("Move")]
    public float timeToMove = 1.0f;
    private float currentMoveTime = 0.0f;
    private Vector3 startPos;
    private Vector3 originalPos;

    [Header("Rotation")]
    public float rotationSpeed = 150f;
    private Quaternion startRot;
    private Quaternion originalRot;

    public float restartRotTime = 0.8f;
    private float currentRestartRot;
    private bool restartingRotation;
    private bool hasBeenRotated;
    private Quaternion restartRot;

    [Header("Settings")]
    public bool isAction = false;
    public float timeToRestartRotation = 3.0f;
    public bool keepBetweenDays = true;
    public Day DayLoad;
    private float currentTimeRotated;
    public bool farPosition;

    private enum InteractableType { Rotator, Look }
    [SerializeField] private InteractableType type;

    private PlayerController player;
    private Transform inspectTransform;
    private Transform farTransform;

    [SerializeField] Quaternion closeRot;
    [SerializeField] Transform inspectorAnchor;

    private enum State
    {
        Idle,
        MovingToInspect,
        Inspecting,
        Returning
    }

    private State state = State.Idle;

    void Start()
    {
        ForceInternalInit();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        LevelManager.OnLevelLoaded += LevelLoaded;
        ForceInternalInit();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        LevelManager.OnLevelLoaded -= LevelLoaded;
    }

    void LevelLoaded()
    {
        ForceInternalInit();
    }

    private void ForceInternalInit()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            player = GameManager.Instance.playerController;
        }
        else
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            var interactComp = player.GetComponent<Interact>();
            if (interactComp != null)
            {
                inspectTransform = interactComp.readTransform;
                farTransform = interactComp.farTransform;
            }
        }

        if (originalPos == Vector3.zero)
        {
            originalPos = transform.position;
            originalRot = transform.rotation;
        }
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        // Forzamos la actualización de la referencia pase lo que pase
        ForceInternalInit();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState = GameManager.GameState.Interacting;
        }

        startPos = transform.position;
        startRot = transform.rotation;

        if (Camera.main != null && inspectorAnchor != null)
        {
            Quaternion lookAtRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
            closeRot = lookAtRotation * Quaternion.Inverse(inspectorAnchor.localRotation);
        }

        currentMoveTime = 0f;
        state = State.MovingToInspect;

        ApplyTotalCameraLock(true);
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToInspect:

                Vector3 targetPos = farPosition ? farTransform.position : inspectTransform.position;

                // Mantenemos la cámara congelada mientras se traslada
                ApplyTotalCameraLock(true);

                Move(targetPos, closeRot, () =>
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");
                    state = State.Inspecting;
                });

                break;

            case State.Inspecting:
                if (GameManager.Instance.currentState != GameManager.GameState.Interacting)
                    return;

                ApplyTotalCameraLock(true);

                if (hasBeenRotated && !Input.GetMouseButton(0) && !restartingRotation)
                {
                    currentTimeRotated += Time.deltaTime;
                }
                else if (Input.GetMouseButton(0) && type == InteractableType.Rotator)
                {
                    currentTimeRotated = 0f;
                }

                if (currentTimeRotated >= timeToRestartRotation)
                {
                    if (!restartingRotation)
                    {
                        restartingRotation = true;
                        restartRot = transform.rotation;
                        currentRestartRot = 0f;
                    }
                    RestartRotation();
                }

                if (Input.GetMouseButton(0) && type == InteractableType.Rotator && !restartingRotation)
                {
                    RotateObject();
                    hasBeenRotated = true;
                }

                if (Input.GetKeyDown(KeyCode.E) && !restartingRotation)
                {
                    startPos = transform.position;
                    startRot = transform.rotation;
                    currentMoveTime = 0f;
                    state = State.Returning;
                }
                break;

            case State.Returning:

                Move(originalPos, originalRot, () =>
                {
                    gameObject.layer = LayerMask.NameToLayer("Interactable");

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.currentState = GameManager.GameState.Gameplay;
                    }

                    state = State.Idle;

                    ApplyTotalCameraLock(false);

                    if (isAction)
                        OnActionExecuted();
                });

                break;
        }
    }

    private void ApplyTotalCameraLock(bool lockState)
    {
        if (player == null) ForceInternalInit();

        if (player != null)
        {
            player.LockPlayer(lockState);
            player.LockCamera(lockState);
            player.cameraLocked = lockState;

            // Desactivamos el mapa de acciones de "Look" del PlayerInput para que no lea el ratón de fondo
            PlayerInput pInput = player.GetComponent<PlayerInput>();
            if (pInput != null && pInput.actions != null)
            {
                var lookAction = pInput.actions["Look"];
                if (lookAction != null)
                {
                    if (lockState) lookAction.Disable();
                    else lookAction.Enable();
                }
            }
        }
    }

    void RestartRotation()
    {
        currentRestartRot += Time.deltaTime;
        float pct = Mathf.Clamp01(currentRestartRot / restartRotTime);

        transform.rotation = Quaternion.Lerp(restartRot, closeRot, pct);

        if (pct >= 1.0f)
        {
            restartingRotation = false;
            hasBeenRotated = false;
            currentTimeRotated = 0f;
        }
    }

    void RotateObject()
    {
        if (Camera.main == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Camera.main.transform.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Camera.main.transform.right, mouseY * rotationSpeed * Time.deltaTime, Space.World);
    }

    void Move(Vector3 targetPos, Quaternion targetRot, System.Action onFinish)
    {
        currentMoveTime += Time.deltaTime;

        float pct = Mathf.Clamp01(currentMoveTime / timeToMove);

        transform.position = Vector3.Lerp(startPos, targetPos, pct);
        transform.rotation = Quaternion.Lerp(startRot, targetRot, pct);

        if (pct >= 1.0f)
        {
            onFinish?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main != null && inspectorAnchor != null)
        {
            Vector3 dirForward = inspectorAnchor.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(inspectorAnchor.transform.position, dirForward.normalized * 0.35f);
        }
    }
}