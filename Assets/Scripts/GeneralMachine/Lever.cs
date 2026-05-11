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

        if (Mathf.Abs(angle - suckAngle) <= threshold)
        {
            if (currentState != LeverState.Suck)
            {
                currentState = LeverState.Suck;
                tubeController.SetSuck();
            }
        }
        else if (Mathf.Abs(angle - neutralAngle) <= threshold)
        {
            if (currentState != LeverState.Neutral)
            {
                currentState = LeverState.Neutral;
                tubeController.SetNeutral();
            }
        }
        else if (Mathf.Abs(angle - outAngle) <= threshold)
        {
            if (currentState != LeverState.Out)
            {
                currentState = LeverState.Out;
                tubeController.SetOut();
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