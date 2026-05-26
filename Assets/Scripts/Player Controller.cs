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

    [Header("Head Bobbing")]
    public bool m_UseBobbing = true;
    [Range(0, 0.1f)] public float m_BobAmplitude = 0.015f;
    [Range(0, 30)] public float m_BobFrequency = 12.0f;
    public float m_BobSmooth = 8f;

    Vector3 cameraStartLocalPos;
    float bobTimer;

    [Header("Camera Zoom")]
    public float[] objectiveFov;
    public float timeToZoom = 1.5f;
    float currentZoomTime = 0f;
    bool zoomingCamera = false;
    int zoomIndex = 0;
    float targetFov;

    [HideInInspector]
    public bool playerLocked;

    PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        GameManager.Instance.SetPlayer(this);

        Cursor.lockState = CursorLockMode.Locked;

        targetFov = objectiveFov[zoomIndex];
        cameraStartLocalPos = pitchController.localPosition;
    }

    public void LockPlayer(bool locked) => playerLocked = locked;
    public void LockCamera(bool locked) => cameraLocked = locked;
    public void ChangeGameMode(bool mode) => miniGameMode = mode;

    void Update()
    {
        if (!playerLocked)
            Move();

        if (!cameraLocked)
            Look();

        if (m_UseBobbing)
            HandleHeadBob();

        float scroll = Input.mouseScrollDelta.y;

        if (scroll < 0) ChangeZoom(-1);
        else if (scroll > 0) ChangeZoom(1);

        if (zoomingCamera)
            ChangeCameraZoom();
    }

    void Move()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 move = (transform.forward * input.y + transform.right * input.x).normalized;
        move *= walkSpeed;

        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 finalMove = move + Vector3.up * verticalVelocity;
        characterController.Move(finalMove * Time.deltaTime);
    }

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

    void HandleHeadBob()
    {
        Vector3 horizontalVel = characterController.velocity;
        horizontalVel.y = 0f;

        float speed = horizontalVel.magnitude;

        if (speed > 0.1f && characterController.isGrounded)
        {
            bobTimer += Time.deltaTime * m_BobFrequency;

            float bobX = Mathf.Sin(bobTimer * 0.5f) * m_BobAmplitude;
            float bobY = Mathf.Sin(bobTimer) * m_BobAmplitude;

            Vector3 targetPos = cameraStartLocalPos + new Vector3(bobX, bobY, 0f);

            pitchController.localPosition = Vector3.Lerp(
                pitchController.localPosition,
                targetPos,
                Time.deltaTime * m_BobSmooth
            );
        }
        else
        {
            pitchController.localPosition = Vector3.Lerp(
                pitchController.localPosition,
                cameraStartLocalPos,
                Time.deltaTime * m_BobSmooth
            );
        }
    }

    void ChangeZoom(int direction)
    {
        zoomIndex = Mathf.Clamp(zoomIndex + direction, 0, objectiveFov.Length - 1);

        targetFov = objectiveFov[zoomIndex];
        currentZoomTime = 0f;
        zoomingCamera = true;
    }

    void ChangeCameraZoom()
    {
        currentZoomTime += Time.deltaTime;

        float pct = currentZoomTime / timeToZoom;

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, pct);

        if (pct >= 1f)
        {
            mainCamera.fieldOfView = targetFov;
            zoomingCamera = false;
        }
    }
}