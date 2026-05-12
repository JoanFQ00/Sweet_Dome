using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WaveWinner : MonoBehaviour
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;

    [Header("UI Text")]
    public GameObject winText;

    [Header("Tolerancies")]
    public float amplitudeTolerance = 1.0f;
    public float frequencyTolerance = 1.0f;
    public float phaseTolerance = 1.0f;

    [Header("Time Message")]
    public float messageDuration = 3f;

    private bool hasWon = false;
    private float timer = 0f;
    private bool showingMessage = false;

    void Start()
    {
        if(winText != null)
            winText.SetActive(false);
    }

    void Update()
    {
        if (!hasWon)
        {
            if (IsMatching())
            {
                hasWon = true;
                ShowWin();
            }
        }

        if (showingMessage)
        {
            timer += Time.deltaTime;

            if (timer >= messageDuration)
            {
                HideWinMessage();
            }
        }
    }

    bool IsMatching()
    {
        if (Mathf.Abs(playerRenderer.amplitude - targetRenderer.amplitude) < amplitudeTolerance && 
            Mathf.Abs(playerRenderer.frequency - targetRenderer.frequency) < frequencyTolerance && 
            Mathf.Abs(playerRenderer.phase - targetRenderer.phase) < phaseTolerance)
        {
            return true;
        }

        return false;
    }

    void ShowWin()
    {
        if (winText != null)
            winText.SetActive(false);

        if (codeSystem != null)
        {
            codeSystem.GenerateCode();
        }
    }

    void HideWinMessage()
    {
        if (winText != null)
            winText.SetActive(false);

        showingMessage = false;
    }
}