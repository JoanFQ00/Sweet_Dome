using UnityEngine;
using System;

public class SliderCont : MonoBehaviour, IResetable
{
    public static Action<float, SliderType> OnSliderMoved;

    [Header("Configuración del Slider")]
    [SerializeField] private SliderType sliderType = SliderType.Position;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float centerDeadZonePixels = 25f;

    private bool isDragging;
    private float currentNormalizedValue = 0f;
    private RadarMinigame radarMinigame;

    // Propiedad estática controlada de forma infalible
    public static bool IsAtMinPos { get; private set; } = true;

    public static void ForceMinPosState(bool state)
    {
        IsAtMinPos = state;
    }

    void Start()
    {
        radarMinigame = UnityEngine.Object.FindAnyObjectByType<RadarMinigame>();
        if (GameManager.Instance != null) GameManager.Instance.RegisterResetable(this);

        IsAtMinPos = true;
        currentNormalizedValue = 0f;

        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);
    }

    public bool IsInteractable()
    {
        if (radarMinigame == null || !radarMinigame.IsEventActive()) return false;
        return !radarMinigame.IsSliderResetting();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // MARGEN DE TOLERANCIA BLINDADO: Si el valor normalizado está en el 15% inferior,
        // liberamos el volante de rotación de forma automática sin importar milésimas flotantes.
        if (startPoint != null)
        {
            IsAtMinPos = currentNormalizedValue < 0.15f;
        }

        if (!IsInteractable())
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
            }
        }

        if (isDragging)
        {
            Calculate3DDrag();
        }
    }

    void Calculate3DDrag()
    {
        if (startPoint == null || endPoint == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane movementPlane = new Plane(Camera.main.transform.forward, startPoint.position);

        if (movementPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 mouseWorldPoint = ray.GetPoint(enterDistance);

            Vector3 carrilVector = endPoint.position - startPoint.position;
            Vector3 mouseDeltaVector = mouseWorldPoint - startPoint.position;

            Vector3 projectedPoint = Vector3.Project(mouseDeltaVector, carrilVector.normalized);
            float magnitudeMax = carrilVector.magnitude;
            float dotProduct = Vector3.Dot(projectedPoint, carrilVector.normalized);

            currentNormalizedValue = Mathf.Clamp01(dotProduct / magnitudeMax);

            ApplyVisualPosition();
            OnSliderMoved?.Invoke(currentNormalizedValue, sliderType);
        }
    }

    private void ApplyVisualPosition()
    {
        if (startPoint == null || endPoint == null) return;
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, currentNormalizedValue);
        IsAtMinPos = currentNormalizedValue < 0.15f; // Sincronización de tolerancia
    }

    public void ApplyInstantPosition(float normalizedValue)
    {
        currentNormalizedValue = Mathf.Clamp01(normalizedValue);
        ApplyVisualPosition();
    }

    public void ResetValues()
    {
        isDragging = false;
        IsAtMinPos = true;
        currentNormalizedValue = 0f;
        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }
    }
}