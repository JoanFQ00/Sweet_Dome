using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    [Header("Speed")]
    public float dragSpeed = 0.5f; 

    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
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
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        float mouseX = Input.GetAxis("Mouse X");
        float deltaRotation = mouseX * dragSpeed;

        currentAngle -= deltaRotation;

        ApplyRotation();
        SendValue(deltaRotation);
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            currentAngle,
            initialRotation.y,
            initialRotation.z
        );
    }

    void SendValue(float d)
    {
        OnRotatedMoved?.Invoke(d, type);
    }
}