using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Tube Controller")]
    public TubeController tubeController;

    [Header("Lever Angles")]
    public float suckAngle = -22f;
    public float neutralAngle = -70f;
    public float outAngle = -129f;

    [Header("Drag Settings")]
    public float dragSpeed = 2f;
    public float threshold = 2f;
    private bool isDragging;

    private enum LeverState
    {
        Suck,
        Neutral,
        Out
    }

    private LeverState currentState;

    private void Update()
    {
        if (isDragging)
        {
            DragLever();
        }
        CheckLeverState();
    }

    void DragLever()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        Vector3 rot = transform.localEulerAngles;
        float currentX = NormalizeAngle(rot.x);
        currentX += mouseY * dragSpeed;
        currentX = Mathf.Clamp(currentX, outAngle, suckAngle);
        transform.localEulerAngles =new Vector3(currentX,rot.y,rot.z);
    }

    void CheckLeverState()
    {
        float angle = NormalizeAngle(transform.localEulerAngles.x);

        float suckDist = Mathf.Abs(angle - suckAngle);
        float neutralDist = Mathf.Abs(angle - neutralAngle);
        float outDist = Mathf.Abs(angle - outAngle);

        LeverState newState;

        if (suckDist < neutralDist && suckDist < outDist)
        {
            newState = LeverState.Suck;
        }
        else if (neutralDist < outDist)
        {
            newState = LeverState.Neutral;
        }
        else
        {
            newState = LeverState.Out;
        }

        if (newState != currentState)
        {
            currentState = newState;

            switch (currentState)
            {
                case LeverState.Suck:
                    tubeController.SetSuck();
                    break;

                case LeverState.Neutral:
                    tubeController.SetNeutral();
                    break;

                case LeverState.Out:
                    tubeController.SetOut();
                    break;
            }
        }
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;

        return angle;
    }

    private void OnMouseDown()
    {
        isDragging = true;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }
}