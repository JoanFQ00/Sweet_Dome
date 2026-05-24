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

        robot.agent.SetDestination(robot.waypointManager.rechargeWaypoint.position);
    }

    public void OnState()
    {
        if (!robot.agent.pathPending && robot.agent.remainingDistance <= robot.agent.stoppingDistance)
        {
            robot.agent.isStopped = true;
        }
    }

    public void OnExit()
    {
        robot.agent.isStopped = false;
    }
}
