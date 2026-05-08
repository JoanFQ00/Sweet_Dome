using NUnit.Framework.Constraints;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public Dec_Con HasEated;
    public Dec_Con HasWatchedTv;
    public Dec_Con HasRead;

    private void Update()
    {
        switch (GameManager.Instance.currentDay) 
        {
            case Day.Day1: 
                
                break;
        }
    }

    void ChangeRobotDialogue(int id) 
    {
        GameManager.Instance.robotBehaviour.dialogueSys.ChargeDialogues(id);
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
