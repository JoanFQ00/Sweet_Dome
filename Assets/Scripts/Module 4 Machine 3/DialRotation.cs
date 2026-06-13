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

    private bool isResetting = false;
    private float targetResetAngle;

    // Referencia local automática para saber el estado del juego
    private WaveWinner waveWinner;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        targetResetAngle = (minAngle + maxAngle) / 2f;
        currentAngle = NormalizeAngle(transform.localEulerAngles.z);

        // Buscamos el componente WaveWinner de forma automática en la escena
        waveWinner = Object.FindAnyObjectByType<WaveWinner>();

        ApplyRotation();
        UpdateWaveValue();
    }

    private void OnEnable()
    {
        WaveWinner.OnHardwareResetRequested += StartHardwareReset;
        WaveWinner.OnDialsLockRequested += LockDialInteraction;
    }

    private void OnDisable()
    {
        WaveWinner.OnHardwareResetRequested -= StartHardwareReset;
        WaveWinner.OnDialsLockRequested -= LockDialInteraction;
    }

    void Update()
    {
        // REGLA NUEVA: Si el mánager ya ha solucionado la sintonización, congelamos las ruedas de inmediato
        if (waveWinner != null && waveWinner.waveSolved)
        {
            isDragging = false;
            return;
        }

        if (isResetting)
        {
            currentAngle = Mathf.MoveTowards(currentAngle, targetResetAngle, resetLerpSpeed * 50f * Time.deltaTime);
            ApplyRotation();
            UpdateWaveValue();

            if (Mathf.Approximately(currentAngle, targetResetAngle))
            {
                currentAngle = targetResetAngle;
                isResetting = false;
            }
            return;
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
        isResetting = true;
    }

    private void LockDialInteraction()
    {
        isDragging = false;
        isResetting = false;
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
        // REGLA NUEVA: Si se está reseteando, la pantalla está apagada o la onda YA está solucionada, no dejes arrastrar con el ratón
        if (isResetting || !waveRenderer.gameObject.activeInHierarchy || (waveWinner != null && waveWinner.waveSolved))
            return;

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