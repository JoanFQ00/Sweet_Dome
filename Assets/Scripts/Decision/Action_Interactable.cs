using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

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
    private float currentTimeRotated;

    private enum InteractableType { Rotator, Look }
    [SerializeField] private InteractableType type;

    private PlayerController player;
    private Transform inspectTransform;

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
        Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LevelManager.OnLevelLoaded += LevelLoaded;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        LevelManager.OnLevelLoaded -= LevelLoaded;
    }

    void LevelLoaded()
    {
        Init();
    }

    void Init()
    {
        if (GameManager.Instance.playerController != null)
        {
            player = GameManager.Instance.playerController;

            inspectTransform = player
                .GetComponent<Interact>()
                .readTransform;

            originalPos = transform.position;
            originalRot = transform.rotation;

            hasBeenRotated = false;
            restartingRotation = false;
        }
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        startPos = transform.position;
        startRot = transform.rotation;

        Quaternion lookAtRotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
        closeRot = lookAtRotation * Quaternion.Inverse(inspectorAnchor.localRotation);

        player.LockPlayer(true);
        player.LockCamera(true);

        currentMoveTime = 0f;
        state = State.MovingToInspect;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToInspect:

                Move(inspectTransform.position, closeRot, () =>
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");

                    state = State.Inspecting;
                });

                break;

            case State.Inspecting:
                if (GameManager.Instance.currentState != GameManager.GameState.Interacting)
                    return;

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
                    player.LockPlayer(false);
                    player.LockCamera(false);

                    gameObject.layer = LayerMask.NameToLayer("Interactable");

                    GameManager.Instance.currentState = GameManager.GameState.Gameplay;

                    state = State.Idle;

                    if (isAction)
                        OnActionExecuted();
                });

                break;
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
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(
            Camera.main.transform.up,
            -mouseX * rotationSpeed * Time.deltaTime,
            Space.World);

        transform.Rotate(
            Camera.main.transform.right,
            mouseY * rotationSpeed * Time.deltaTime,
            Space.World);
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
        if (Camera.main != null)
        {
            Vector3 dirForward = inspectorAnchor.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(inspectorAnchor.transform.position, dirForward.normalized * 0.35f);
        }
    }
}