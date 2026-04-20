using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject interactTxt;
    public GameObject leverBar;

    void Start()
    {
        interactTxt.SetActive(false);
        leverBar.SetActive(false);
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
        leverBar.SetActive(visibility);
    }

}
