using UnityEngine;

public class ButtonTemperature : MonoBehaviour
{
    public enum ButtonTypeTemperature
    {
        Up,
        Down
    }

    public ButtonTypeTemperature typeButton;

    public TemperatureSistem sistem;

    void OnMouseDown()
    {
        if (typeButton == ButtonTypeTemperature.Up)
        {
            sistem.TemperatureUp();
        }
        else
        {
            sistem.TemperatureDown();
        }
    }
}
