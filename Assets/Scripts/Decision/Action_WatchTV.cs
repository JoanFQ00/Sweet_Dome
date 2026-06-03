using UnityEngine;

public class Action_WatchTV : BaseAction
{
    [Header("Movement Settings")]
    public float timeToMove = 1.0f;
    public Transform playerTarget;

    [Header("Interaction Settings")]
    public LayerMask interactionLayer;

    [Header("Settings")]
    public bool isAction = false;

    public GameObject tvClickPopUp;

    private PlayerController player;
    private Transform playerTransform;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 originalPos;
    private Quaternion originalRot;

    bool hasWatchedTV;
    bool dorothyTurnedOff;

    private float currentMoveTime = 0f;

    private enum State
    {
        Idle,
        MovingToTV,
        WatchingPos,
        Watching,
        Returning
    }

    private State state = State.Idle;

    void Start()
    {
        player = GameManager.Instance.playerController;
        tvClickPopUp.SetActive(false);

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        player.LockPlayer(true);
        player.LockCamera(true);
        player.ChangeGameMode(false);

        originalPos = playerTransform.position;
        originalRot = playerTransform.rotation;

        startPos = playerTransform.position;
        startRot = playerTransform.rotation;

        currentMoveTime = 0f;
        state = State.MovingToTV;
    }

    protected override void OnEnable()
    {
        base.OnEnable(); 
        GameManager.NewDay += RestartBool;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.NewDay -= RestartBool;
    }

    void RestartBool() 
    {
        hasWatchedTV = false;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToTV:

                Move(playerTarget.position, playerTarget.rotation, () =>
                {
                    player.SyncRotationFromCurrentTransform();

                    player.ChangeGameMode(true);
                    player.LockCamera(false); 

                    state = State.WatchingPos;
                });

                break;

            case State.WatchingPos:
                bool isLookingAtTV = false;

                if (!hasWatchedTV && !dorothyTurnedOff && act.hourToInteract == GameManager.Instance.dayTime.currentHour)
                {
                    Ray ray = player.mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactionLayer))
                    {
                        if (hit.collider.CompareTag("TV"))
                        {
                            isLookingAtTV = true;

                            if (Input.GetMouseButtonDown(0))
                            {
                                tvClickPopUp.SetActive(false); 
                                hasWatchedTV = true;           
                                state = State.Watching;        
                            }
                        }
                    }
                }

                if (!hasWatchedTV)
                {
                    tvClickPopUp.SetActive(isLookingAtTV);
                }
                else
                {
                    tvClickPopUp.SetActive(false);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    tvClickPopUp.SetActive(false);
                    StartReturn();
                }
                break;

            case State.Watching:

                if (Input.GetKeyDown(KeyCode.E))
                {
                    // UnloadTV();
                    tvClickPopUp.SetActive(false);
                    StartReturn();
                }

                break;

            case State.Returning:

                Move(originalPos, originalRot, () =>
                {
                    player.ChangeGameMode(false);
                    player.SyncRotationFromCurrentTransform();

                    player.LockPlayer(false);
                    player.LockCamera(false);

                    GameManager.Instance.currentState = GameManager.GameState.Gameplay;

                    state = State.Idle;
                });

                break;
        }
    }

    void StartReturn()
    {
        player.LockCamera(true);

        startPos = playerTransform.position;
        startRot = playerTransform.rotation;

        currentMoveTime = 0f;
        state = State.Returning;
    }

    void Move(Vector3 targetPos, Quaternion targetRot, System.Action onFinish)
    {
        currentMoveTime += Time.deltaTime;

        float pct = Mathf.Clamp01(currentMoveTime / timeToMove);
        float smoothPct = Mathf.SmoothStep(0f, 1f, pct);

        playerTransform.position = Vector3.Lerp(startPos, targetPos, smoothPct);
        playerTransform.rotation = Quaternion.Lerp(startRot, targetRot, smoothPct);

        if (pct >= 1f)
        {
            onFinish?.Invoke();

            if (isAction && hasWatchedTV)
                OnActionExecuted();
        }
    }
}