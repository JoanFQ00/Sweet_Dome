using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WaveRenderer : MonoBehaviour
{
    public int resolutionRenderer = 100;
    public float length = 5f;

    [Header("Wave Settings")]
    public float amplitude = 1f;
    public float frequency = 1f;
    public float phase = 0f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolutionRenderer;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            if (lineRenderer.positionCount != 0)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        if (amplitude <= 0f || frequency <= 0f)
        {
            if (lineRenderer.positionCount != 0)
            {
                lineRenderer.positionCount = 0;
            }
            return;
        }

        if (lineRenderer.positionCount != resolutionRenderer)
        {
            lineRenderer.positionCount = resolutionRenderer;
        }

        DrawWave();
    }

    void DrawWave()
    {
        for (int i = 0; i < resolutionRenderer; i++)
        {
            float x = (i / (float)(resolutionRenderer - 1)) * length - length / 2;

            float y = amplitude * Mathf.Sin(frequency * x + phase);

            lineRenderer.SetPosition(i, new Vector3(x, 0, y));
        }
    }
}