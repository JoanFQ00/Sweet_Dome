using UnityEngine;

[RequireComponent(typeof(HL_Interact))]

public class BasicInt : MonoBehaviour, IInteractable, ILookable
{
    private HL_Interact highlight;
    private PlayerController playerController;

    private enum Type {Lights, Radio, Megaphone}
    [SerializeField] private Type type;


    [Header("Pop Up")]
    [SerializeField] private float disanceToPop = 1.6f;
    [SerializeField] private GameObject popUpGO;
    [SerializeField] private GameObject interactPopUpGO;
    private bool onSight;

    void Start()
    {
        highlight = GetComponent<HL_Interact>();
        playerController = GameManager.Instance.playerController;
    }

    void Update() 
    {
        if (popUpGO != null)
        {
            PopUpCheck();
        }
    }

    public void Interact() 
    {
        switch (type) 
        {
            case Type.Lights:
                break;

            case Type.Radio:
                break;

            case Type.Megaphone:
                break;
        }
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }

    private void PopUpCheck()
    {
        if (GameManager.Instance.currentState == GameManager.GameState.Gameplay)
        {
            if (playerController == null) return; 

            float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

            if (onSight)
            {
                popUpGO.SetActive(false);
                interactPopUpGO.SetActive(true);
                interactPopUpGO.transform.LookAt(playerController.transform);
            }
            else if (distanceToPlayer <= disanceToPop)
            {
                popUpGO.SetActive(true);
                interactPopUpGO.SetActive(false);
                popUpGO.transform.LookAt(playerController.transform);
            }
            else
            {
                popUpGO.SetActive(false);
                interactPopUpGO.SetActive(false);
            }
        }
        else
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
        }
    }
}
