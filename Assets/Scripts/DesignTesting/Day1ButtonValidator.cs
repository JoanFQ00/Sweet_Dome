using UnityEngine;

public class Day1ButtonValidator : ButtonValidator
{
    public override bool CanPress()
    {
        if (GameManager.Instance == null) return false;

        if (GameManager.Instance.currentDay == Day.Day1)
        {
            return false;
        }

        return true;
    }
}