using UnityEngine;
using UnityEngine.UI;

public class CrerditsButton : MonoBehaviour
{
    public Button menuButton;

    private void OnEnable()
    {
        LevelManager.OnLevelLoaded += ThisLevelLoaded;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLoaded -= ThisLevelLoaded;
    }

    void ThisLevelLoaded()
    {
    }

    void GoToMenuAction()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //GameManager.Instance.levelManager.LoadMenu();
        GameManager.Instance.levelManager.ExitGame();
    }
}