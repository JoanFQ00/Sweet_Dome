using UnityEngine;

[CreateAssetMenu(fileName = "Dec_Con", menuName = "Scriptable Objects/Dec_Con")]
public class Dec_Con : ScriptableObject
{
    public NDec_Act actionToSearch;

    public bool IsMet(DecManager decManager)
    {
        return decManager.actionsPerformed.Exists(a => a.ID == actionToSearch.ID);
    }
}
