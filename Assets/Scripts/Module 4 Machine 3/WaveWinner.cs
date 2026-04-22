using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WaveWinner : MonoBehaviour
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;

    [Header("UI Text")]
    public GameObject winText;

    [Header("Tolerancies")]
    public float amplitudeTolerance = 1.0f;
    public float frequencyTolerance = 1.0f;
    public float phaseTolerance = 1.0f;

    private bool hasWon;

    void Start()
    {
        if(winText != null)
            winText.SetActive(false);
    }

    void Update()
    {
        if (hasWon) return;

        if (IsMatching())
        {
            hasWon = true;
            ShowWin();
        }
    }

    bool IsMatching()
    {
        if (Mathf.Abs(playerRenderer.amplitude - targetRenderer.amplitude) < amplitudeTolerance && 
            Mathf.Abs(playerRenderer.frequency - targetRenderer.frequency) < frequencyTolerance && 
            Mathf.Abs(targetRenderer.phase - targetRenderer.phase) < phaseTolerance)
        {
            return true;
        }

        return false;
    }

    void ShowWin()
    {
        if (winText != null)
            winText.SetActive(true);
    }
}
