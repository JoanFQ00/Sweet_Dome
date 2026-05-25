using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GoToPlayerState : IState
{
    private RobotBehaviour robot;
    private float stopDistance = 1f;

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
        robot.gameObject.layer = defaultLayer;
        robot.agent.speed = 3.5f;

        robot.animator.SetBool("IsTalking", false);
    }

    public void OnState()
    {
        robot.animator.SetFloat("Speed", robot.agent.velocity.magnitude);
        if (!robot.agent.pathPending && robot.agent.remainingDistance <= stopDistance)
        {
            robot.stateMachine.ChangeState(robot.interactionState);
            return;
        }

        Vector3 offsetDirection = (robot.player.position - robot.transform.position).normalized;

        robot.agent.SetDestination(robot.player.position - offsetDirection * 1f);

        robot.LookAtTarget(robot.player.position);
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
        robot.gameObject.layer = interactingLayer;
    }
}