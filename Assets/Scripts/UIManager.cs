using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject interactTxt;
    public TextMeshProUGUI timerText, dayText;
    public static UIManager Instance;


    void Start()
    {
        interactTxt.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.NewDay += OnDayChanged;
        Interact.CanInteract += ChangeInteractVisibility;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= OnDayChanged;
        Interact.CanInteract -= ChangeInteractVisibility;
    }

    void ChangeInteractVisibility(bool visibility, bool interacting) 
    {
        Debug.Log(visibility + " " +  interacting);
        if (interacting)
        {
            interactTxt.SetActive(false);
        }
        else 
        {
            interactTxt.SetActive(visibility);
        }
    }

    void OnDayChanged()
    {
        dayText.text = $"{GameManager.Instance.currentDay.ToString().ToUpper()}";
    }
}