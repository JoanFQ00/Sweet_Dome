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
        Debug.Log("Here");
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(GoToMenuAction);
    }

    void GoToMenuAction()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameManager.Instance.levelManager.LoadMenu();
    }
}