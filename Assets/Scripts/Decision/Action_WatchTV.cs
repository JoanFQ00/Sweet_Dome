using UnityEngine;

public class Action_WatchTV : BaseAction
{
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

    protected override void OnUpdateAction()
    {
        if (!watching) return;

        cam.position = Vector3.Lerp(cam.position, cameraTarget.position, Time.deltaTime * moveSpeed);
        cam.rotation = Quaternion.Lerp(cam.rotation, cameraTarget.rotation, Time.deltaTime * moveSpeed);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopWatching();
        }
    }

    public override void Interact()
    {
        if (watching) return;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        originalPos = cam.position;
        originalRot = cam.rotation;

        watching = true;
    }

    void StopWatching()
    {
        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        cam.position = originalPos;
        cam.rotation = originalRot;

        watching = false;

        OnActionExecuted();
    }
}