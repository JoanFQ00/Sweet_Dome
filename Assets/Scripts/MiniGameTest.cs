using UnityEngine;

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

    public SphereMinigame sphereRotation;

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

        sphereRotation.SetActive(true);
    }

    void UpdateCameraPos(bool gameOn) 
    {
        if (playerCamera == null) return;

        if (gameOn)
        {
            currenMoveTime += Time.deltaTime;
            float pct = currenMoveTime / timeToMove;

            playerCamera.transform.position = Vector3.Lerp(originalCameraPos, gameCameraPosition.position, pct);
            playerCamera.transform.rotation = Quaternion.Lerp(originalCameraRot, gameCameraPosition.rotation, pct);

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

        playerCamera.transform.position = originalCameraPos;
        playerCamera.transform.rotation = originalCameraRot;
        GameManager.Instance.playerController.LockPlayer(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sphereRotation.SetActive(true);
    }
}
