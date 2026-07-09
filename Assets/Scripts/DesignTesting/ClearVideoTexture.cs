using UnityEngine;

public class ClearVideoTexture : MonoBehaviour
{
    [SerializeField] private RenderTexture videoTexture;

    void Start()
    {
        if (videoTexture != null)
        {
            // Guarda la textura activa actual
            RenderTexture currentActive = RenderTexture.active;

            // Define la textura del video como activa y la limpia con color negro
            RenderTexture.active = videoTexture;
            GL.Clear(true, true, Color.black);

            // Restaura la textura activa previa
            RenderTexture.active = currentActive;
        }
    }
}