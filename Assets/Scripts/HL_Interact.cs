using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    private Color originalColor;
    private BaseAction baseAction;


    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();

        if (rend == null) rend = GetComponent<Renderer>();

        baseAction = GetComponent<BaseAction>(); 

        rend.material = new Material(rend.material);
        originalColor = rend.material.color;
    }

    public void LightenColor()
    {
        if (baseAction != null && baseAction.act != null)
        {
            bool isTime= baseAction.act.hourToInteract == GameManager.Instance.dayTime.currentHour;
            bool isWatchTv = baseAction.act.actionName == "WatchTV";
            if (!isTime && !isWatchTv)
            {
                return;
            }
        }

        rend.material.color = originalColor + (highlightColor * lightenAmount);
    }

    public void ResetColor() 
    {
        rend.material.color = originalColor;
    }
}
