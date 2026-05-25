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

    private MiniGameState state = MiniGameState.None;

    private float currentMoveTime;
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private HL_Interact currentHighlight;

    public bool isClickingUI = false;

    void Update()
    {
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

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        playerCamera = GameManager.Instance.playerController.mainCamera;
        if (playerCamera == null) return;

        originalCameraPos = playerCamera.transform.position;
        originalCameraRot = playerCamera.transform.rotation;

        currentMoveTime = 0f;
        state = MiniGameState.Entering;

        ShowCursor(true);

        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void HandleCameraTransition(bool entering)
    {
        if (playerCamera == null) return;

        currentMoveTime += Time.deltaTime;
        float pct = currentMoveTime / timeToMove;

        if (entering)
        {
            playerCamera.transform.position =
                Vector3.Lerp(originalCameraPos, gameCameraPosition.position, pct);

            playerCamera.transform.rotation =
                Quaternion.Lerp(originalCameraRot, gameCameraPosition.rotation, pct);

            if (pct >= 1f)
            {
                currentMoveTime = 0f;
                state = MiniGameState.Active;
            }
        }
        else
        {
            playerCamera.transform.position =
                Vector3.Lerp(gameCameraPosition.position, originalCameraPos, pct);

            playerCamera.transform.rotation =
                Quaternion.Lerp(gameCameraPosition.rotation, originalCameraRot, pct);

            if (pct >= 1f)
            {
                FinishExit();
            }
        }
    }

    void HandleActiveInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
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
        }

        if (Input.GetMouseButtonUp(1))
        {
            GameManager.Instance.playerController.LockCamera(true);
            GameManager.Instance.playerController.ChangeGameMode(false);
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

        state = MiniGameState.Exiting;
        currentMoveTime = 0f;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        gameObject.layer = LayerMask.NameToLayer("Interactable");

        GameManager.Instance.playerController.GetComponent<Interact>().ChangeInteracting(false);
    }

    void FinishExit()
    {
        state = MiniGameState.None;

        playerCamera.transform.position = originalCameraPos;
        playerCamera.transform.rotation = originalCameraRot;
        GameManager.Instance.playerController.SetRotationFromCamera();

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

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
}