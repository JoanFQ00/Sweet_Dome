using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class ActionReader : MonoBehaviour, IInteractable
{
    public Dec_Act act;
    private bool actionCompleted = false;

    public float timeToMove = 1.0f;
    private float currentMoveTime = 0.0f;

    private Transform readTransform;
    private bool interacting;
    private bool returning;
    private bool onInteractedPosition = false;
    private int currentLayer = -1;

    private Vector3 startPos;
    private Quaternion startRot;

    private Vector3 originalPos;
    private Quaternion originalRot;

    void Start()
    {
        readTransform = GameManager.Instance.playerController.
            gameObject.GetComponent<Interact>().readTransform;

        originalPos = transform.position;
        originalRot = transform.rotation;

        interacting = false;
        returning = false;
    }

    void Update()
    {
        CheckHour();

        if (interacting)
        {
            UpdateBookPos(startPos, startRot, readTransform.position, readTransform.rotation);
        }

        if (returning)
        {
            UpdateBookPos(startPos, startRot, originalPos, originalRot);
        }

        if (onInteractedPosition)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.playerController.LockPlayer(false);

                startPos = transform.position;
                startRot = transform.rotation;

                currentMoveTime = 0f;

                returning = true;
                interacting = false;
                onInteractedPosition = false;
                actionCompleted = true;
                DayTime.Instance.StopTimeCount(false);
                act.ActionExecuted();
            }
        }
    }

    void CheckHour() 
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (hour < act.minIntHour || hour >= act.maxIntHour)
            newLayer = LayerMask.NameToLayer("Default");
        else
            newLayer = LayerMask.NameToLayer("Interactable");

        if (hour >= act.maxIntHour && actionCompleted == false) 
        {
            act.ActionFailed();
        }

        if (newLayer != currentLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }

    void UpdateBookPos(Vector3 fromPos, Quaternion fromRot, Vector3 toPos, Quaternion toRot)
    {
        currentMoveTime += Time.deltaTime;

        float pct = Mathf.Clamp01(currentMoveTime / timeToMove);

        transform.position = Vector3.Lerp(fromPos, toPos, pct);
        transform.rotation = Quaternion.Lerp(fromRot, toRot, pct);

        if (pct >= 1.0f)
        {
            interacting = false;
            returning = false;

            if (Vector3.Distance(toPos, readTransform.position) < 0.001f)
            {
                onInteractedPosition = true;
            }
        }
    }

    public void Interact()
    {
        GameManager.Instance.playerController.LockPlayer(true);
        DayTime.Instance.StopTimeCount(true);
        startPos = transform.position;
        startRot = transform.rotation;

        currentMoveTime = 0f;

        interacting = true;
        returning = false;
    }
}