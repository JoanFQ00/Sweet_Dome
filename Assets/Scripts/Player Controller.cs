using System.Net.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Camera mainCamera;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public bool cameraLocked = false;
    bool miniGameMode = false;

    [Header("Gravity")]
    public float groundedGravity = -2f;
    float verticalVelocity;

    [Header("Mouse Movement")]
    public Transform pitchController;
    public float yawSpeed = 120f;
    public float pitchSpeed = 120f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    float yaw;
    float pitch;

    [Header("Camera Zoom")]
    public float[] objectiveFov;
    public float timeToZoom = 1.5f;
    float currentZoomTime = 0f;
    bool zoomingCamera = false;
    bool cameraOnZoom = false;
    float originalFov;
    int zoomIndex = 0;
    float targetFov;

    [HideInInspector]
    public bool playerLocked;
    PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerLocked = false;
        cameraLocked = false;
    }

    void Start()
    {
        GameManager.Instance.SetPlayer(this);
        Cursor.lockState = CursorLockMode.Locked;
        originalFov = mainCamera.fieldOfView;

        targetFov = objectiveFov[zoomIndex];
    }
    public void LockPlayer(bool locked) 
    {
        playerLocked = locked;
    }

    public void LockCamera(bool locked) 
    {
        cameraLocked = locked;
    }

    public void ChangeGameMode(bool mode) 
    {
        miniGameMode = mode;
    }

    void Update()
    {
        if (!playerLocked)
            Move();

        if (!cameraLocked)
            Look();

        float scroll = Input.mouseScrollDelta.y;

        if (scroll < 0)
        {
            ChangeZoom(-1);
        }
        else if (scroll > 0)
        {
            ChangeZoom(1); 
        }

        if (zoomingCamera)
            ChangeCameraZoom();
    }

    void Move()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 move = forward * input.y + right * input.x;
        move.Normalize();

        move *= walkSpeed;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = groundedGravity;
            }
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 movementCharacter = move + Vector3.up * verticalVelocity;

        characterController.Move(movementCharacter * Time.deltaTime);
    }

    //Movement Camera method
    void Look()
    {
        Vector2 mouse = playerInput.actions["Look"].ReadValue<Vector2>();

        yaw += mouse.x * yawSpeed * Time.deltaTime;
        pitch -= mouse.y * pitchSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (miniGameMode)
        {
            mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            pitchController.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    public void SetRotationFromCamera()
    {
        Vector3 euler = mainCamera.transform.rotation.eulerAngles;

        yaw = euler.y;

        pitch = euler.x;

        if (pitch > 180f)
            pitch -= 360f;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchController.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void ChangeZoom(int direction)
    {
        zoomIndex += direction;
        zoomIndex = Mathf.Clamp(zoomIndex, 0, objectiveFov.Length - 1);

        targetFov = objectiveFov[zoomIndex];

        currentZoomTime = 0f;
        zoomingCamera = true;
    }

    void ChangeCameraZoom()
    {
        currentZoomTime += Time.deltaTime;

        float pct = currentZoomTime / timeToZoom;

        float currentFov = mainCamera.fieldOfView;
        mainCamera.fieldOfView = Mathf.Lerp(currentFov, targetFov, pct);

        if (pct >= 1f)
        {
            mainCamera.fieldOfView = targetFov;
            zoomingCamera = false;
        }
    }
}