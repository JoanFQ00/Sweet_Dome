using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class RobotSimpleFSM : MonoBehaviour, IInteractable
{
    public enum State
    {
        Patrol,
        GoToWaypoint,
        GoToPlayer,
        Talk,
        Idle
    }

    [Header("State")]
    public State currentState;

    [Header("Refs")]
    public Transform player;
    public Camera playerCamera;
    public NavMeshAgent agent;

    [Header("Movement")]
    public float patrolRange = 10f;
    public float detectRange = 6f;
    public float talkRange = 2f;

    [Header("Waypoints")]
    public Transform[] waypoints;

    [Header("Timers")]
    public float changeWaypointTime = 5f;
    private float timer;

    [Header("Dialogue (TU SISTEMA)")]
    public GameObject interactionBubble;
    public TextMeshProUGUI dialogueTxt;
    private RoboSys dialogueSys;

    private bool isTalking;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        interactionBubble.SetActive(false);

        dialogueSys = GetComponent<RoboSys>();

        NextPatrol();
        ChangeState(State.Patrol);
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.GoToWaypoint: GoToWaypoint(); break;
            case State.GoToPlayer: GoToPlayer(); break;
            case State.Talk: Talk(); break;
            case State.Idle: Idle(); break;
        }
    }

    // ---------------- STATES ----------------

    void Patrol()
    {
        LookAt(agent.destination);

        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            NextPatrol();
        }

        if (Vector3.Distance(transform.position, player.position) < detectRange)
        {
            ChangeState(State.GoToPlayer);
            return;
        }

        // cambio cada X segundos
        if (timer >= changeWaypointTime)
        {
            timer = 0f;
            ChangeState(State.GoToWaypoint);
        }
    }

    void GoToWaypoint()
    {
        LookAt(agent.destination);

        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            Transform wp = waypoints[Random.Range(0, waypoints.Length)];
            agent.SetDestination(wp.position);
        }

        if (Vector3.Distance(transform.position, player.position) < detectRange)
        {
            ChangeState(State.GoToPlayer);
        }
    }

    void GoToPlayer()
    {
        agent.SetDestination(player.position);
        LookAt(player.position);

        if (Vector3.Distance(transform.position, player.position) < talkRange)
        {
            ChangeState(State.Talk);
        }
    }

    void Talk()
    {
        agent.isStopped = true;

        LookAt(player.position);

        interactionBubble.SetActive(true);

        if (GameManager.Instance != null &&
            GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(true);
        }

        playerCamera.transform.LookAt(transform);
    }

    void Idle()
    {
        agent.isStopped = true;
        LookAt(player.position);
    }

    // ---------------- TRANSITIONS ----------------

    void ChangeState(State newState)
    {
        timer = 0f;

        if (currentState == State.Talk)
        {
            agent.isStopped = false;
            interactionBubble.SetActive(false);

            if (GameManager.Instance != null &&
                GameManager.Instance.playerController != null)
            {
                GameManager.Instance.playerController.LockPlayer(false);
            }
        }

        currentState = newState;
    }

    // ---------------- PATROL ----------------

    void NextPatrol()
    {
        Vector3 random = Random.insideUnitSphere * patrolRange;
        random += transform.position;

        if (NavMesh.SamplePosition(random, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ---------------- LOOK ----------------

    void LookAt(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    // ---------------- INTERACTION ----------------

    public void Interact()
    {
        if (currentState != State.Talk)
        {
            ChangeState(State.Talk);
        }
        else
        {
            ChangeState(State.Patrol);
        }
    }
}