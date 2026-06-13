using UnityEngine;

public class DialRotation : MonoBehaviour
{
    public enum DialType
    {
        Amplitude,
        Frequency,
        Phase
    }

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
    [Tooltip("Valor mínimo absoluto que se le puede asignar a la Amplitud o Frecuencia para evitar que la onda se aplane por completo en una línea recta.")]
    [SerializeField] private float safeMinimumValue = 0.2f;

    [Header("Tolerancia de Control")]
    [Tooltip("Distancia mínima en píxeles al centro del dial. Si el ratón pasa muy cerca del centro, se ignora el frame para evitar giros bruscos.")]
    [SerializeField] private float centerDeadZonePixels = 25f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = NormalizeAngle(transform.localEulerAngles.z);

        ApplyRotation();
        UpdateWaveValue();
    }

    void Update()
    {
        if (!isDragging)
            return;

        // 1. Calculamos la distancia actual del cursor al centro del dial en la pantalla
        Vector3 screenCenter = Camera.main.WorldToScreenPoint(transform.position);
        float distanceToCenter = Vector2.Distance(Input.mousePosition, (Vector2)screenCenter);

        // 2. Si el jugador pasa el ratón por todo el medio cruzando de lado a lado, congelamos el cálculo
        if (distanceToCenter < centerDeadZonePixels)
            return;

        // 3. Calculamos el ángulo actual y su variación respecto al frame anterior
        float currentMouseAngle = GetMouseAngle();
        float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);

        if (!Mathf.Approximately(deltaAngle, 0f))
        {
            // 4. Amortiguamos cambios de ángulo absurdamente gigantescos por cruce repentino de ejes
            if (Mathf.Abs(deltaAngle) < 120f)
            {
                currentAngle -= deltaAngle;

                // 5. Clampeamos con tus límites de seguridad de rotación física
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

                ApplyRotation();
                UpdateWaveValue();
            }
        }

        // Mantenemos la actualización constante para la fluidez milimétrica al arrastrar
        lastMouseAngle = currentMouseAngle;
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, currentAngle);
    }

    void UpdateWaveValue()
    {
        if (waveRenderer == null) return;

        // Normalizamos la posición actual del dial entre 0 y 1 (0 es minAngle, 1 es maxAngle)
        float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);

        switch (dialType)
        {
            case DialType.Amplitude:
                // MODIFICACIÓN: El cambio gradual ahora empieza en 'safeMinimumValue' en lugar de 'minValue'
                // para que use todo el rango del dial sin estancarse ni llegar a 0.
                waveRenderer.amplitude = Mathf.Lerp(safeMinimumValue, maxValue, t);
                break;

            case DialType.Frequency:
                // MODIFICACIÓN: Lo mismo para la frecuencia.
                waveRenderer.frequency = Mathf.Lerp(safeMinimumValue, maxValue, t);
                break;

            case DialType.Phase:
                // La fase sí puede usar el 'minValue' original (por ejemplo 0) ya que no aplana la onda
                float phaseValue = Mathf.Lerp(minValue, maxValue, t);
                waveRenderer.phase = phaseValue;
                break;
        }
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }

    void OnMouseDown()
    {
        isDragging = true;
        lastMouseAngle = GetMouseAngle();
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
}