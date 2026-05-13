using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject interactTxt;
    public TextMeshProUGUI timerText;
    public static UIManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        interactTxt.SetActive(false);
    }

    private void OnEnable()
    {
        Interact.CanInteract += ChangeInteractVisibility;
        LevelManager.OnLevelLoaded += LevelLoaded;
    }

    private void OnDisable()
    {
        Interact.CanInteract -= ChangeInteractVisibility;
        LevelManager.OnLevelLoaded -= LevelLoaded;
    }

    private void LevelLoaded()
    {
        GameManager.Instance.dayTime.timerText = timerText;
    }

    void ChangeInteractVisibility(bool visibility) 
    {
        interactTxt.SetActive(visibility);
    }
}