using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class Cin_SubtitleBehaviour : PlayableBehaviour
{
    public string subtitleText;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        TextMeshProUGUI text = playerData as TextMeshProUGUI;
        text.text = subtitleText;
        text.color = new Color(1, 1, 1, info.weight);
    }
}
