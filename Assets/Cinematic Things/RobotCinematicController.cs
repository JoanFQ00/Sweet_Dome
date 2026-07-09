using UnityEngine;
using UnityEngine.AI;

public class RobotCinematicController : MonoBehaviour
{
    [Header("Robot References")]
    public NavMeshAgent navMeshAgent;
    public RobotBehaviour robotBehaviorScript; // Cambiado de MonoBehaviour a RobotBehaviour para acceder a sus variables
    public GameObject visualObject;

    public static RobotCinematicController Instance;

    private void Awake()
    {
        Instance = this;

        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (robotBehaviorScript == null)
            robotBehaviorScript = GetComponent<RobotBehaviour>();
    }

    public void DisableAIBehavior()
    {
        // 1. Frenamos la máquina de estados antes de apagar el script
        if (robotBehaviorScript != null && robotBehaviorScript.agent != null)
        {
            if (robotBehaviorScript.agent.isOnNavMesh)
            {
                robotBehaviorScript.agent.isStopped = true;
                robotBehaviorScript.agent.ResetPath();
            }
            robotBehaviorScript.animator.SetFloat("Speed", 0f);
            robotBehaviorScript.animator.SetBool("IsMoving", false);
        }

        // 2. Ahora desactivamos de forma segura
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

            // Forzamos al robot a regresar a un estado seguro (Patrol o Idle) 
            // para que no se quede atrapado en el estado anterior a la cinemática
            if (robotBehaviorScript.stateMachine != null)
            {
                robotBehaviorScript.stateMachine.ChangeState(robotBehaviorScript.idleState);
            }
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