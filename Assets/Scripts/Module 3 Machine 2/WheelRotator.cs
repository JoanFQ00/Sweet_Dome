using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    [Header("Control Config")]
    [SerializeField] private float centerDeadZonePixels = 35f;

    [Header("Targe Light Renderer")]
    [SerializeField] private Renderer targetLightRenderer;

    [Space(5)]
    [ColorUsage(true, true)][SerializeField] private Color activeColor = Color.white;
    [ColorUsage(true, true)][SerializeField] private Color disabledColor = Color.black;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    private RadarMinigame radarMinigame;

    private Material runtimeMaterial;
    private bool lastInteractableState = false;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
        radarMinigame = UnityEngine.Object.FindAnyObjectByType<RadarMinigame>();

        Renderer rend = targetLightRenderer != null ? targetLightRenderer : (GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>());
        if (rend != null)
        {
            runtimeMaterial = rend.material;
        }

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