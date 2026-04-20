using UnityEngine;
using UnityEngine.InputSystem;

public class SphereMinigame : MonoBehaviour
{
    public float rotationSpeed = 5f;

    private PlayerInput playerInput;
    private InputAction lookAction;
    private MiniGameTest miniGame;
    public GameObject pointPrefab;
    public int numberOfPoints = 5;

    private bool isActive = false;

    float yaw;
    float pitch;

    private void Start()
    {
        playerInput = GameManager.Instance.playerController.GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        miniGame = GetComponentInParent<MiniGameTest>();

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 direction = Random.onUnitSphere;

            GameObject pointSphere = Instantiate(pointPrefab, transform);
            pointSphere.transform.localPosition = direction * 0.5f;
        }
    }

    public void SetActive(bool activation)
    {
        isActive = activation;
    }

    void Update()
    {
        if (!isActive)
            return;

        if (Mouse.current.leftButton.isPressed && !miniGame.isClickingUI)
        {
            Debug.Log("Arrastrando");
            Vector2 mouse = lookAction.ReadValue<Vector2>();
            Debug.Log("Mouse: " + mouse);

            yaw -= mouse.x * rotationSpeed;
            pitch += mouse.y * rotationSpeed;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
