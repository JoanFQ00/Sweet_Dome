using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class RobotBehaviour : MonoBehaviour
{
    public Transform player;
    public float InteractionRadius = 3f;
    public float patrolRange = 10f;

    public GameObject interactionText;

    private NavMeshAgent agent;
    private bool isInteracting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        interactionText.SetActive(false);
        agent.updateRotation = false;
        NextPatrolPoint();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < InteractionRadius && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting)
                StartInteraction();
            else
                EndInteraction();
        }
        if (isInteracting)
        {
            LookAtPlayer();
            return;
        }
        Patrol();
    }
    void StartInteraction()
    {
        isInteracting = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        interactionText.SetActive(true);
    }

    void EndInteraction()
    {
        isInteracting = false;
        agent.isStopped = false;
        interactionText.SetActive(false);
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
    void LookAtTarget(Vector3 target)
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
}