using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackBindingType(typeof(TextMeshProUGUI))]
[TrackClipType(typeof(Cin_SubtitleClip))]
public class Cin_SubtitleTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<Cin_SubtitleTrackMixer>.Create(graph, inputCount);
    }
    
}
