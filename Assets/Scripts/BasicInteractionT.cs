using UnityEngine;

public class BasicInteractionT : MonoBehaviour, IInteractable
{
    public void Interact() 
    {
        Debug.Log("Interacteeeeeed");
        GetComponent<Rigidbody>().AddForce(new Vector3(0, 0, 10), ForceMode.Impulse );
    }
}
