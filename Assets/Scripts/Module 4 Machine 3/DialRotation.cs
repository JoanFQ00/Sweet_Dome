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

    [Header("Wave Safety")]
    [SerializeField] private float safeMinimumValue = 0.8f;

    [Header("Tolerance Control")]
    [SerializeField] private float centerDeadZonePixels = 25f;

    [Header("Automatic Return")]
    [SerializeField] private float resetLerpSpeed = 5f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    private bool isResetting = false;
    private float targetResetAngle;

    private WaveWinner waveWinner;
    private HL_Interact hlInteract;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        targetResetAngle = (minAngle + maxAngle) / 2f;
        currentAngle = NormalizeAngle(transform.localEulerAngles.z);

        waveWinner = Object.FindAnyObjectByType<WaveWinner>();
        hlInteract = GetComponent<HL_Interact>();

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

    public bool IsInteractable()
    {
        if (waveWinner != null && waveWinner.waveSolved) return false;
        if (isResetting) return false;
        if (waveRenderer != null && !waveRenderer.gameObject.activeInHierarchy) return false;

        return true;
    }

    void Update()
    {
        if (!IsInteractable())
        {
            isDragging = false;

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
        if (!IsInteractable()) return;

        isDragging = true;
        lastMouseAngle = GetMouseAngle();
        SoundManagerOwn.Instance.PlaySFX(SoundManagerOwn.Instance.rotateWheelSFX, true);
    }

    void OnMouseUp()
    {
        StopDialSound();
    }

    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void StopDialSound()
    {
        isDragging = false;
        SoundManagerOwn.Instance.StopSFX();
    }
}