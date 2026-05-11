using UnityEngine;

public class NumberButton : MonoBehaviour
{
    public int number;
    public CodeSystem codeSystem;

    public void Press()
    {
        codeSystem.AddNumber(number);
    }
}