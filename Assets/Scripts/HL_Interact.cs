using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    private Color originalColor;

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        originalColor = rend.material.color;
    }

    public void LightenColor() 
    {
        Color lighterCol = Color.Lerp(originalColor, Color.white, lightenAmount);

        rend.material.color = lighterCol;
    }

    public void ResetColor() 
    {
        rend.material.color = originalColor;
    }
}
