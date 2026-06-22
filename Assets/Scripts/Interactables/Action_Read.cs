using UnityEngine;

public class Action_Reader : BaseAction
{
    public float timeToMove = 1.0f;
    public bool isAction = false;

    public enum BookType { Dorothy, President}
    public BookType bookType;

    private float currentMoveTime = 0.0f;

    private Vector3 readPos;
    private Quaternion readRot;

    private PlayerController player;

    public Renderer bookRenderer;
    public Texture2D[] bookTextures;

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

            if (bookRenderer == null)
            {
                bookRenderer = GetComponentInChildren<Renderer>();
                if (bookType == BookType.President)
                    ChangeTexture(GameManager.Instance.currentDay);
            }

            originalPos = transform.position;
            originalRot = transform.rotation;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        LevelManager.OnLevelLoaded += LevelLoaded;
        GameManager.OnChangeDay += ChangeTexture;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        LevelManager.OnLevelLoaded -= LevelLoaded;
        GameManager.OnChangeDay -= ChangeTexture;
    }

    

    void LevelLoaded() 
    {
        player = GameManager.Instance.playerController;

        bookRenderer = GetComponentInChildren<Renderer>();

        if (bookType == BookType.President)
            ChangeTexture(GameManager.Instance.currentDay);

        originalPos = transform.position;
        originalRot = transform.rotation;
    }

    public override void Interact()
    {
        if (state != State.Idle) return;

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        startPos = transform.position;
        startRot = transform.rotation;

        readPos = player.GetComponent<Interact>().readTransform.position;
        readRot = player.GetComponent<Interact>().readTransform.rotation;

        player.LockPlayer(true);

        currentMoveTime = 0f;
        state = State.MovingToRead;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToRead:
                Move(readPos, readRot, () =>
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
                    {
                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.PresiSaysYouCanWatchTv);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");
                        OnActionExecuted();
                    }
                });
                break;
        }
    }

    public void ChangeTexture(Day currentDay)
    {
        if (bookRenderer == null)
        {
            bookRenderer = GetComponentInChildren<Renderer>(true);
        }

        if (bookRenderer == null) return;

        int currentDayIdx = (int)currentDay;
        if (bookTextures != null && currentDayIdx >= 0 && currentDayIdx < bookTextures.Length)
        {
            bookRenderer.material.mainTexture = bookTextures[currentDayIdx];
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