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

    public NavMeshAgent agent;
    private bool isInteracting = false;
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
        else
        {
            if (dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].next == null)
            {
                stateMachine.ChangeState(patrolState);
            }
            else
            {
                LoadNextDialogue();
            }
        }
    }

    void StartInteraction()
    {
        isInteracting = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        dialogueSys.ChargeDialogues(1);
        UpdateUI();
        interactionBubble.SetActive(true);
        GameManager.Instance.playerController.LockPlayer(true);
    }

    void EndInteraction()
    {
        isInteracting = false;
        agent.isStopped = false;
        interactionBubble.SetActive(false);
        GameManager.Instance.playerController.LockPlayer(false);
        NextPatrolPoint();
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

    void Patrol()
    {
        if (agent.hasPath)
        {
            LookAtTarget(agent.destination);
        }

        if (!agent.hasPath || agent.remainingDistance <1f)
        {
            NextPatrolPoint();
        }
    }

    void NextPatrolPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * patrolRange;
        randomPoint += transform.position;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void DialogueIni() 
    {
        dialogueSys = GetComponent<RoboSys>();

        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }

    void LoadNextDialogue() 
    {
        dialogueSys.LoadNextIdx();

        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }

    void UpdateUI()
    {
        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }
}