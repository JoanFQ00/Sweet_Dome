using UnityEngine;

public class GoToPlayerState : IState
{
    private RobotBehaviour robot;

    private float stopDistance = 0.5f;

    private int defaultLayer;
    private int interactingLayer;
    private bool waitingForGameplay;

    private float updatePathTimer;

    public GoToPlayerState(RobotBehaviour robot)
    {
        this.robot = robot;
        defaultLayer = LayerMask.NameToLayer("Default");
        interactingLayer = LayerMask.NameToLayer("Interactable");
    }

    public void OnEnter()
    {
        robot.agent.isStopped = false;
        robot.agent.stoppingDistance = stopDistance;
        robot.agent.speed = 1f;
        robot.agent.acceleration = 1f;
        robot.gameObject.layer = defaultLayer;
        robot.animator.SetBool("IsTalking", false);
        robot.animator.SetBool("IsMoving", true);
        updatePathTimer = 0f;

        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LockPlayer(true);
            // GameManager.Instance.playerController.LockCamera(true);
        }
    }

    public void OnState()
    {
        if (waitingForGameplay)
        {
            if (GameManager.Instance.currentState == GameManager.GameState.Gameplay)
            {
                waitingForGameplay = false;
                robot.agent.isStopped = false;
                robot.animator.SetBool("IsMoving", true);

                GameManager.Instance.playerController.LockPlayer(true);
            }
            else
            {
                robot.animator.SetBool("IsMoving", false);
                robot.animator.SetFloat("Speed", 0f);
                return;
            }
        }

        if (GameManager.Instance.currentState == GameManager.GameState.Interacting)
        {
            robot.agent.isStopped = true;
            robot.animator.SetBool("IsMoving", false);
            robot.animator.SetFloat("Speed", 0f);
            waitingForGameplay = true;
            return;
        }

        // Esto ejecuta el Slerp de tu PlayerController frame a frame permitiendo un sutil margen de resistencia
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            GameManager.Instance.playerController.LookAtDorothy();
        }

        robot.animator.SetFloat("Speed", robot.agent.velocity.magnitude);
        updatePathTimer -= Time.deltaTime;

        // ... El resto del método OnState() se queda exactamente igual ...
        if (updatePathTimer <= 0f)
        {
            updatePathTimer = 0.15f;
            Vector3 directionToPlayer = (robot.player.position - robot.transform.position).normalized;
            Vector3 targetPos = robot.player.position - directionToPlayer * stopDistance;
            robot.agent.SetDestination(targetPos);
        }

        if (robot.agent.velocity.magnitude > 0.05f)
        {
            Vector3 moveDirection = robot.agent.velocity.normalized;
            Vector3 lookPos = robot.transform.position + moveDirection;

            robot.LookAtTarget(lookPos);
        }

        if (!robot.agent.pathPending && robot.agent.remainingDistance <= stopDistance + 0.1f)
        {
            robot.LookAtTarget(robot.player.position);

            if (GameManager.Instance.currentState == GameManager.GameState.Interacting)
            {
                robot.agent.isStopped = true;
                waitingForGameplay = true;
                return;
            }

            robot.stateMachine.ChangeState(robot.interactionState);
            return;
        }
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
        robot.animator.SetBool("IsMoving", false);
        robot.animator.SetFloat("Speed", 0);
        robot.gameObject.layer = interactingLayer;
    }
}