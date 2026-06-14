using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    [Header("Configuración de Control")]
    [SerializeField] private float centerDeadZonePixels = 35f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    private RadarMinigame radarMinigame;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
        radarMinigame = UnityEngine.Object.FindAnyObjectByType<RadarMinigame>();
    }

    public bool IsInteractable()
    {
        if (radarMinigame == null || !radarMinigame.IsEventActive()) return false;

        // SÓLO se bloquea físicamente si este volante de giro específico está haciendo su Lerp automático
        if (radarMinigame.IsRotationResetting()) return false;

        if (type == SliderType.Rotation)
        {
            return SliderCont.IsAtMinPos;
        }

        return true;
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!IsInteractable())
        {
            isDragging = false;
            return;
        }

        if (type == SliderType.Rotation && !SliderCont.IsAtMinPos)
        {
            isDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
                lastMouseAngle = GetMouseAngle();
            }
        }

        if (isDragging)
        {
            HandleDrag();
        }
    }

    void HandleDrag()
    {
        Vector3 screenCenter = Camera.main.WorldToScreenPoint(transform.position);
        float distanceToCenter = Vector2.Distance(Input.mousePosition, (Vector2)screenCenter);

        if (distanceToCenter < centerDeadZonePixels) return;

        float currentMouseAngle = GetMouseAngle();
        float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);

        lastMouseAngle = currentMouseAngle;

        if (!Mathf.Approximately(deltaAngle, 0f))
        {
            if (Mathf.Abs(deltaAngle) < 120f)
            {
                currentAngle += deltaAngle;
                ApplyRotation();
                SendValue(deltaAngle);
            }
            else
            {
                lastMouseAngle = currentMouseAngle;
            }
        }
    }

    public void ForceRotationReset(float targetX)
    {
        currentAngle = targetX;
        ApplyRotation();
    }

    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(currentAngle, initialRotation.y, initialRotation.z);
    }

    void SendValue(float d)
    {
        OnRotatedMoved?.Invoke(d, type);
    }
}