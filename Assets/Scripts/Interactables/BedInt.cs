using UnityEngine;
using System;

public class BedInt : MonoBehaviour, IInteractable, ILookable
{
    public static Action OnInteract;

    [Header("Pop Up")]
    [SerializeField] private float disanceToPop = 1.6f;
    [SerializeField] private GameObject popUpGO;
    [SerializeField] private GameObject interactPopUpGO;
    PlayerController playerController;
    private bool onSight;

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        LevelManager.OnLevelLoaded += Init;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLoaded -= Init;
    }

    void Init() 
    {
        if (popUpGO != null)
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
            playerController = GameManager.Instance.playerController;
        }
    }

    void Update() 
    {
        gameObject.layer = LayerMask.NameToLayer
            (DayTime.Instance.CanSleepInBed() ? "Interactable" : "Default");

        if (popUpGO != null)
        {
            PopUpCheck();
        }
    }

    public void Interact() 
    {
        OnInteract?.Invoke();
        DayTime.Instance.ResetDayTime();
    }

    private void PopUpCheck()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Gameplay)
        {
            SetPopUps(false, false);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

        if (onSight)
        {
            SetPopUps(false, true);
            interactPopUpGO.transform.LookAt(playerController.transform);
        }
        else if (distanceToPlayer <= disanceToPop)
        {
            SetPopUps(true, false);
            popUpGO.transform.LookAt(playerController.transform);
        }
        else
        {
            SetPopUps(false, false);
        }
    }

    private void SetPopUps(bool showNormal, bool showInteract)
    {
        if (popUpGO.activeSelf != showNormal) popUpGO.SetActive(showNormal);
        if (interactPopUpGO.activeSelf != showInteract) interactPopUpGO.SetActive(showInteract);
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }
}
