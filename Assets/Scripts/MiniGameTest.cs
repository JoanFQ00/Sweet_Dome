using System;
using System.Security.Cryptography;
using UnityEngine;

public class MiniGameTest : MonoBehaviour, IInteractable
{
    public enum MiniGameState
    {
        None,
        Entering,
        Active,
        Exiting
    }

    [Header("Camera")]
    public Transform gameCameraPosition;
    public Camera playerCamera;
    public float timeToMove = 1.5f;

    [Header("Interaction")]
    public LayerMask interactLayer;
    public float maxDist = 50f;
    public Texture2D rotateCursor;
    public Texture2D originalCursor;

    public static Action<bool> OnMachineEntered;

    private MiniGameState state = MiniGameState.None;

    private float currentMoveTime;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private Transform playerTransform;
    private HL_Interact currentHighlight;

    public bool isClickingUI = false;

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Minigame &&
            state == MiniGameState.None)
            return;

        CheckHour();
        CheckHover();

        switch (state)
        {
            case MiniGameState.Entering:
                HandleCameraTransition(true);
                break;

            case MiniGameState.Active:
                HandleActiveInput();
                break;

            case MiniGameState.Exiting:
                HandleCameraTransition(false);
                break;
        }
    }

    private void OnEnable()
    {
        GameManager.OnChangeDay += ActivateChairLayer;
    }

    private void OnDisable()
    {
        GameManager.OnChangeDay -= ActivateChairLayer;
    }

    public void Interact()
    {
        if (state != MiniGameState.None) return;

        OnMachineEntered?.Invoke(true);

        GameManager.Instance.currentState = GameManager.GameState.Minigame;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        playerCamera = GameManager.Instance.playerController.mainCamera;
        if (playerCamera == null) return;

        playerTransform = GameManager.Instance.playerController.transform;

        originalPlayerPos = playerTransform.position;
        originalPlayerRot = playerTransform.rotation;

        currentMoveTime = 0f;
        state = MiniGameState.Entering;

        ShowCursor(true);

        gameObject.layer = LayerMask.NameToLayer("Default");

        BeginDayInstructions();
    }

    void HandleCameraTransition(bool entering)
    {
        if (playerTransform == null) return;
        if (gameCameraPosition == null) return;

        currentMoveTime += Time.deltaTime;
        float pct = currentMoveTime / timeToMove;

        if (entering)
        {
            playerTransform.position = Vector3.Lerp(originalPlayerPos,gameCameraPosition.position,pct);
            playerTransform.rotation = Quaternion.Lerp(originalPlayerRot,gameCameraPosition.rotation,pct);

            if (pct >= 1f)
            {
                currentMoveTime = 0f;
                state = MiniGameState.Active;
            }
        }
        else
        {
            playerTransform.position = Vector3.Lerp(gameCameraPosition.position,originalPlayerPos,pct);
            playerTransform.rotation = Quaternion.Lerp(gameCameraPosition.rotation,originalPlayerRot,pct);

            if (pct >= 1f)
            {
                FinishExit();
            }
        }
    }

    void HandleActiveInput()
    {
        if (Input.GetKeyDown(KeyCode.E)
            && GameManager.Instance.dayTime.currentHour >= 21)
        {
            GameManager.Instance.playerController.ChangeGameMode(false);
            ExitMiniGame();
        }

        HandleCamMove();
        HandleClick();
    }

    void HandleCamMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.playerController.LockCamera(false);
            GameManager.Instance.playerController.ChangeGameMode(true);
            ChangeCursor(rotateCursor);
        }

        if (Input.GetMouseButtonUp(1))
        {
            GameManager.Instance.playerController.LockCamera(true);
            GameManager.Instance.playerController.ChangeGameMode(false);
            ChangeCursor(originalCursor);
        }
    }

    void HandleClick()
    {
        if (playerCamera == null) return;

        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
        {
            if (hit.collider.TryGetComponent(out SpherePoints points))
                points.OnClick();

            if (hit.collider.TryGetComponent(out MainButt button))
                button.Press();
        }
    }


    void CheckHover()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
        {
            HL_Interact newHighlight = hit.collider.GetComponent<HL_Interact>();

            if (currentHighlight != newHighlight)
            {
                if (currentHighlight != null)
                {
                    currentHighlight.ResetColor();
                }

                currentHighlight = newHighlight;

                if (currentHighlight != null)
                {
                    currentHighlight.LightenColor();
                }
            }
        }
        else
        {
            if (currentHighlight != null)
            {
                currentHighlight.ResetColor();
                currentHighlight = null;
            }
        }
    }

    void ExitMiniGame()
    {
        if (state != MiniGameState.Active) return;

        OnMachineEntered?.Invoke(false);

        state = MiniGameState.Exiting;
        currentMoveTime = 0f;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    void FinishExit()
    {
        state = MiniGameState.None;

        GameManager.Instance.currentState = GameManager.GameState.Gameplay;

        if (playerTransform != null)
        {
            playerTransform.position = originalPlayerPos;
            playerTransform.rotation = originalPlayerRot;
        }

        GameManager.Instance.playerController.ChangeGameMode(false);

        GameManager.Instance.playerController.SetRotationFromCamera();

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        ChangeCursor(originalCursor);
        ShowCursor(false);
    }

    void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    void ActivateChairLayer(Day c_Day)  
    {
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    void CheckHour()
    {
        if (DayTime.Instance.GetCurrentHour() >= 21)
            gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void ChangeCursor(Texture2D cursorTexture) 
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    void BeginDayInstructions() 
    {
        if (GameManager.Instance.currentDay == Day.Day1) 
        {
            GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.ExplainMachineDay1);
            GameManager.Instance.presiSpeaker.PresiSpeaks();
        }

        if (GameManager.Instance.currentDay == Day.Day2) 
        {
            GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.ExplainMachineDay2);
            GameManager.Instance.presiSpeaker.PresiSpeaks();
        }

        if (GameManager.Instance.currentDay == Day.Day3) 
        {
            GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.ExplainMachineDay3);
            GameManager.Instance.presiSpeaker.PresiSpeaks();
        }

        if (GameManager.Instance.currentDay == Day.Day4)
        {
            GameManager.Instance.presiSpeaker.ChangeDialogueID((int)PresiDialogue.ExplainMachineDay4);
            GameManager.Instance.presiSpeaker.PresiSpeaks();
        }
    }
}