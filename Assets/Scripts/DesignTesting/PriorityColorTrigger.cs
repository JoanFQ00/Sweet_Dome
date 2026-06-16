using UnityEngine;

public class PriorityColorTrigger : CustomButtonColorTrigger
{
    [Header("Scene Referencies")]
    [SerializeField] private PriorityPress priorityButtonSource;

    private void OnEnable()
    {
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed += HandlePriorityPressed;
        }
    }

    private void OnDisable()
    {
        if (priorityButtonSource != null)
        {
            priorityButtonSource.OnPriorityButtonPressed -= HandlePriorityPressed;
        }
    }

    protected override void Start()
    {
        base.Start();
        EvaluateDayRestrictions();
    }

    private void HandlePriorityPressed()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1)
        {
            base.RestoreOriginalBehavior();
            return;
        }

        if (targetButton != null)
        {
            targetButton.OverrideEmissionColor(customHdrColor);
        }
    }

    private void EvaluateDayRestrictions()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == Day.Day1)
        {
            base.RestoreOriginalBehavior();
        }
    }

    public override void ResetValues()
    {
        base.ResetValues();
        EvaluateDayRestrictions();
    }
}