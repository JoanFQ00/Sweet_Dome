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

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;

    private float dragStartMouseAngle;
    private float dragStartDialAngle;

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

        float mouseAngle = GetMouseAngle();
        float delta = Mathf.DeltaAngle(dragStartMouseAngle,mouseAngle);
        currentAngle = dragStartDialAngle - delta;

        currentAngle = Mathf.Clamp(currentAngle,minAngle,maxAngle);

        ApplyRotation();
        UpdateWaveValue();
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

        dragStartMouseAngle = GetMouseAngle();
        dragStartDialAngle = currentAngle;
    }
    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;
        return Mathf.Atan2(direction.y,direction.x) * Mathf.Rad2Deg;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}