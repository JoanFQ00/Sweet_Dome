using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
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
    public static bool isInMinigame;

    public bool isClickingUI = false;

    public SphereMinigame sphereRotation;
    public LeverGame leverGame;
    public Slider slider;

    PlayerInput playerInput;

    void Start()
    {
        gameOn = false;
        cameraOnPositon = false;
        currenMoveTime = 0;
        playerInput = GameManager.Instance.playerController.GetComponent<PlayerInput>();
    }

    void Update()
    {        
        //if (!cameraOnPositon)
            //UpdateCameraPos(gameOn);

        if (gameOn && Input.GetKeyDown(KeyCode.Escape)) // AIXO ES PER FER EL TEST ,,,, CAL CAMBIAR AL INPUT ACTUAL
        {
            ExitMiniGame();
        }

        if (isInMinigame)
            HandleClick();
        //if (leverGame != null && slider != null)
        //{
        //    slider.value = leverGame.value;
        //}
    }

    public void Interact()
    {
        if (isInMinigame) return;

        isInMinigame = true;
        GameManager.Instance.playerController.LockPlayer(true);

        playerCamera = GameManager.Instance.playerController.mainCamera;
        if (playerCamera == null) return;

        originalCameraPos = playerCamera.transform.position;
        originalCameraRot = playerCamera.transform.rotation;

        playerCamera.transform.position = gameCameraPosition.position;
        playerCamera.transform.rotation = gameCameraPosition.rotation;

        gameOn = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sphereRotation != null)
            sphereRotation.SetActive(true);
    }

    //public void Interact() 
    //{

    //    if (isInMinigame) return;
    //    Debug.Log("ENTER MINIGAME");

    //    isInMinigame = true;
    //    GameManager.Instance.playerController.LockPlayer(true);

    //    playerCamera = GameManager.Instance.playerController.mainCamera;

    //    if (playerCamera == null) return;

    //    gameOn = true;
    //    cameraOnPositon = false;

    //    originalCameraPos = playerCamera.transform.position;
    //    originalCameraRot = playerCamera.transform.rotation;

    //    playerCamera.transform.position = gameCameraPosition.transform.position;
    //    playerCamera.transform.rotation = gameCameraPosition.transform.rotation;

    //    Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;

    //    if (sphereRotation != null) 
    //    {
    //        sphereRotation.SetActive(true);
    //    }
    //}

    //void UpdateCameraPos(bool gameOn) 
    //{
    //    if (playerCamera == null) return;

    //    if (gameOn)
    //    {
    //        playerCamera.transform.position = gameCameraPosition.transform.position;
    //        playerCamera.transform.rotation = gameCameraPosition.transform.rotation;
    //        //currenMoveTime += Time.deltaTime;
    //        //float pct = currenMoveTime / timeToMove;

    //        //playerCamera.transform.position = Vector3.Lerp(originalCameraPos, gameCameraPosition.position, pct);
    //        //playerCamera.transform.rotation = Quaternion.Lerp(originalCameraRot, gameCameraPosition.rotation, pct);

    //        //if (pct >= 1.0f)
    //        //{
    //        //    currenMoveTime = 0f;
    //        //    cameraOnPositon = true;
    //        //}
    //    }

    //    else 
    //    {
    //        playerCamera.transform.position = originalCameraPos;
    //        playerCamera.transform.rotation = originalCameraRot;
    //        //currenMoveTime += Time.deltaTime;
    //        //float pct = currenMoveTime / timeToMove;

    //        //playerCamera.transform.position = Vector3.Lerp(gameCameraPosition.position, originalCameraPos, pct);
    //        //playerCamera.transform.rotation = Quaternion.Lerp(gameCameraPosition.rotation, originalCameraRot, pct);

    //        //if (pct >= 1.0f)
    //        //{
    //        //    currenMoveTime = 0f;
    //        //    cameraOnPositon = true;
    //        //}
    //    }
    //}

    void ExitMiniGame()
    {
        gameOn = false;

        playerCamera.transform.position = originalCameraPos;
        playerCamera.transform.rotation = originalCameraRot;

        GameManager.Instance.playerController.LockPlayer(false); 

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (sphereRotation != null)
            sphereRotation.SetActive(false);

        isInMinigame = false;
    }

    //void ExitMiniGame()
    //{
    //    gameOn = false;
    //    cameraOnPositon = false;
    //    currenMoveTime = 0f;

    //    playerCamera.transform.position = originalCameraPos;
    //    playerCamera.transform.rotation = originalCameraRot;
    //    GameManager.Instance.playerController.LockPlayer(true);

    //    Cursor.lockState = CursorLockMode.Locked;
    //    Cursor.visible = false;

    //    if (sphereRotation != null) 
    //    {
    //        sphereRotation.SetActive(false);
    //    }

    //    isInMinigame = false;
    //}

    void HandleClick()
    {
        if (playerCamera == null) return;

        isClickingUI = false;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit.collider.name);

                //Button
                ButtonGame button = hit.collider.GetComponent<ButtonGame>();
                if (button != null)
                {
                    button.Press();
                }
                SpherePoints points = hit.collider.GetComponent<SpherePoints>();
                if (points != null)
                {
                    points.OnClick();
                }
            }
        }
    }
}