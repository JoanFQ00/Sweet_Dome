using UnityEngine;

public class Action_WatchTV : BaseAction
{
    public float timeToMove = 1.0f;
    public Transform cameraTarget;
    public bool isAction = false;

    private Transform cam;
    private PlayerController player;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 originalPos;
    private Quaternion originalRot;

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
        cam = Camera.main.transform;

        if (GameManager.Instance.playerController != null)
        {
            player = GameManager.Instance.playerController;
        }
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        startPos = cam.position;
        startRot = cam.rotation;

        originalPos = cam.position;
        originalRot = cam.rotation;

        player.LockPlayer(true);

        currentMoveTime = 0f;
        state = State.MovingToTV;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToTV:
                Move(cameraTarget.position, cameraTarget.rotation, () =>
                {
                    state = State.WatchingPos;
                });
                break;

            case State.WatchingPos:

                if (Input.GetKeyDown(KeyCode.E))
                {
                    //Load Tv Image
                    state = State.Watching;
                }

                if (Input.GetMouseButtonDown(1)) 
                {

                }

                break;

            case State.Watching:

                if (Input.GetKeyDown(KeyCode.E)) 
                {
                    // Unload TV Image

                    startPos = cam.position;
                    startRot = cam.rotation;
                    currentMoveTime = 0f;
                    state = State.Returning;
                }
                break;

            case State.Returning:
                Move(originalPos, originalRot, () =>
                {
                    player.LockPlayer(false);

                    GameManager.Instance.currentState = GameManager.GameState.Gameplay;

                    GameManager.Instance.playerController
                        .GetComponent<Interact>()
                        .ChangeInteracting(false);

                    state = State.Idle;

                    if (isAction)
                        OnActionExecuted();
                });
                break;
        }
    }

    void Move(Vector3 targetPos, Quaternion targetRot, System.Action onFinish)
    {
        currentMoveTime += Time.deltaTime;

        float pct = Mathf.Clamp01(currentMoveTime / timeToMove);

        cam.position = Vector3.Lerp(startPos, targetPos, pct);
        cam.rotation = Quaternion.Lerp(startRot, targetRot, pct);

        if (pct >= 1f)
        {
            onFinish?.Invoke();
        }
    }
}