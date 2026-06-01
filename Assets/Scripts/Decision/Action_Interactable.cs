using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class Action_Interactable : BaseAction
{
    [Header("Move")]
    public float timeToMove = 1.0f;

    [Header("Rotation")]
    public float rotationSpeed = 150f;

    [Header("Settings")]
    public bool isAction = false;
    private enum InteractableType { Rotator, Look }
    [SerializeField] private InteractableType type;

    private float currentMoveTime = 0.0f;

    private Transform inspectTransform;

    private PlayerController player;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 originalPos;
    private Quaternion originalRot;

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
        }
    }



    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        startPos = transform.position;
        startRot = transform.rotation;

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

                Move(inspectTransform.position, inspectTransform.rotation, () =>
                {
                    gameObject.layer = LayerMask.NameToLayer("Default");

                    state = State.Inspecting;
                });

                break;

            case State.Inspecting:
                if (GameManager.Instance.currentState != GameManager.GameState.Interacting)
                    return;

                if (Input.GetMouseButton(0) && type == InteractableType.Rotator)
                {
                    RotateObject();
                }

                if (Input.GetKeyDown(KeyCode.E))
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
}