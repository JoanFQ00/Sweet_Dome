using FMOD.Studio;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        ChargePlayer();

        if (popUpGO != null) 
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
        }

    }
    protected virtual void OnEnable()
    {
      
        LevelManager.OnLevelLoaded += ChargePlayer;

        GameManager.OnChangeDay += ResetActionState;
    }

    protected virtual void OnDisable()
    {
        LevelManager.OnLevelLoaded -= ChargePlayer;

        GameManager.OnChangeDay -= ResetActionState;
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }

    void ChargePlayer()
    {
        if (GameManager.Instance != null)
        {
            playerController = GameManager.Instance.playerController;
            if (playerController == null)
                playerController = FindFirstObjectByType<PlayerController>();
        }
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
        if (GameManager.Instance == null || playerController == null)
        {
            if (popUpGO.activeSelf) popUpGO.SetActive(false);
            if (interactPopUpGO.activeSelf) interactPopUpGO.SetActive(false);
            return;
        }

        if (GameManager.Instance.currentState != GameManager.GameState.Gameplay)
        {
            SetPopUps(false, false);
            return;
        }

        if (act != null && (!act.alwaysAccesible && DayTime.Instance.GetCurrentHour() != act.hourToInteract))
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