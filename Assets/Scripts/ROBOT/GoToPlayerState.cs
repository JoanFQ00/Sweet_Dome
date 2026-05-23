using Unity.VisualScripting;
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
        robot.gameObject.layer = defaultLayer; 
    }

    public void OnState()
    {
        float distance = Vector3.Distance(robot.transform.position, robot.player.position);
        if (distance <= stopDistance)
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
