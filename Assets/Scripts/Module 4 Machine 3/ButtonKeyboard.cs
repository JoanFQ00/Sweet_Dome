using UnityEngine;

public class ButtonKeyboard : ButtonValidator
{
    [Header("Ref Control")]
    [SerializeField] private WaveWinner waveWinner;
    [SerializeField] private CodeSystem codeSystem;

    void Start()
    {
        if (waveWinner == null)
            waveWinner = Object.FindAnyObjectByType<WaveWinner>();

        if (codeSystem == null)
            codeSystem = Object.FindAnyObjectByType<CodeSystem>();
    }

    public override bool CanPress()
    {
        if (codeSystem == null) return false;

        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day5)
        {
            return codeSystem.IsCodeSystemActive();
        }

        // --- COMPORTAMIENTO HABITUAL (Días 1 a 4) ---
        if (waveWinner == null) return false;

        return waveWinner.waveSolved && codeSystem.IsCodeSystemActive();
    }
}