using UnityEngine;

public class ButtonTemperature : MonoBehaviour
{
    public enum ButtonTypeTemperature
    {
        Up,
        Down
    }

    public ButtonTypeTemperature typeButton;

    public TemperatureSistem system;

    void OnMouseDown()
    {
        if (typeButton == ButtonTypeTemperature.Up)
        {
            system.TemperatureUp();
        }
        else
        {
            system.TemperatureDown();
        }
    }
}
