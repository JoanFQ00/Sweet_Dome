using System;
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

    [Header("Camera & Transition")]
    public Transform gameCameraPosition;
    public Camera playerCamera;
    public float timeToMove = 1.5f;

    [Header("Interaction Settings")]
    public LayerMask interactLayer;
    public float maxDist = 50f;

    [Header("4 Cursores Finales (Texturas)")]
    [Tooltip("Cursor 1: Puntero básico por defecto.")]
    public Texture2D originalCursor;
    [Tooltip("Cursor 2: Mano apuntando (Hover sobre elemento interactuable válido).")]
    public Texture2D hoverInteractCursor;
    [Tooltip("Cursor 3: Mano agarrando (Clic izquierdo o manteniendo pulsado).")]
    public Texture2D grabCursor;
    [Tooltip("Cursor 4: Ojo (Manteniendo clic derecho / Cámara libre).")]
    public Texture2D rotateCursor;

    [Header("Hotspot Offset (Opcional)")]
    [Tooltip("Punto de pivote del cursor. Para flechas suele ser (0,0), para manos o el ojo suele ser el centro de la textura.")]
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;

    public static Action<bool> OnMachineEntered;

    private MiniGameState state = MiniGameState.None;
    private bool interacting;

    private float currentMoveTime;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private Transform playerTransform;
    private HL_Interact currentHighlight;

    public bool isClickingUI = false;

    // Rastro de memoria del botón hundido
    private MainButt activePressedButton = null;

    // Estado interno para saber qué textura tiene el cursor aplicada ahora mismo y evitar llamadas redundantes a la GPU
    private Texture2D currentAppliedCursor;

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Minigame && state == MiniGameState.None)
            return;

        CheckHour();

        if (state == MiniGameState.Active)
        {
            HandleActiveInput();
            // El controlador de cursores procesa frame a frame las prioridades visuales
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

        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.chairEnter, gameObject);

        ShowCursor(true);
        ChangeCursor(originalCursor); // Empezamos con el puntero base
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void HandleCameraTransition(bool entering)
    {
        if (playerTransform == null || gameCameraPosition == null) return;

        currentMoveTime += Time.deltaTime;
        float pct = currentMoveTime / timeToMove;

        if (entering)
        {
            playerTransform.position = Vector3.Lerp(originalPlayerPos, gameCameraPosition.position, pct);
            playerTransform.rotation = Quaternion.Lerp(originalPlayerRot, gameCameraPosition.rotation, pct);
            if (pct >= 1f)
            {
                currentMoveTime = 0f;
                state = MiniGameState.Active;
            }
        }
        else
        {
            playerTransform.position = Vector3.Lerp(gameCameraPosition.position, originalPlayerPos, pct);
            playerTransform.rotation = Quaternion.Lerp(originalPlayerRot, originalPlayerRot, pct);
            if (pct >= 1f) FinishExit();
        }
    }

    void HandleActiveInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && GameManager.Instance.dayTime.currentHour >= 21)
        {
            GameManager.Instance.playerController.ChangeGameMode(false);
            ExitMiniGame();
            return;
        }

        HandleCamMove();

        // Solo permitimos clics e interacciones mecánicas si NO estamos mirando libremente con el clic derecho
        if (!Input.GetMouseButton(1))
        {
            HandleClick();
        }
    }

    void HandleCamMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.playerController.LockCamera(false);
            GameManager.Instance.playerController.ChangeGameMode(true);

            // Si estábamos interactuando con un botón al hacer clic derecho, lo liberamos por seguridad
            ReleaseActiveButton();
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

        // MOUSE DOWN: Click inicial
        if (Input.GetMouseButtonDown(0))
        {
            interacting = true;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
            {
                // Integramos la comprobación estructural: ¿Es un botón bloqueado por un validador?
                if (hit.collider.TryGetComponent(out MainButt button))
                {
                    // Si el botón tiene un validador y este dice que NO se puede pulsar, ignoramos el clic por completo
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

        // MOUSE HOLD: Mantener pulsado
        if (Input.GetMouseButton(0))
        {
            if (activePressedButton != null)
            {
                activePressedButton.HoldUpdate();
            }
        }

        // MOUSE UP: Soltar clic izquierdo
        if (Input.GetMouseButtonUp(0))
        {
            interacting = false;
            ReleaseActiveButton();
        }
    }

    /// <summary>
    /// Gestión unificada de la jerarquía de los 4 cursores y el coloreado de siluetas (Highlight).
    /// </summary>
    void UpdateCursorAndHoverState()
    {
        if (playerCamera == null) return;

        // PRIORIDAD 1: MÁXIMO RANGO - ¿Muriendo libremente la cámara con el Clic Derecho? (Cursor 4)
        if (Input.GetMouseButton(1))
        {
            ClearCurrentHighlight();
            ChangeCursor(rotateCursor);
            return;
        }

        // PRIORIDAD 2: ¿Manteniendo clic izquierdo sobre un elemento interactuado? (Cursor 3)
        if (Input.GetMouseButton(0) && interacting)
        {
            ChangeCursor(grabCursor);
            return;
        }

        // PRIORIDAD 3 y 4: Lanzamos el Raycast analítico para el Hover normal (Cursor 2 o Cursor 1)
        // ... Busca este fragmento exacto dentro del método UpdateCursorAndHoverState() de tu MiniGameTest.cs:

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDist, interactLayer))
        {
            bool isInteractableNow = true;

            // Análisis estructural del objeto apuntado:
            if (hit.collider.TryGetComponent(out MainButt button))
            {
                if (button.Validator != null && !button.Validator.CanPress())
                {
                    isInteractableNow = false;
                }
            }
            // NUEVA ADAPTACIÓN PARA LAS RUEDECITAS DE LA FRECUENCIA:
            else if (hit.collider.TryGetComponent(out DialRotation dial))
            {
                // Si chocamos contra un dial pero la lógica dice que no se puede mover, el cursor NO cambia a mano
                if (!dial.IsInteractable())
                {
                    isInteractableNow = false;
                }
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
                ChangeCursor(originalCursor); // Fuerza Cursor 1 (Flecha normal)
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
        if (state != MiniGameState.Active) return;
        OnMachineEntered?.Invoke(false);
        state = MiniGameState.Exiting;
        currentMoveTime = 0f;
        AudioManager.Instance.PlayOneShotAttached(AudioManager.Instance.chairExit, gameObject);
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

    void ActivateChairLayer(Day c_Day) => gameObject.layer = LayerMask.NameToLayer("Interactable");

    void CheckHour()
    {
        if (DayTime.Instance.GetCurrentHour() >= 21)
            gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void ChangeCursor(Texture2D cursorTexture)
    {
        // Optimizador: Si la textura ya está puesta, evitamos saturar de llamadas el bus gráfico de Unity
        if (currentAppliedCursor == cursorTexture) return;

        currentAppliedCursor = cursorTexture;
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }
}