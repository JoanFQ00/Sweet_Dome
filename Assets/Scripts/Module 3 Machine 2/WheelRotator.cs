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

    /// <summary>
    /// COMPUERTA ULTRA-ESTRICTA: Controla el feedback visual y físico del dial.
    /// </summary>
    public bool IsInteractable()
    {
        if (radarMinigame == null || !radarMinigame.IsEventActive()) return false;
        if (radarMinigame.IsResettingHardware()) return false; // Bloqueo si está auto-reseteándose

        // NUEVA REGLA: Si este dial es el de Rotación, SOLO es interactuable si el Slider de Posición está en el inicio
        if (type == SliderType.Rotation)
        {
            return SliderCont.IsAtMinPos;
        }

        // Si es el de Posición, es interactuable libremente mientras dure el evento
        return true;
    }

    void Update()
    {
        if (!IsInteractable())
        {
            isDragging = false;
            return;
        }

        // Si es el de rotación, la regla física del proyecto exige estar en MinPos
        if (type == SliderType.Rotation && !SliderCont.IsAtMinPos)
        {
            isDragging = false;
            return;
        }

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
                lastMouseAngle = GetMouseAngle();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging)
            return;

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