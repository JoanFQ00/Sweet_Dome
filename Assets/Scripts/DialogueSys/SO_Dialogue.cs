using UnityEngine;

[CreateAssetMenu(fileName = "SO_Dialogue", menuName = "Scriptable Objects/SO_Dialogue")]
public class SO_Dialogue : ScriptableObject
{
    public string speaker;
    [TextArea(3, 10)]
    public string text;
    public AudioClip audio;
    public AnimationClip animation;

    public SO_Dialogue next;
}
