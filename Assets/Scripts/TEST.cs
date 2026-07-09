using UnityEngine;
using UnityEngine.UI;

public class TEST : MonoBehaviour
{
    public Image fadeImage;

    [Header("FadeSpeed")]
    public float fadeSpeed = 5f;

    private bool isFadingOut = false;
    private bool isFadingIn = false;

    float counter = 0;

    public static TEST Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        isFadingOut = true;
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        counter += Time.deltaTime;
        if (counter < 1) return;


        Color imgColor = fadeImage.color;

        imgColor.a += Time.unscaledDeltaTime * fadeSpeed;
        if (imgColor.a >= 1f)
        {
            imgColor.a = 1f;
            isFadingOut = false;
        }
        fadeImage.color = imgColor;

        if (isFadingOut == false)
        {
            Destroy(gameObject);
        }
    }
}