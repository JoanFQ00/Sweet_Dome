using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class TubeButtons : MonoBehaviour
{
    public enum ButtonType { Left, Right, ChangeActionMode, ChangeMode}
    public ButtonType Type;

    TubeController tubeController;

    void Start()
    {
        tubeController = GetComponentInParent<TubeController>();
    }

    public void Press() 
    {
        if (Type == ButtonType.ChangeMode)
        {
            tubeController.ChangeMode();
        }

        if (Type == ButtonType.Right)
        {
            tubeController.ChangeTubePositionRight();
        }

        if (Type == ButtonType.Left)
        {
            tubeController.ChangeTubePositionLeft();
        }

        if (Type == ButtonType.ChangeActionMode)
        {
            tubeController.ActivateTube();
        }
    }
}
