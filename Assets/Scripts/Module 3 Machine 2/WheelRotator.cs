using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    [Header("Limits")]
    public float minAngle = -135f;
    public float maxAngle = 135f;

    [Header("Speed")]
    public float dragSpeed = 3.0f;

    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    private bool isDragging;
    private float currentAngle;
    private float lastSentValue;

    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;

        currentAngle = NormalizeAngle(transform.localEulerAngles.x);
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        ApplyRotation();
        SendValue();
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        float mouseX = Input.GetAxis("Mouse X");

        currentAngle -= mouseX * dragSpeed;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        ApplyRotation();
        SendValue();
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            currentAngle,
            initialRotation.y,
            initialRotation.z
        );
    }

    void SendValue()
    {
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        // Evita spam innecesario si quieres (opcional)
        if (Mathf.Approximately(t, lastSentValue)) return;

        lastSentValue = t;
        OnRotatedMoved?.Invoke(t, type);
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}