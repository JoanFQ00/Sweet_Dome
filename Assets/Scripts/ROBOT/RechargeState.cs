using UnityEngine;

public class RechargeState : IState
{
    private RobotBehaviour robot;

    public RechargeState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        if (!robot.agent.isOnNavMesh)
            return;

        robot.agent.isStopped = false;
        robot.animator.SetBool("IsRecharging", true);
        robot.agent.SetDestination(robot.waypointManager.rechargeWaypoint.position);
    }

    public void OnState()
    {
        if (!robot.agent.pathPending && robot.agent.remainingDistance <= robot.agent.stoppingDistance)
        {
            robot.agent.isStopped = true;
            robot.animator.SetFloat("Speed", 0);
        }
    }

    public void OnExit()
    {
        robot.agent.isStopped = false;
        robot.animator.SetBool("IsRecharging", false);
    }
}
