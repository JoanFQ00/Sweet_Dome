using UnityEngine;

public class HL_Interact : MonoBehaviour
{
    public Renderer rend;
    private Color originalColor;

    [Range(0f, 1f)]
    public float lightenAmount = 0.7f;

    private void Start()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>(); // Busca automáticamente
        if (rend == null) Debug.LogError("¡No encontré un Renderer en " + gameObject.name + "!");
        originalColor = rend.material.color;
    }

    public void LightenColor() 
    {
        Debug.Log("Intentando cambiar color de: " + gameObject.name);
        if (GameManager.Instance.playerController.GetComponent<Interact>()) 
        {
            return;
        }
        Color lighterCol = Color.Lerp(originalColor, Color.red, lightenAmount);

        rend.material.color = lighterCol;
    }

    public void ResetColor() 
    {
        rend.material.color = originalColor;
    }
}
