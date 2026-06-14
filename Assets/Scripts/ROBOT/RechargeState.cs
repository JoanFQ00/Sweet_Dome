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

        robot.agent.isStopped = true;
        robot.agent.ResetPath();
        Vector3 rechargePos = robot.waypointManager.rechargeWaypoint.position;
        robot.agent.Warp(rechargePos);
        robot.transform.rotation = robot.waypointManager.rechargeWaypoint.rotation;
        robot.animator.SetFloat("Speed", 0);
        robot.animator.ResetTrigger("IdleTrigger");
        robot.animator.ResetTrigger("SitTrigger");
        robot.animator.ResetTrigger("TurnOffTvTrigger");

        robot.animator.SetBool("IsRecharging", true);
    }

    public void OnState()
    {
        robot.animator.SetBool("IsRecharging", true);
        robot.animator.ResetTrigger("IdleTrigger");
    }

    public void OnExit()
    {
        robot.animator.SetBool("IsRecharging", false);
        robot.agent.isStopped = false;
    }
}
