using UnityEngine;

public class ActionReader : MonoBehaviour, IInteractable
{
    public Dec_Act act;

    public void Interact() 
    {
        act.Execute();
    }
}
