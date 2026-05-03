using UnityEngine;

[CreateAssetMenu(fileName = "SO_Dialogue", menuName = "Scriptable Objects/SO_Dialogue")]
public class SO_Dialogue : ScriptableObject
{
    public string speaker;
    public string text;

    public SO_Dialogue next;
    
}
