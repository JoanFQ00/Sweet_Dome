using UnityEngine;

public class Action_Reader : BaseAction
{
    public float timeToMove = 1.0f;
    public bool isAction = false;

    private float currentMoveTime = 0.0f;

    private Transform readTransform;

    private PlayerController player;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private enum State
    {
        Idle,
        MovingToRead,
        Reading,
        Returning
    }

    private State state = State.Idle;

    void Start()
    {
        if (GameManager.Instance.playerController != null)
        { 
            player = GameManager.Instance.playerController;
            readTransform = player.GetComponent<Interact>().readTransform;

            originalPos = transform.position;
            originalRot = transform.rotation;
        }
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
        player = GameManager.Instance.playerController;
        readTransform = player.GetComponent<Interact>().readTransform;

        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        startPos = transform.position;
        startRot = transform.rotation;

        player.LockPlayer(true);

        currentMoveTime = 0f;
        state = State.MovingToRead;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToRead:
                Move(readTransform.position, readTransform.rotation, () =>
                {
                    state = State.Reading;
                });
                break;

            case State.Reading:
                gameObject.layer = LayerMask.NameToLayer("Default");

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
                    gameObject.layer = LayerMask.NameToLayer("Interactable");

                    GameManager.Instance.currentState = GameManager.GameState.Gameplay;
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

        transform.position = Vector3.Lerp(startPos, targetPos, pct);
        transform.rotation = Quaternion.Lerp(startRot, targetRot, pct);

        if (pct >= 1.0f)
        {
            onFinish?.Invoke();
        }
    }
}