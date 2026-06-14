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

    // Referencia automática para saber el estado del minijuego
    private RadarMinigame radarMinigame;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
        radarMinigame = UnityEngine.Object.FindAnyObjectByType<RadarMinigame>();
    }

    /// <summary>
    /// COMPUERTA CLAVE: Devuelve si la rueda es accionable en este frame del juego.
    /// </summary>
    public bool IsInteractable()
    {
        if (radarMinigame == null) return false;
        // Solo es interactuable si el evento de radar está corriendo activamente
        return radarMinigame.IsEventActive();
    }

    void Update()
    {
        if (!IsInteractable())
        {
            isDragging = false;
            return;
        }

        if (!SliderCont.IsAtMinPos)
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

        if (distanceToCenter < centerDeadZonePixels)
        {
            return;
        }

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