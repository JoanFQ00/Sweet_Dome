using Mono.Cecil;
using UnityEngine;

public class Action_WatchTV : MonoBehaviour, IInteractable
{
    public Dec_Act watchTV;

    public Transform cameraTarget;
    public float moveSpeed = 2f;

    private Transform cam;

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

        cam.position = originalPos;
        cam.rotation = originalRot;

        watching = false;

        watchTV.Execute();
    }

    bool CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();

        if (hour < watchTV.minIntHour
            || hour > watchTV.maxIntHour)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            return false;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            return true;
        }
    }
}