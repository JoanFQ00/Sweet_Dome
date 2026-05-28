using UnityEngine;

public class Action_Eat : BaseAction
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float rotateSpeed = 10f;

    [Header("Eat Offset")]
    public Vector3 localOffset = new Vector3(0.25f, -0.2f, 0.5f);
    public Vector3 localRotationOffset = new Vector3(25f, -35f, 0f);

    private Transform cam;
    private Transform obj;

    private Vector3 originalPos;
    private Quaternion originalRot;

    private Vector3 targetPos;
    private Quaternion targetRot;

    private bool goingToMouth;
    private bool waitingInput;
    private bool returning;

    private bool isActive;

    public override void Interact()
    {
        if (isActive) return;

        cam = Camera.main.transform;
        obj = transform;

        originalPos = obj.position;
        originalRot = obj.rotation;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        GameManager.Instance.currentState =
            GameManager.GameState.Interacting;

        targetPos =
            cam.position +
            cam.right * localOffset.x +
            cam.up * localOffset.y +
            cam.forward * localOffset.z;

        targetRot =
            cam.rotation *
            Quaternion.Euler(localRotationOffset);

        goingToMouth = true;
        isActive = true;
    }

    protected override void OnUpdateAction()
    {
        if (!isActive) return;

        if (goingToMouth)
        {
            MoveToMouth();
            return;
        }

        if (waitingInput)
        {
            WaitForSecondInput();
            return;
        }

        if (returning)
        {
            ReturnObject();
        }
    }

    void MoveToMouth()
    {
        obj.position = Vector3.Lerp(obj.position, targetPos, moveSpeed * Time.deltaTime);
        obj.rotation = Quaternion.Slerp(obj.rotation, targetRot, rotateSpeed * Time.deltaTime);

        if (Vector3.Distance(obj.position, targetPos) < 0.03f)
        {
            obj.position = targetPos;
            obj.rotation = targetRot;

            goingToMouth = false;
            waitingInput = true;
        }
    }

    void WaitForSecondInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            waitingInput = false;
            returning = true;
        }
    }

    void ReturnObject()
    {
        obj.position = Vector3.Lerp(obj.position, originalPos, moveSpeed * Time.deltaTime);
        obj.rotation = Quaternion.Slerp(obj.rotation, originalRot, rotateSpeed * Time.deltaTime);

        if (Vector3.Distance(obj.position, originalPos) < 0.03f)
        {
            obj.position = originalPos;
            obj.rotation = originalRot;

            FinishAction();
        }
    }
    void FinishAction()
    {
        if (!isActive) return;

        isActive = false;

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        GameManager.Instance.currentState = GameManager.GameState.Gameplay;

        GameManager.Instance.playerController
            .GetComponent<Interact>()
            .ChangeInteracting(false);

        OnActionExecuted();
    }
}