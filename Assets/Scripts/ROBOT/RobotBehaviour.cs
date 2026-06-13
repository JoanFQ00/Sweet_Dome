using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class RobotBehaviour : MonoBehaviour, IInteractable
{
    public Transform player;
    public float interactionRadius = 3f;
    public float patrolRange = 10f;
    public Animator animator;

    [Header("Waypoint Manager")]
    public WaypointManager waypointManager;
    public DayManager dayManager;

    [Header("Visual")]
    public Transform visualMesh;

    [Header("Dialogue")]
    public GameObject dialogueBubble;
    public GameObject dialogueSkipIm;
    public TextMeshProUGUI interlocutor;
    public TextMeshProUGUI dialogueTxt;
    public RoboSys dialogueSys;
    public Transform speakPoint; 
    public int currentRandomDialogue;
    public float angleToSpeak;

    public NavMeshAgent agent;
    public StateMachineChanger stateMachine;
    public IdleState idleState;
    public PatrolState patrolState;
    public InteractionState interactionState;
    public GoToPlayerState goToPlayerState;
    public RechargeState rechargeState;

    public TaskMoveState taskMoveState;
    public TaskActionState taskActionState;
    public HistoryMoveState historyMoveState;
    public HistoryActionState historyActionState;
    public Waypoint currentTask;
    public Waypoint currentHistoryWaypoint;

    public bool testGoToPlayer = false;
    private bool isRecharging;
    [HideInInspector] public string activeHistoryEvent = "";
    private bool isExecutingHistory = false;
    public bool isInDialogue;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;

        stateMachine = new StateMachineChanger();

        idleState = new IdleState(this);
        patrolState = new PatrolState(this);
        interactionState = new InteractionState(this);
        goToPlayerState = new GoToPlayerState(this);
        rechargeState = new RechargeState(this);
        taskMoveState = new TaskMoveState(this);
        taskActionState = new TaskActionState(this);
        historyMoveState = new HistoryMoveState(this);
        historyActionState = new HistoryActionState(this);

        dialogueSys.SysIni();

        stateMachine.ChangeState(patrolState);
    }

    void OnEnable()
    {
        GameManager.Instance.robotBehaviour = this;
        LevelManager.OnLevelLoaded += LevelLoaded;
        DayTime.OnTimeSkiped += HandleTimeSkip;
    }

    void OnDisable()
    {
        LevelManager.OnLevelLoaded -= LevelLoaded;
        DayTime.OnTimeSkiped -= HandleTimeSkip;
    }

    void LevelLoaded() 
    {

    }

    public void ChangeGoToPlayer(bool _GoToPlayer) 
    {
        testGoToPlayer = _GoToPlayer;
    }

    void Update()
    {
        //animator.SetFloat("Speed", agent.desiredVelocity.magnitude);
        CheckRechargeState();
        CheckDayManagerHistory();

        stateMachine.Update();
    }
    public void Interact()
    {
        if (stateMachine.CurrentState != interactionState)
        {
            stateMachine.ChangeState(interactionState);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        //Quaternion rotationRobot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.LookRotation(direction);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotationRobot, Time.deltaTime * 5f);
    }

    public void LookAtTarget(Vector3 target)
    {
        Vector3 direction = target - visualMesh.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        visualMesh.rotation = Quaternion.Slerp(visualMesh.rotation,targetRotation,Time.deltaTime * 8f);
    }

    public void LoadNextDialogue()
    {
        dialogueSys.LoadNextIdx();
        UpdateUI();
    }

    public void UpdateUI()
    {
        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }

    void HandleTimeSkip(int hour)
    {
        testGoToPlayer = true;
    }

    void CheckRechargeState()
    {
        int currentHour = GameManager.Instance.dayTime.GetCurrentHour();

        if (!isRecharging && currentHour >= 21)
        {
            isRecharging = true;

            stateMachine.ChangeState(rechargeState);
        }

        if (isRecharging && currentHour < 21)
        {
            isRecharging = false;

            stateMachine.ChangeState(patrolState);
        }
    }

    void CheckDayManagerHistory()
    {
        if (isRecharging || dayManager == null)
            return;

        if (GameManager.Instance.currentDay == Day.Day3)
        {
            if (dayManager.hasRead.IsMet(GameManager.Instance.decManager) && dayManager.dorothyLaunchEated.IsMet(GameManager.Instance.decManager))
            {
                TriggerHistoryEvent("DorothySitDay3");
                return;
            }

            if (dayManager.hasWatchedTv.IsMet(GameManager.Instance.decManager) && dayManager.dorothyBook2Readed.IsMet(GameManager.Instance.decManager))
            {
                TriggerHistoryEvent("DorothyTurnOffTvDay3");
                return;
            }
        }
    }

    public void TriggerHistoryEvent(string eventKey)
    {
        activeHistoryEvent = eventKey;
        if (waypointManager != null)
        {
            Waypoint targetWp = waypointManager.GetHistoryWaypointByKey(eventKey);

            if (targetWp != null)
            {
                agent.SetDestination(targetWp.transform.position);
                stateMachine.ChangeState(historyMoveState);
            }
            else
            {
            }
        }
    }

    public void FinishHistoryAction()
    {
        currentHistoryWaypoint = null;
        stateMachine.ChangeState(patrolState);
    }

    public void ExecuteWaypointAnimation(Waypoint waypoint)
    {
        if (waypoint == null)
            return;

        if (!string.IsNullOrEmpty(waypoint.animationTrigger))
            animator.SetTrigger(waypoint.animationTrigger);
    }

    public void TriggerSitAnimation()
    {
        animator.SetTrigger("SitTrigger");
    }

    public void TriggerTurnOffTvAnimation()
    {
        animator.SetTrigger("TurnOffTvTrigger");
    }
}