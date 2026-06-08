using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;

    private float dragStartMouseAngle;
    private float dragStartWheelAngle;

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

                dragStartMouseAngle = GetMouseAngle();
                dragStartWheelAngle = currentAngle;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging)
            return;

        float mouseAngle = GetMouseAngle();
        float delta = Mathf.DeltaAngle(dragStartMouseAngle,mouseAngle);
        float previousAngle = currentAngle;
        currentAngle = dragStartWheelAngle + delta;

        ApplyRotation();
        SendValue(currentAngle - previousAngle);
    }

    float GetMouseAngle()
    {
        Vector3 screenPos =  Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;
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