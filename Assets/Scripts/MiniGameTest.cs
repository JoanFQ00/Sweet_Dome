using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGameTest : MonoBehaviour, IInteractable
{
    public Transform gameCameraPosition;
    public Camera playerCamera;
    public float timeToMove = 1.5f;

    float currenMoveTime;
    Vector3 originalCameraPos;
    Quaternion originalCameraRot;
    bool gameOn;
    bool cameraOnPositon;

    public bool isClickingUI = false;

    public SphereMinigame sphereRotation;
    public LeverGame leverGame;
    public Slider slider;

    [Header("Camera Drag")]
    public float dragRadius = 40f;
    public float camSensitivity = 0.2f;

    Vector2 dragStartPos;
    bool isDragging;

    float yaw;
    float pitch;

    void Start()
    {
        gameOn = false;
        cameraOnPositon = false;
        currenMoveTime = 0;
    }

    void Update()
    {        
        if (!cameraOnPositon)
            UpdateCameraPos(gameOn);

        if (gameOn && Input.GetKeyDown(KeyCode.Escape)) // AIXO ES PER FER EL TEST ,,,, CAL CAMBIAR AL INPUT ACTUAL
        {
            ExitMiniGame();
        }

        if (gameOn)
        {
            HandleClick();
            HandleCamMove();
        }
    }

    public void Interact() 
    {
        GameManager.Instance.playerController.LockPlayer(true);
        playerCamera = GameManager.Instance.playerController.mainCamera;

        if (playerCamera == null) return;

        gameOn = true;
        cameraOnPositon = false;
        originalCameraPos = playerCamera.transform.position;
        originalCameraRot = playerCamera.transform.rotation;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Vector3 rot = playerCamera.transform.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;

        if (sphereRotation != null) 
        {
            sphereRotation.SetActive(true);
        }
    }

    void UpdateCameraPos(bool gameOn) 
    {
        if (playerCamera == null) return;

        if (gameOn)
        {
            currenMoveTime += Time.deltaTime;
            float pct = currenMoveTime / timeToMove;
            //para el alpha las animaciones
            playerCamera.transform.position = Vector3.Lerp(originalCameraPos, gameCameraPosition.position, pct);
            playerCamera.transform.rotation = Quaternion.Lerp(originalCameraRot, gameCameraPosition.rotation, pct);
            //Lerp del forward vector y no el del quaternion

            if (pct >= 1.0f)
            {
                currenMoveTime = 0f;
                cameraOnPositon = true;
            }
        }

        else 
        {
            currenMoveTime += Time.deltaTime;
            float pct = currenMoveTime / timeToMove;

            playerCamera.transform.position = Vector3.Lerp(gameCameraPosition.position, originalCameraPos, pct);
            playerCamera.transform.rotation = Quaternion.Lerp(gameCameraPosition.rotation, originalCameraRot, pct);

            if (pct >= 1.0f)
            {
                currenMoveTime = 0f;
                cameraOnPositon = true;
                GameManager.Instance.playerController.LockPlayer(false);
            }
        }
    }

    void ExitMiniGame()
    {
        gameOn = false;
        cameraOnPositon = false;
        currenMoveTime = 0f;

        playerCamera.transform.rotation = originalCameraRot;
        playerCamera.transform.position = originalCameraPos;
        GameManager.Instance.playerController.LockPlayer(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (sphereRotation != null) 
        {
            sphereRotation.SetActive(false);
        }
    }

 
    void HandleCamMove()
    {
        if (playerCamera == null) return;

        if (Input.GetMouseButtonDown(1))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector2 currentMousePos = Input.mousePosition;
        Vector2 delta = currentMousePos - dragStartPos;

        float distance = delta.magnitude;

        if (distance > dragRadius)
        {
            float excess = distance - dragRadius;
            Vector2 effectiveDelta = delta.normalized * excess;

            yaw += effectiveDelta.x * camSensitivity;
            pitch -= effectiveDelta.y * camSensitivity;

            pitch = Mathf.Clamp(pitch, -80f, 80f);

            playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
  


    void HandleClick()
    {
        if (playerCamera == null) return;

        isClickingUI = false;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                SpherePoints points = hit.collider.GetComponent<SpherePoints>();
                if (points != null)
                {
                    points.OnClick();
                }
         
                MainButt button = hit.collider.GetComponent<MainButt>();
                if (button != null) 
                {
                    button.Press();
                }
            }
        }
    }
}