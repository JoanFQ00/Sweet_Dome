using UnityEngine;
using UnityEngine.AI;

public class RobotCinematicController : MonoBehaviour
{
    [Header("Robot References")]
    public NavMeshAgent navMeshAgent;
    public MonoBehaviour robotBehaviorScript;
    public GameObject visualObject;

    public static RobotCinematicController Instance;

    private void Awake()
    {
        Instance = this;

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void DisableAIBehavior()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        if (robotBehaviorScript != null)
        {
            robotBehaviorScript.enabled = false;
        }
    }

    public void EnableAIBehavior()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }

        if (robotBehaviorScript != null)
        {
            robotBehaviorScript.enabled = true;
        }

        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();
        }
    }

    public void SetVisualActive(bool isActive)
    {
        if (visualObject != null)
        {
            visualObject.SetActive(isActive);
        }
    }
}