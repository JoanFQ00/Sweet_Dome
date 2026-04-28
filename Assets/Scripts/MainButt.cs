using UnityEngine;
using UnityEngine.Events;

public class MainButt : MonoBehaviour
{
    public UnityEvent onPress;

    public void Press() 
    {
        onPress?.Invoke();
    }
}
