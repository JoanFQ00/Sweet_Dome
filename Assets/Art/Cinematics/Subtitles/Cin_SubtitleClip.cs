using UnityEngine;
using UnityEngine.Playables;

public class Cin_SubtitleClip : PlayableAsset
{
    public string subtitleText;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<Cin_SubtitleBehaviour>.Create(graph);

        Cin_SubtitleBehaviour subtitleBehaviour = playable.GetBehaviour();
        subtitleBehaviour.subtitleText = subtitleText;

        return playable;
    }
}
