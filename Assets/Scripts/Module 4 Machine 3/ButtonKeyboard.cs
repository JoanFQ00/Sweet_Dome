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
        if (waveWinner == null || codeSystem == null)
            return false;

        return waveWinner.waveSolved && codeSystem.IsCodeSystemActive();
    }
}
