using UnityEngine;

public class Lever : MonoBehaviour
{
    public TubeController tubeController;

    [Header("Lever Angles")]
    public float suckAngle = -22f;
    public float neutralAngle = -70f;
    public float outAngle = -129f;

    [Header("Drag Settings")]
    public float dragSpeed = 2f;

    private bool isDragging;

    public float currentAngle;

    private Vector3 initialRotation;

    private enum LeverState
    {
        Suck,
        Neutral,
        Out
    }

    private LeverState currentState;

    private void Start()
    {
        initialRotation = transform.localEulerAngles;

        currentAngle = NormalizeAngle(initialRotation.x);
    }

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

        currentAngle += mouseY * dragSpeed;

        currentAngle = Mathf.Clamp(currentAngle, outAngle, suckAngle);

        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            currentAngle,
            initialRotation.y,
            initialRotation.z
        );
    }

    void CheckLeverState()
    {
        float suckDist = Mathf.Abs(currentAngle - suckAngle);
        float neutralDist = Mathf.Abs(currentAngle - neutralAngle);
        float outDist = Mathf.Abs(currentAngle - outAngle);

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