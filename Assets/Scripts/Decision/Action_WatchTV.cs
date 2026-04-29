using Mono.Cecil;
using UnityEngine;

public class Action_WatchTV : MonoBehaviour, IInteractable
{
    public Dec_Act watchTV;
    private bool actionCompleted = false;

    public Transform cameraTarget;
    public float moveSpeed = 2f;

    private Transform cam;
    private int currentLayer = -1;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private bool watching = false;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        CheckHour();
        if (!watching) return;

        MoveCamera();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopWatching();
        }
    }

    public void Interact()
    {
        if (watching) return;

        StartWatching();
    }

    void StartWatching()
    {
        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);
        gameObject.layer = LayerMask.NameToLayer("Default");

        originalPos = cam.position;
        originalRot = cam.rotation;

        watching = true;
    }

    void MoveCamera()
    {
        cam.position = Vector3.Lerp(cam.position, cameraTarget.position, Time.deltaTime * moveSpeed);
        cam.rotation = Quaternion.Lerp(cam.rotation, cameraTarget.rotation, Time.deltaTime * moveSpeed);
    }

    void StopWatching()
    {
        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        cam.position = originalPos;
        cam.rotation = originalRot;

        watching = false;

        watchTV.ActionExecuted();
    }

    void CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (hour < watchTV.minIntHour || hour >= watchTV.maxIntHour)
            newLayer = LayerMask.NameToLayer("Default");
        else
            newLayer = LayerMask.NameToLayer("Interactable");

        if (hour >= watchTV.maxIntHour && actionCompleted == false)
        {
            watchTV.ActionFailed();
        }

        if (newLayer != currentLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }
}