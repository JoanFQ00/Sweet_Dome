using UnityEngine;
using System;

public class SliderCont : MonoBehaviour, IResetable
{
    public static Action<float, SliderType> OnSliderMoved;

    [Header("Configuración del Slider")]
    [SerializeField] private SliderType sliderType = SliderType.Position;
    [SerializeField] private Transform startPoint; // MinPos
    [SerializeField] private Transform endPoint;   // MaxPos
    [SerializeField] private float centerDeadZonePixels = 25f;

    private bool isDragging;
    private float currentNormalizedValue = 0f;
    private RadarMinigame radarMinigame;

    // Propiedad estática de control para el volante de rotación
    public static bool IsAtMinPos { get; private set; } = true;

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
        if (radarMinigame == null) return false;
        // No interactuable si no hay evento o si el hardware se está auto-reseteando/limpiando
        return radarMinigame.IsEventActive() && !radarMinigame.IsResettingHardware();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // BUGFIX: Aunque el jugador no esté arrastrando, recalculamos de forma estricta el IsAtMinPos 
        // frame a frame en base a la distancia física real. Así el volante detecta el Lerp al instante.
        if (startPoint != null)
        {
            IsAtMinPos = Vector3.Distance(transform.position, startPoint.position) < 0.01f;
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
    }

    /// <summary>
    /// Permite al script del Radar inyectar la posición de retorno suave por Lerp.
    /// </summary>
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