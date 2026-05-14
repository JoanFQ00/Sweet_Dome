using UnityEngine;

public class Lever : MonoBehaviour
{
    public Machine01 machine;

    [Header("Lever Angles")]
    public float suckAngle = -22f;
    public float neutralAngle = -70f;
    public float outAngle = -129f;

    [Header("Drag Settings")]
    public float dragSpeed = 2f;

    [Header("Return Settings")]
    public float returnSpeed = 10f;

    private bool isDragging;

    public float currentAngle;

    private Vector3 initialRotation;

    private enum LeverState
    {
        Absorb,
        Neutral,
        Expelling
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
        else
        {
            ReturnToNeutral();
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

    void ReturnToNeutral()
    {
        currentAngle = Mathf.Lerp(
            currentAngle,
            neutralAngle,
            Time.deltaTime * returnSpeed
        );

        // Evita vibraciones infinitas cerca del objetivo
        if (Mathf.Abs(currentAngle - neutralAngle) < 0.01f)
        {
            currentAngle = neutralAngle;
        }

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
            newState = LeverState.Absorb;
        }
        else if (neutralDist < outDist)
        {
            newState = LeverState.Neutral;
        }
        else
        {
            newState = LeverState.Expelling;
        }

        switch (newState)
        {
            case LeverState.Absorb:
                machine.GetActivePump().Absorbing();
                break;

            case LeverState.Expelling:
                machine.GetActivePump().Expelling();
                break;

            default:
                // Neutral
                break;
        }

        if (newState != currentState)
            currentState = newState;
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