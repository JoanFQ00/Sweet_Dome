using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WaveWinner : MonoBehaviour
{
    [Header("Referencies")]
    public WaveRenderer playerRenderer;
    public WaveRenderer targetRenderer;
    public CodeSystem codeSystem;


    [Header("Tolerancies")]
    public float amplitudeTolerance = 1.0f;
    public float frequencyTolerance = 1.0f;
    public float phaseTolerance = 1.0f;

    [Header("Time Message")]
    public float messageDuration = 3f;

    private bool hasWon = false;
    private float timer = 0f;
    private bool showingMessage = false;
    public bool waveSolved = false;

    void Start()
    {
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
        float ampDiff = Mathf.Abs(playerRenderer.amplitude - targetRenderer.amplitude);
        float freqDiff = Mathf.Abs(playerRenderer.frequency - targetRenderer.frequency);
        float phaseDiff = PhaseDifference(playerRenderer.phase,targetRenderer.phase);

        return ampDiff < amplitudeTolerance && freqDiff < frequencyTolerance && phaseDiff < phaseTolerance;
    }

    float PhaseDifference(float a, float b)
    {
        float diff = Mathf.Abs(a - b);

        return Mathf.Min(diff, Mathf.PI * 2f - diff);
    }

    void ShowWin()
    {
        if (codeSystem != null)
        {
            codeSystem.GenerateCode();
        }
    }

    void HideWinMessage()
    {
        showingMessage = false;
    }
}