using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject interactTxt;
    public TextMeshProUGUI timerText;
    public static UIManager Instance;


    void Start()
    {
        interactTxt.SetActive(false);
    }

    private void OnEnable()
    {
        Interact.CanInteract += ChangeInteractVisibility;
    }

    private void OnDisable()
    {
        Interact.CanInteract -= ChangeInteractVisibility;
    }

    void ChangeInteractVisibility(bool visibility) 
    {
        interactTxt.SetActive(visibility);
    }
}