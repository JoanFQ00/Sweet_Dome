using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class LevelManager : MonoBehaviour
{
    public enum NameLevels { Gameplay, MenuScene, Final, Credits}

    public static event Action OnLevelLoaded;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MenuScene") return;

        OnLevelLoaded?.Invoke();
    }


    public void LoadGameplay()
    {
        SceneManager.LoadScene(NameLevels.Gameplay.ToString());
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(NameLevels.MenuScene.ToString());
    }

    public void LoadCredits() 
    {
        SceneManager.LoadScene(NameLevels.Credits.ToString());
    }

    public void LoadFinal() 
    {
        SceneManager.LoadScene(NameLevels.Final.ToString());
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
