using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class Cin_SubtitleTrackMixer : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        TextMeshProUGUI text = playerData as TextMeshProUGUI;
        string currentText = "";
        float currentAlpha = 0;

        if (!text) return;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);

            if (inputWeight > 0f) 
            {
                ScriptPlayable<Cin_SubtitleBehaviour> inputPlayable = (ScriptPlayable<Cin_SubtitleBehaviour>)playable.GetInput(i);
                Cin_SubtitleBehaviour input = inputPlayable.GetBehaviour();
                currentText = input.subtitleText;
                currentAlpha = inputWeight;
            }
        }

        text.text = currentText;
        text.color = new Color(1,1,1, currentAlpha);
    }
}
