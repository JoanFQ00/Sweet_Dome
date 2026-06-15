using UnityEngine;
using UnityEngine.UI;

public class CinematicFader : MonoBehaviour
{
    public Image fadeImage;

    [Header("FadeSpeed")]
    public float fadeSpeed = 5f;

    private bool isFadingOut = false;
    private bool isFadingIn = false;

    public static CinematicFader Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (fadeImage == null) return;

        Color imgColor = fadeImage.color;

        if (isFadingOut)
        {
            imgColor.a += Time.unscaledDeltaTime * fadeSpeed;
            if (imgColor.a >= 1f)
            {
                imgColor.a = 1f;
                isFadingOut = false;
            }
            fadeImage.color = imgColor;
        }

        if (isFadingIn)
        {
            imgColor.a -= Time.unscaledDeltaTime * fadeSpeed;
            if (imgColor.a <= 0f)
            {
                imgColor.a = 0f;
                isFadingIn = false;

                fadeImage.gameObject.SetActive(false);
            }
            fadeImage.color = imgColor;
        }
    }

    public void FundidoANegro()
    {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(true);
        isFadingOut = true;
        isFadingIn = false;
    }

    public void FundidoATransparente()
    {
        if (fadeImage == null) return;

        fadeImage.gameObject.SetActive(true);
        isFadingIn = true;
        isFadingOut = false;
    }
}