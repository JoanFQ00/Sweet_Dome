using UnityEngine;

public class DayManager : MonoBehaviour
{


    private void Update()
    {
    }

    public void StartDay() 
    {
        PresidentSpeaks();
    }

   void PresidentSpeaks() 
    {
        GameManager.Instance.presiSpeaker.PresiSpeaks();
    }
}
