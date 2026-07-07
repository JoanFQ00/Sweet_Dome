using System;
using System.Collections;
using UnityEngine;

public class MiniGameTest : MonoBehaviour, IInteractable, ILookable
{
    public enum MiniGameState
    {
        None,
        Entering,
        Active,
        Exiting
    }

    [Header("Camera & Transition")]
    public Transform gameCameraPosition;
    public Camera playerCamera;
    public float timeToMove = 1.5f;

    [Header("Interaction Settings")]
    public LayerMask interactLayer;
    public float maxDist = 50f;

    [Header("4 Cursores Finales (Texturas)")]
    public Texture2D originalCursor;
    public Texture2D hoverInteractCursor;
    public Texture2D grabCursor;
    public Texture2D rotateCursor;

    [Header("Hotspot Offset (Opcional)")]
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    [Header("Pop Up")]
    [SerializeField] private float distanceToPop = 1.6f;
    [SerializeField] private GameObject popUpGO;
    [SerializeField] private GameObject interactPopUpGO;

    [Header("Chair Rotation Settings")]
    [SerializeField] private float billboardSmoothSpeed = 5f;
    [SerializeField] private float returnRotationSpeed = 3f;

    private Quaternion originalChairRotation;
    private Coroutine centerChairCoroutine;

    private PlayerController playerController;
    private bool onSight;

    public static Action<bool> OnMachineEntered;

    private MiniGameState state = MiniGameState.None;
    private bool interacting;

    private float currentMoveTime;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private Transform playerTransform;
    private HL_Interact currentHighlight;

    public bool isClickingUI = false;

    private MainButt activePressedButton = null;
    private Texture2D currentAppliedCursor;

    private bool isChairRotatingProcess = false;

    private void Start()
    {
        originalChairRotation = transform.rotation;

        if (popUpGO != null)
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
            playerController = GameManager.Instance.playerController;
        }
    }

    void Update()
    {
        if (popUpGO != null)
            PopUpCheck();

        if (state == MiniGameState.None && DayTime.Instance.GetCurrentHour() < 21)
        {
            HandleChairBillboard();
        }

        if (GameManager.Instance.currentState != GameManager.GameState.Minigame && state == MiniGameState.None)
            return;

        CheckHour();

        if (state == MiniGameState.Active)
        {
            HandleActiveInput();
            UpdateCursorAndHoverState();
        }

        switch (state)
        {
            case MiniGameState.Entering:
                HandleCameraTransition(true);
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

    void HandleChairBillboard()
    {
        if (playerController == null) return;

        Vector3 directionToPlayer = playerController.transform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, 90f, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * billboardSmoothSpeed);
        }
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

        playerController.enabled = false;

        currentMoveTime = 0f;
        state = MiniGameState.Entering;

        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.chairEnter, gameObject);

        ShowCursor(true);
        ChangeCursor(originalCursor);
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void HandleCameraTransition(bool entering)
    {
        if (playerTransform == null || gameCameraPosition == null) return;

        currentMoveTime += Time.deltaTime;
        float pct = currentMoveTime / timeToMove;

        if (entering)
        {
            GameManager.Instance.playerController.LockCamera(true);
            GameManager.Instance.playerController.ChangeGameMode(false);

            playerTransform.position = Vector3.Lerp(originalPlayerPos, gameCameraPosition.position, pct);
            playerTransform.rotation = Quaternion.Slerp(originalPlayerRot, gameCameraPosition.rotation, pct);

            if (pct >= 1f)
            {
                currentMoveTime = 0f;
                state = MiniGameState.Active;

                playerController.ForceRotation(gameCameraPosition.rotation);

                GameManager.Instance.playerController.LockCamera(true);
                GameManager.Instance.playerController.ChangeGameMode(false);

                // Iniciamos la nueva secuencia de rotación controlada por tiempo continuo
                if (centerChairCoroutine != null) StopCoroutine(centerChairCoroutine);
                centerChairCoroutine = StartCoroutine(WaitAndRotateTogether());
            }
        }
        else
        {
            playerTransform.position = Vector3.Lerp(gameCameraPosition.position, originalPlayerPos, pct);
            playerTransform.rotation = Quaternion.Lerp(playerTransform.rotation, originalPlayerRot, pct);
            if (pct >= 1f) FinishExit();
        }
    }

    IEnumerator WaitAndRotateTogether()
    {
        isChairRotatingProcess = true;

        // Forzamos candado a la cámara durante los 0.7 segundos iniciales de espera
        GameManager.Instance.playerController.LockCamera(true);
        playerController.enabled = false;

        yield return new WaitForSeconds(0.7f);

        // Guardamos los puntos de inicio exactos antes de arrancar los 2 segundos de rotación
        Quaternion startChairRot = transform.rotation;
        float elapsedRotationTime = 0f;
        float rotationDuration = 2.0f; // 2 segundos exactos girando sin poder mover la cámara

        while (elapsedRotationTime < rotationDuration)
        {
            elapsedRotationTime += Time.deltaTime;
            float pct = Mathf.Clamp01(elapsedRotationTime / rotationDuration);

            // Suavizado smoothstep para que la aceleración y frenada del giro se sientan orgánicas
            pct = pct * pct * (3f - 2f * pct);

            // Rotamos la silla hacia su origen geométrico
            transform.rotation = Quaternion.Slerp(startChairRot, originalChairRotation, pct);

            // Bloqueamos la cámara frame a frame obligando al player a imitar el nodo gameCameraPosition
            if (playerTransform != null && gameCameraPosition != null)
            {
                playerTransform.rotation = gameCameraPosition.rotation;
                playerController.ForceRotation(gameCameraPosition.rotation);
                GameManager.Instance.playerController.LockCamera(true);
            }

            yield return null;
        }

        // Aseguramos el final perfecto del eje de rotación
        transform.rotation = originalChairRotation;
        if (playerTransform != null && gameCameraPosition != null)
        {
            playerTransform.rotation = gameCameraPosition.rotation;
            playerController.ForceRotation(gameCameraPosition.rotation);
        }

        // Liberamos el proceso de la silla
        isChairRotatingProcess = false;

        // Mantenemos el bloqueo de cámara por defecto para interactuar con los botones de la cabina
        GameManager.Instance.playerController.LockCamera(true);
    }

    void HandleActiveInput()
    {
        // Bloqueamos la salida rápida o cualquier acción si la silla está en medio del proceso inicial de giro
        if (isChairRotatingProcess) return;

        if (Input.GetKeyDown(KeyCode.E) && GameManager.Instance.dayTime.currentHour >= 21)
        {
            GameManager.Instance.playerController.ChangeGameMode(false);
            ExitMiniGame();
            return;
        }

        HandleCamMove();

        if (!Input.GetMouseButton(1))
        {
            HandleClick();
        }
    }

    void HandleCamMove()
    {
        // Bloqueamos la posibilidad de mover la cámara con Click Derecho si la silla aún no ha terminado de colocarse
        if (isChairRotatingProcess)
        {
            GameManager.Instance.playerController.LockCamera(true);
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            playerController.enabled = true;
            playerController.LockCamera(false);
            playerController.ChangeGameMode(true);

            ReleaseActiveButton();
        }

        if (Input.GetMouseButtonUp(1))
        {
            playerController.LockCamera(true);
            playerController.ChangeGameMode(false);
            playerController.enabled = false;
        }
    }

    void HandleClick()
    {
        if (playerCamera == null || isChairRotatingProcess) return;

        if (Input.GetMouseButtonDown(0))
        {
            interacting = true;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
            {
                if (hit.collider.TryGetComponent(out MainButt button))
                {
                    if (button.Validator != null && !button.Validator.CanPress())
                    {
                        return;
                    }
                    activePressedButton = button;
                    activePressedButton.Press();
                }
                else if (hit.collider.TryGetComponent(out SpherePoints points))
                {
                    points.OnClick();
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (activePressedButton != null)
            {
                activePressedButton.HoldUpdate();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            interacting = false;
            ReleaseActiveButton();
        }
    }

    void UpdateCursorAndHoverState()
    {
        if (playerCamera == null || isChairRotatingProcess) return;

        if (Input.GetMouseButton(1))
        {
            ClearCurrentHighlight();
            ChangeCursor(rotateCursor);
            return;
        }

        if (Input.GetMouseButton(0) && interacting)
        {
            ChangeCursor(grabCursor);
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
        {
            bool isInteractableNow = true;

            if (hit.collider.TryGetComponent(out MainButt button))
            {
                if (button.Validator != null && !button.Validator.CanPress())
                {
                    isInteractableNow = false;
                }
            }
            else if (hit.collider.TryGetComponent(out DialRotation dial))
            {
                if (!dial.IsInteractable()) isInteractableNow = false;
            }
            else if (hit.collider.TryGetComponent(out WheelRotator radarWheel))
            {
                if (!radarWheel.IsInteractable()) isInteractableNow = false;
            }
            else if (hit.collider.TryGetComponent(out SliderCont slider))
            {
                if (!slider.IsInteractable()) isInteractableNow = false;
            }

            if (isInteractableNow)
            {
                ChangeCursor(hoverInteractCursor);

                HL_Interact newHighlight = hit.collider.GetComponent<HL_Interact>();
                if (currentHighlight != newHighlight)
                {
                    if (currentHighlight != null) currentHighlight.ResetColor();
                    currentHighlight = newHighlight;
                    if (currentHighlight != null) currentHighlight.LightenColor();
                }
            }
            else
            {
                ClearCurrentHighlight();
                ChangeCursor(originalCursor);
            }
        }
        else
        {
            ClearCurrentHighlight();
            ChangeCursor(originalCursor);
        }
    }

    private void ReleaseActiveButton()
    {
        if (activePressedButton != null)
        {
            activePressedButton.Release();
            activePressedButton = null;
        }
    }

    private void ClearCurrentHighlight()
    {
        if (currentHighlight != null)
        {
            currentHighlight.ResetColor();
            currentHighlight = null;
        }
    }

    void ExitMiniGame()
    {
        if (state != MiniGameState.Active || isChairRotatingProcess) return;
        if (centerChairCoroutine != null) StopCoroutine(centerChairCoroutine);

        OnMachineEntered?.Invoke(false);
        state = MiniGameState.Exiting;
        currentMoveTime = 0f;
        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.chairExit, gameObject);
        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void ForceExitMiniGameFromSystem()
    {
        if (state != MiniGameState.Active) return;
        if (centerChairCoroutine != null) StopCoroutine(centerChairCoroutine);

        OnMachineEntered?.Invoke(false);
        state = MiniGameState.Exiting;
        currentMoveTime = 0f;
        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.chairExit, gameObject);
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

        playerController.enabled = true;
        GameManager.Instance.playerController.ChangeGameMode(false);
        GameManager.Instance.playerController.SetRotationFromCamera();
        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        ChangeCursor(originalCursor);
        ShowCursor(false);
        isChairRotatingProcess = false;
    }

    void ShowCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    void ActivateChairLayer(Day c_Day) => gameObject.layer = LayerMask.NameToLayer("Interactable");

    void CheckHour()
    {
        if (DayTime.Instance.GetCurrentHour() >= 21)
            gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void ChangeCursor(Texture2D cursorTexture)
    {
        if (currentAppliedCursor == cursorTexture) return;

        currentAppliedCursor = cursorTexture;
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    private void PopUpCheck()
    {
        if (playerController == null)
        {
            playerController = GameManager.Instance.playerController;
            return;
        }

        if (GameManager.Instance.currentState != GameManager.GameState.Gameplay)
        {
            SetPopUps(false, false);
            return;
        }

        if (DayTime.Instance.GetCurrentHour() < 21)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

            if (onSight)
            {
                SetPopUps(false, true);

                if (playerCamera != null)
                    interactPopUpGO.transform.rotation = playerCamera.transform.rotation;
            }
            else if (distanceToPlayer <= distanceToPop)
            {
                SetPopUps(true, false);

                if (playerCamera != null)
                    popUpGO.transform.rotation = playerCamera.transform.rotation;
            }
            else
            {
                SetPopUps(false, false);
            }
        }
        else
        {
            SetPopUps(false, false);
        }
    }

    private void SetPopUps(bool showNormal, bool showInteract)
    {
        if (popUpGO.activeSelf != showNormal)
            popUpGO.SetActive(showNormal);

        if (interactPopUpGO.activeSelf != showInteract)
            interactPopUpGO.SetActive(showInteract);
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }
}