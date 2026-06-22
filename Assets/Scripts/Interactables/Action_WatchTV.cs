using UnityEngine;
using UnityEngine.Video;
using System;

public class Action_WatchTV : BaseAction
{
    [Header("Movement Settings")]
    public float timeToMove = 1.0f;
    public Transform playerTarget;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RenderTexture videoTexture;
    public Texture blackTexture;
    public Renderer tvRenderer;
    public GameObject tvClickPopUp;

    private PlayerController player;
    private Transform playerTransform;
    private Vector3 startPos, originalPos;
    private Quaternion startRot, originalRot;

    private bool hasWatchedTV = false;
    private bool actionExecuted = false;
    private float currentMoveTime = 0f;

    private bool dorothyTurnedOff;

    private enum State { Idle, MovingToTV, WatchingPos, Watching, Returning }
    private State state = State.Idle;

    void Start()
    {
        player = GameManager.Instance.playerController;
        if (player != null) playerTransform = player.transform;
        tvClickPopUp.SetActive(false);
        TurnOffTV();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.NewDay += ResetWatchState;
        DayManager.OnTurnOffTV += () => dorothyTurnedOff = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameManager.NewDay -= ResetWatchState;
        DayManager.OnTurnOffTV += () => dorothyTurnedOff = true;
    }

    private void ResetWatchState() => hasWatchedTV = false;

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

    protected override bool IsReturning()
    {
        return state == State.Returning;
    }

    protected override void OnUpdateAction()
    {
        switch (state)
        {
            case State.MovingToTV:
                Move(playerTarget.position, playerTarget.rotation, () => {
                    player.SyncRotationFromCurrentTransform();
                    player.ChangeGameMode(true);
                    player.LockCamera(false);
                    state = State.WatchingPos;
                });
                break;

            case State.WatchingPos:
                if (Input.GetKeyDown(KeyCode.E)) StartReturn();

                bool canWatch = !hasWatchedTV && act.hourToInteract == GameManager.Instance.dayTime.currentHour;
                Ray ray = player.mainCamera.ScreenPointToRay(Input.mousePosition);
                bool isLookingAtTV = Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.collider.CompareTag("TV");

                if (canWatch)
                {
                    tvClickPopUp.SetActive(isLookingAtTV);
                    if (isLookingAtTV && Input.GetMouseButtonDown(0) && !dorothyTurnedOff)
                    {
                        TurnOnTV();
                        hasWatchedTV = true;
                        actionExecuted = false;
                        tvClickPopUp.SetActive(false);
                        state = State.Watching;
                    }
                }
                else { tvClickPopUp.SetActive(false); }
                break;

            case State.Watching:
                if (Input.GetKeyDown(KeyCode.E)) { TurnOffTV(); StartReturn(); }
                break;

            case State.Returning:
                Move(originalPos, originalRot, () => {
                    player.ChangeGameMode(false);
                    player.SyncRotationFromCurrentTransform();
                    player.LockPlayer(false);
                    player.LockCamera(false);
                    GameManager.Instance.currentState = GameManager.GameState.Gameplay;
                    state = State.Idle;
                });

                if (hasWatchedTV && !actionExecuted)
                {
                    OnActionExecuted(); 
                    actionExecuted = true;
                }
                break;
        }
    }

    void TurnOnTV()
    {
        if (tvRenderer != null) tvRenderer.material.mainTexture = videoTexture;
        videoPlayer.enabled = true;
        videoPlayer.Play();
        SoundManagerOwn.Instance.PlaySFX(SoundManagerOwn.Instance.tvSFX);
        GameManager.Instance.presiSpeaker.PresiSpeaks(600 + (int)GameManager.Instance.currentDay);
    }

    void TurnOffTV()
    {
        videoPlayer.Stop();
        videoPlayer.enabled = false;
        //SoundManagerOwn.Instance.StopSFX();
        if (tvRenderer != null) tvRenderer.material.mainTexture = blackTexture;
    }

    void StartReturn()
    {
        player.LockCamera(true);
        startPos = playerTransform.position;
        startRot = playerTransform.rotation;
        currentMoveTime = 0f;
        state = State.Returning;
    }

    void Move(Vector3 targetPos, Quaternion targetRot, Action onFinish)
    {
        currentMoveTime += Time.deltaTime;
        float pct = Mathf.Clamp01(currentMoveTime / timeToMove);
        playerTransform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, pct));
        playerTransform.rotation = Quaternion.Lerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, pct));
        if (pct >= 1f) onFinish?.Invoke();
    }
}