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
                WorldZoneButton worldButton = hit.collider.GetComponent<WorldZoneButton>();
                if (worldButton != null)
                {
                    worldButton.Press();
                    return;
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