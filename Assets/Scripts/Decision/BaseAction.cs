//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour, IInteractable, ILookable
{
    public NDec_Act act;

    [Header("Pop Up")]
    [SerializeField] private float disanceToPop = 1.6f;
    [SerializeField] private GameObject popUpGO;
    [SerializeField] private GameObject interactPopUpGO;
    private bool onSight;

    PlayerController playerController;
    protected bool actionCompleted = false;
    protected bool actionTransfered = false;
    protected bool canInteract = false;

    protected int currentLayer = -1;

    private void Start()
    {
        playerController = GameManager.Instance.playerController;
        Debug.Log(playerController);


        if (popUpGO != null) 
        {
            popUpGO.SetActive(false);
        }

    }
    protected virtual void OnEnable()
    {
        if (GameManager.Instance != null && playerController == null)
        {
            playerController = GameManager.Instance.playerController;
            Debug.Log(playerController);
        }

        GameManager.OnChangeDay += ResetActionState;
    }

    protected virtual void OnDisable()
    {
        GameManager.OnChangeDay -= ResetActionState;
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }

    protected virtual void Update()
    {
        if (actionTransfered) return;

        if (act != null) 
        {
            CheckHour();
        }

        if (popUpGO != null)
        {
            PopUpCheck();
        }

        OnUpdateAction();
    }

    public void CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (act.alwaysAccesible || hour == act.hourToInteract)
        {
            newLayer = LayerMask.NameToLayer("Interactable");
        }
        else
        {
            newLayer = LayerMask.NameToLayer("Default");
        }

        if (currentLayer != newLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }

    private void PopUpCheck()
    {
        if (GameManager.Instance.currentState == GameManager.GameState.Gameplay)
        {
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

    public abstract void Interact();

    protected virtual void OnUpdateAction() { }

    public void ResetActionState(Day c_Day)
    {
        actionCompleted = false;
        actionTransfered = false;
    }

    protected virtual void OnActionExecuted()
    {
        act.ActionExecuted();
        DayTime.Instance.SetCurrentTime(act.passTimeToHour, act.passTimeToMinute);
        actionCompleted = true;
        actionTransfered = true;
    }  
}