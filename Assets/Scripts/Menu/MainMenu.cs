using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 2f;

    private bool fadingOut = false;
    private bool loadScene = false;

    void Start()
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
    }

    void Update()
    {
        if (fadingOut)
        {
            Color c = fadeImage.color;

            c.a += fadeSpeed * Time.unscaledDeltaTime;
            fadeImage.color = c;

            if (c.a >= 1f && !loadScene)
            {
                loadScene = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    public void PlayGame()
    {
        fadingOut = true;
    }

    public void OptionsMenu()
    {
        Debug.Log("Options");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}