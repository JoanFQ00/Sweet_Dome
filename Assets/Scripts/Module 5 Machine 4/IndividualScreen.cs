using TMPro;
using UnityEngine;

public class IndividualScreen : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    public int startValue = 10;
    public float timeToRestart = 1f;

    int actualValue;
    float time;

    void OnEnable()
    {
        actualValue = startValue;
        counterText.text = "Value: " + actualValue;
        time = 0f;
    }

    void Update()
    {
        time += Time.deltaTime;

        if (time >= timeToRestart)
        {
            time = 0f;

            if (actualValue > 0)
            {
                actualValue--;
                counterText.text = "Value: " + actualValue;
            }
        }
    }
}