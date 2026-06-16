using UnityEngine;

public class CustomButtonColorTrigger : MonoBehaviour, IResetable
{
    [Header("Base Referencies")]
    [SerializeField] protected ButtonEmissionController targetButton;

    [Header("Color Base Config")]
    [ColorUsage(true, true)]
    [SerializeField] protected Color customHdrColor = Color.blue;

    protected virtual void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterResetable(this);
        }
    }

    protected virtual void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterResetable(this);
        }
    }

    public virtual void ChangeToCustomColor()
    {
        if (targetButton != null)
        {
            targetButton.OverrideEmissionColor(customHdrColor);
        }
    }

    public virtual void RestoreOriginalBehavior()
    {
        if (targetButton != null)
        {
            targetButton.ClearOverride();
        }
    }

    public virtual void ResetValues()
    {
        RestoreOriginalBehavior();
    }
}