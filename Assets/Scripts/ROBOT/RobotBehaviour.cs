using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class RobotBehaviour : MonoBehaviour, IInteractable
{
    public Transform player;
    public float interactionRadius = 3f;
    public float patrolRange = 10f;

    [Header("Dialogue")]
    public GameObject interactionBubble;
    public TextMeshProUGUI interlocutor;
    public TextMeshProUGUI dialogueTxt;
    public RoboSys dialogueSys;

    [HideInInspector]
    public int currentRandomDialogue;

    public NavMeshAgent agent;
    public StateMachineChanger stateMachine;
    public PatrolState patrolState;
    public InteractionState interactionState;
    public GoToPlayerState goToPlayerState;

    public bool testGoToPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        stateMachine = new StateMachineChanger();

        patrolState = new PatrolState(this);
        interactionState = new InteractionState(this);
        goToPlayerState = new GoToPlayerState(this);

        stateMachine.ChangeState(patrolState);
    }

    void OnEnable()
    {
        GameManager.Instance.robotBehaviour = this;
        SkipTimeButt.OnFirstDayButt += ChangeGoToPlayer;
    }

    void OnDisable()
    {
        SkipTimeButt.OnFirstDayButt -= ChangeGoToPlayer;
    }

    void ChangeGoToPlayer(bool _GoToPlayer) 
    {
        testGoToPlayer = _GoToPlayer;
    }

    void Update()
    {
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
        Vector3 direction = target - transform.position;
        direction.y = 0;

        //Quaternion rotationRob = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.LookRotation(direction);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotationRob, Time.deltaTime * 5f);
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
}