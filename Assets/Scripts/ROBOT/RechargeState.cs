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
        robot.agent.SetDestination(robot.waypointManager.rechargeWaypoints.position);
    }

    public void OnState()
    {

    }

    public void OnExit()
    {
        robot.agent.isStopped = false;
    }
}
