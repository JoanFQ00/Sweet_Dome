using UnityEngine;

public class CloseDoorTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GetComponent<BoxCollider>().enabled = false;
        GetComponentInParent<DoorBehaviour>().CloseDoor();
    }
}