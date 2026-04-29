using UnityEngine;

public class ActionReader : MonoBehaviour, IInteractable
{
    public Dec_Act act;

    public float timeToMove = 1.0f;
    private float currentMoveTime = 0.0f;

    private Transform readTransform;
    private bool interacting;
    private bool returning;
    private bool onInteractedPosition = false;

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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameManager.Instance.playerController.LockPlayer(false);

                startPos = transform.position;
                startRot = transform.rotation;

                currentMoveTime = 0f;

                returning = true;
                interacting = false;
                onInteractedPosition = false;
                act.Execute();
            }
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

            if (toPos == readTransform.position)
            {
                onInteractedPosition = true;
            }
        }
    }

    public void Interact()
    {
        GameManager.Instance.playerController.LockPlayer(true);
        startPos = transform.position;
        startRot = transform.rotation;

        currentMoveTime = 0f;

        interacting = true;
        returning = false;
    }
}