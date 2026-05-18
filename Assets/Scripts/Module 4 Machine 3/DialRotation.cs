using UnityEngine;
using UnityEngine.UIElements;

public class DialRotation : MonoBehaviour
{
    public enum DialType
    {
        Amplitude,
        Frequency,
        Phase
    }
    public DialType dialtype;

    [Header("Wave Target")]
    public WaveRenderer waveRenderer;

    [Header("Rotation")]
    public float minAngle = -135;
    public float maxAngle = 135;

    [Header("Steps")]
    public float stepAngle = 10f;

    [Header("Drag")]
    public float dragSpeed = 5f;

    [Header("Values")]
    public float minValue = 0f;
    public float maxValue = 5f;


    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = NormalizeAngle(initialRotation.z);
    }

    void Update()
    {
        if (isDragging)
            RotateDial();
    }

    void RotateDial()
    {
        UpdateWaveValue();
    }

    void UpdateWaveValue()
    {
        switch (dialtype)
        {
            case DialType.Amplitude:
                break;
            case DialType.Frequency:
                break;
            case DialType.Phase:
                break;
        }
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f)
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
