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

    }

    void UpdateCameraPos(bool state) 
    {
        if (playerCamera == null) return;

        currenMoveTime += Time.deltaTime;
        float pct = currenMoveTime / timeToMove;

        playerCamera.transform.position = Vector3.Lerp(originalCameraPos, gameCameraPosition.position, pct);
        playerCamera.transform.rotation = Quaternion.Lerp(originalCameraRot, gameCameraPosition.rotation, pct);

        if (pct >= 1.0f) 
        {
            cameraOnPositon = true;
        }
    }
}
