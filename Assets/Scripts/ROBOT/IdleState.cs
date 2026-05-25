using UnityEngine;

public class IdleState : IState
{
    private RobotBehaviour robot;

    private float timer;

    public IdleState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        timer = Random.Range(4f, 10f);
        robot.agent.isStopped = true;
        robot.animator.SetFloat("Speed", 0f);

        robot.animator.SetTrigger("IdleTrigger");
    }

    public void OnState()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            robot.stateMachine.ChangeState(robot.patrolState);
        }
    }

    public void OnExit()
    {
    }
}
