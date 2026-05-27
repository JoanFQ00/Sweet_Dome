using UnityEngine;
using UnityEngine.AI;

public class GoToPlayerState : IState
{
    private RobotBehaviour robot;

    private float stopDistance = 0.5f;

    private int defaultLayer;
    private int interactingLayer;

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
        robot.agent.speed = 1.2f;
        robot.agent.acceleration = 2f;
        robot.gameObject.layer = defaultLayer;
        robot.animator.SetBool("IsTalking", false);
        robot.animator.SetBool("IsMoving", true);
        updatePathTimer = 0f;
    }

    public void OnState()
    {
        robot.animator.SetFloat("Speed",robot.agent.velocity.magnitude);
        updatePathTimer -= Time.deltaTime;

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