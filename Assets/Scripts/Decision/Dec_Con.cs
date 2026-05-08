using UnityEngine;

[CreateAssetMenu(fileName = "Dec_Con", menuName = "Scriptable Objects/Dec_Con")]
public class Dec_Con : ScriptableObject
{
    public NDec_Act actionToSearch;
    public bool performed;

    public bool IsMet(DecManager decManager) 
    {
        if (performed)
            return decManager.actionsPerformed.Contains(actionToSearch);

        else 
            return decManager.actionsNotPerformed.Contains(actionToSearch);
                
    }
}
