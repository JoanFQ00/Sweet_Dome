using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    [Header("Configuración de Control")]
    [SerializeField] private float centerDeadZonePixels = 35f;

    [Header("Configuración Estética de Colores (Malla Externa URP)")]
    [Tooltip("¡NUEVO! Arrastra aquí la OTRA mesh que debe brillar (indicador, aro, etc.). Si se deja vacío, se usará a sí mismo.")]
    [SerializeField] private Renderer targetLightRenderer;

    [Space(5)]
    [Tooltip("Color e Intensidad HDR cuando el volante está ACTIVO.")]
    [ColorUsage(true, true)][SerializeField] private Color activeColor = Color.white;
    [Tooltip("Color de la bombilla cuando el volante está bloqueado.")]
    [ColorUsage(true, true)][SerializeField] private Color disabledColor = Color.black;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    private RadarMinigame radarMinigame;

    // Almacenamiento en caché del material dinámico
    private Material runtimeMaterial;
    private bool lastInteractableState = false;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
        radarMinigame = UnityEngine.Object.FindAnyObjectByType<RadarMinigame>();

        // SOLUCIÓN: Si nos has dado una malla externa, pillamos su material. Si no, usamos el nuestro.
        Renderer rend = targetLightRenderer != null ? targetLightRenderer : (GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>());
        if (rend != null)
        {
            runtimeMaterial = rend.material;
        }

        // Forzamos el estado de iluminación inicial (apagado por defecto)
        lastInteractableState = IsInteractable();
        UpdateWheelEmission(lastInteractableState);
    }

    public bool IsInteractable()
    {
        if (radarMinigame == null || !radarMinigame.IsEventActive()) return false;

        if (radarMinigame.IsRotationResetting()) return false;

        if (type == SliderType.Rotation)
        {
            return SliderCont.IsAtMinPos;
        }

        return true;
    }

    void Update()
    {
        bool currentInteractable = IsInteractable();
        if (currentInteractable != lastInteractableState)
        {
            lastInteractableState = currentInteractable;
            UpdateWheelEmission(lastInteractableState);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!currentInteractable)
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

    private void UpdateWheelEmission(bool canUse)
    {
        if (runtimeMaterial == null) return;

        Color targetColor = canUse ? activeColor : disabledColor;

        runtimeMaterial.EnableKeyword("_EMISSION");
        if (runtimeMaterial.HasProperty("_EmissionColor"))
        {
            runtimeMaterial.SetColor("_EmissionColor", targetColor);
        }

        if (!canUse || targetColor == Color.black || targetColor.a == 0f)
        {
            runtimeMaterial.DisableKeyword("_EMISSION");
        }

        // Aplicamos los cambios de iluminación global sobre la malla objetivo (la externa)
        Renderer rend = targetLightRenderer != null ? targetLightRenderer : (GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>());
        if (rend != null)
        {
            DynamicGI.SetEmissive(rend, canUse ? targetColor : Color.black);
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