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

    [Header("Speed")]
    public float dragSpeed = 3f;

    [Header("Values")]
    public float minValue = 0f;
    public float maxValue = 5f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = NormalizeAngle(transform.localEulerAngles.z);

        ApplyRotation();
        UpdateWaveValue();
    }

    void Update()
    {
        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentAngle -= mouseX * dragSpeed;
            currentAngle = Mathf.Clamp(currentAngle,minAngle,maxAngle);

            ApplyRotation();
            UpdateWaveValue();
        }
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(initialRotation.x,initialRotation.y,currentAngle);
    }

    void UpdateWaveValue()
    {
        float t = Mathf.InverseLerp(minAngle,maxAngle,currentAngle);
        float value = Mathf.Lerp(minValue,maxValue,t);

        switch (dialType)
        {
            case DialType.Amplitude:
                waveRenderer.amplitude = value;
                break;

            case DialType.Frequency:
                waveRenderer.frequency = value;
                break;

            case DialType.Phase:
                waveRenderer.phase = value;
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
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}