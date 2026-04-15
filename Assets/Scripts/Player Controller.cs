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

    [Header("Mouse Movement")]
    public float sensitivity = 100f;
    public Transform pitchController;
    float yaw;
    float pitch;

    PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();
        Move();
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

        yaw += mouse.x * sensitivity * Time.deltaTime;
        pitch -= mouse.y * sensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -80f, 80f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        pitchController.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}