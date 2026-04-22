using UnityEngine;

public class WorldZoneButton : MonoBehaviour
{
    public enum ButtonTypeWorld
    {
        Left,
        Right
    }

    public ButtonTypeWorld buttonTypeWorld;
    public MachineControllerZone controllerZone;

    public void Press()
    {
        if (buttonTypeWorld == ButtonTypeWorld.Left)
            controllerZone.PrevScreen();
        else
            controllerZone.NextScreen();
    }
}
