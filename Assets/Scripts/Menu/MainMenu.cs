using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 2f;

    private bool fadingOut = false;
    private bool loadScene = false;

    public GameObject controlPanel;
    public GameObject optionsPanel;
    public GameObject menuPanel;

    void Start()
    {
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        controlPanel.SetActive(false);
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
        optionsPanel.SetActive(true);
        menuPanel.SetActive(false);
    }
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void ShowControls() 
    {
        controlPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void HideControls() 
    {
        controlPanel.SetActive(false);
        menuPanel.SetActive(true); 
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