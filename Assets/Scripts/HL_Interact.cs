using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    private Renderer renderer;
    private Color originalColor;

    [Range(0f, 1f)]
    public float lightenAmount = 0.3f;

    private void Start()
    {
        renderer = GetComponent<Renderer>();

        if (renderer == null) 
        {
            renderer = GetComponentInChildren<Renderer>();    
        }

        originalColor = renderer.material.color;
    }

    public void LightenColor() 
    {
        Color lighterCol = Color.Lerp(originalColor, Color.white, lightenAmount);

        renderer.material.color = lighterCol;
    }

    public void ResetColor() 
    {
        renderer.material.color = originalColor;
    }
}
