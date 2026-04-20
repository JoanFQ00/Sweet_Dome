using UnityEngine;

public class ButtonGame : MonoBehaviour
{
    public enum ButtonType
    {
        Accept,
        Deny
    }

    public ButtonType type;

    public void Press()
    {
        if (type == ButtonType.Accept)
        {
            Debug.Log("ACEPTADO");
        }
        else
        {
            Debug.Log("DENEGADO");
        }
    }
}
