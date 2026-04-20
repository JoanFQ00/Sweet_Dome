using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    public Camera mainCamera;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public bool cameraMovement = true;

    [Header("Mouse Movement")]
    public Transform pitchController;
    public float yawSpeed = 120f;
    public float pitchSpeed = 120f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    float yaw;
    float pitch;

    bool playerLocked;
    PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerLocked = false;
    }

    void Start()
    {
        GameManager.Instance.SetPlayer(this);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LockPlayer(bool locked) { playerLocked = locked; }

    void Update()
    {
        if (playerLocked) return;

        Move();
        if (cameraMovement)
        {
            Look();
        }
    }

    //Movement Character method
    void Move()
    {
        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 move = forward * input.y + right * input.x;
        move.Normalize();

        move *= walkSpeed;

        characterController.Move(move * Time.deltaTime);
    }

    //Movement Camera method
    void Look()
    {
        Vector2 mouse = playerInput.actions["Look"].ReadValue<Vector2>();

        yaw += mouse.x * yawSpeed * Time.deltaTime;
        pitch -= mouse.y * pitchSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        pitchController.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}