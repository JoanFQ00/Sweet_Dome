using UnityEngine;

public class DialRotation : MonoBehaviour
{
    public enum DialType { Amplitude, Frequency, Phase }

    [Header("Dial Type")]
    public DialType dialType;

    [Header("Wave Target")]
    public WaveRenderer waveRenderer;

    [Header("Limits")]
    public float minAngle = -135f;
    public float maxAngle = 135f;

    [Header("Values")]
    public float minValue = 0f;
    public float maxValue = 5f;

    [Header("Seguridad de la Onda")]
    [SerializeField] private float safeMinimumValue = 0.8f;

    [Header("Tolerancia de Control")]
    [SerializeField] private float centerDeadZonePixels = 25f;

    [Header("Retorno Automático")]
    [SerializeField] private float resetLerpSpeed = 5f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    // Flag interno para controlar la animación del Lerp automático
    private bool isResetting = false;
    private float targetResetAngle;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        // El punto de inicio de fábrica de las ruedecitas será el centro exacto entre sus límites
        targetResetAngle = (minAngle + maxAngle) / 2f;

        currentAngle = NormalizeAngle(transform.localEulerAngles.z);

        ApplyRotation();
        UpdateWaveValue();
    }

    private void OnEnable()
    {
        // Nos suscribimos a la orden de reseteo del mánager de ondas
        WaveWinner.OnHardwareResetRequested += StartHardwareReset;
    }

    private void OnDisable()
    {
        WaveWinner.OnHardwareResetRequested -= StartHardwareReset;
    }

    void Update()
    {
        // MÁQUINA DE ESTADOS: Si la consola se está reseteando sola, ejecuta el Lerp rotatorio
        if (isResetting)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetResetAngle, resetLerpSpeed * 50f * Time.deltaTime);
            ApplyRotation();
            UpdateWaveValue();

            if (Mathf.Approximately(currentAngle, targetResetAngle))
            {
                currentAngle = targetResetAngle;
                isResetting = false; // Liberamos los controles
            }
            return; // Bloqueo: el ratón se ignora por completo mientras gira solo
        }

        if (!isDragging)
            return;

        Vector3 screenCenter = Camera.main.WorldToScreenPoint(transform.position);
        float distanceToCenter = Vector2.Distance(Input.mousePosition, (Vector2)screenCenter);

        if (distanceToCenter < centerDeadZonePixels)
            return;

        float currentMouseAngle = GetMouseAngle();
        float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);

        if (!Mathf.Approximately(deltaAngle, 0f))
        {
            if (Mathf.Abs(deltaAngle) < 120f)
            {
                currentAngle -= deltaAngle;
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

                ApplyRotation();
                UpdateWaveValue();
            }
        }

        lastMouseAngle = currentMouseAngle;
    }

    private void StartHardwareReset()
    {
        isDragging = false;
        isResetting = true; // Activa el modo Lerp automático en el Update
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, currentAngle);
    }

    void UpdateWaveValue()
    {
        if (waveRenderer == null) return;

        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        switch (dialType)
        {
            case DialType.Amplitude:
                waveRenderer.amplitude = Mathf.Lerp(safeMinimumValue, maxValue, t);
                break;

            case DialType.Frequency:
                waveRenderer.frequency = Mathf.Lerp(safeMinimumValue, maxValue, t);
                break;

            case DialType.Phase:
                waveRenderer.phase = Mathf.Lerp(minValue, maxValue, t);
                break;
        }
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    void OnMouseDown()
    {
        // Candado: No puedes interactuar con el dial si el minijuego no está activo o se está reseteando
        if (isResetting || !waveRenderer.gameObject.activeInHierarchy) return;

        isDragging = true;
        lastMouseAngle = GetMouseAngle();
    }

    void OnMouseUp() => isDragging = false;

    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}