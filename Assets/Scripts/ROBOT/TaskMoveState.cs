using UnityEngine;

public class TaskMoveState : IState
{
    private RobotBehaviour robot;

    public TaskMoveState (RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        if (robot.currentTask == null)
        {
            robot.stateMachine.ChangeState(robot.patrolState);
            return;
        }

        robot.agent.isStopped = false;
        robot.animator.SetBool("IsMoving", true);
        robot.agent.SetDestination(robot.currentTask.transform.position);
    }

    public void OnState()
    {
        float currentSpeed = robot.agent.velocity.magnitude;
        robot.animator.SetFloat("Speed", currentSpeed);

        if (robot.agent.hasPath)
        {
            robot.LookAtTarget(robot.agent.steeringTarget);
        }

        if (!robot.agent.pathPending && robot.agent.remainingDistance <= 0.6f)
        {
            robot.agent.ResetPath();
            robot.animator.SetFloat("Speed", 0f);
            robot.animator.SetBool("IsMoving", false);

            robot.stateMachine.ChangeState(robot.taskActionState);
        }
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
    }
}