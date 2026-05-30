using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    private Color originalColor;

    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        rend.material = new Material(rend.material);
        originalColor = rend.material.color;
    }

    public void LightenColor()
    {
        rend.material.color = originalColor + (highlightColor * lightenAmount);
    }

    public void ResetColor() 
    {
        rend.material.color = originalColor;
    }
}
