using UnityEngine;

public class Action_Eat : BaseAction
{
    [Header("Referencias")]
    private Transform mouthTarget;
    public float moveSpeed = 8f;
    public float rotateSpeed = 10f;

    private Transform obj;
    private Vector3 originalPos;
    private Quaternion originalRot;

    private bool goingToMouth, waitingInput, returning, isActive;

    private void Start()
    {
        mouthTarget = GameManager.Instance.playerController.GetComponent<Interact>().mouthTarget;
    }

    public override void Interact()
    {
        if (isActive) return;

        obj = transform;
        originalPos = obj.position;
        originalRot = obj.rotation;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);
        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        goingToMouth = true;
        isActive = true;
    }

    protected override void OnUpdateAction()
    {
        if (!isActive) return;

        if (goingToMouth)
        {
            obj.position = Vector3.Lerp(obj.position, mouthTarget.position, moveSpeed * Time.deltaTime);
            obj.rotation = Quaternion.Slerp(obj.rotation, mouthTarget.rotation, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(obj.position, mouthTarget.position) < 0.01f)
            {
                obj.position = mouthTarget.position;
                obj.rotation = mouthTarget.rotation;
                goingToMouth = false;
                waitingInput = true;
            }
        }
        else if (waitingInput)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                waitingInput = false;
                returning = true;
            }
        }
        else if (returning)
        {
            obj.position = Vector3.Lerp(obj.position, originalPos, moveSpeed * Time.deltaTime);
            obj.rotation = Quaternion.Slerp(obj.rotation, originalRot, rotateSpeed * Time.deltaTime);

            if (Vector3.Distance(obj.position, originalPos) < 0.01f) FinishAction();
        }
    }

    void FinishAction()
    {
        isActive = false;
        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);
        GameManager.Instance.currentState = GameManager.GameState.Gameplay;
        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.PresiSaysYouCanRead);

        OnActionExecuted();
    }
}