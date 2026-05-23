using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IState
{
    private RobotBehaviour robot;

    private Waypoints currentWaypoint;

    public PatrolState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    public void OnEnter()
    {
        robot.agent.isStopped = false;
        NextPatrolPoint();
    }

    public void OnState()
    {
        if (robot.testGoToPlayer)
        {
            robot.stateMachine.ChangeState(robot.goToPlayerState);
            return;
        }

        if (robot.agent.hasPath)
        {
            LookAtTarget(robot.agent.destination);
        }

        if (!robot.agent.pathPending && robot.agent.remainingDistance <= 1f)
        {
            robot.stateMachine.ChangeState(robot.idleState);
        }

        float distance = Vector3.Distance(robot.transform.position, robot.player.position);

        if (distance <= robot.interactionRadius)
        {
        }
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
    }

    void NextPatrolPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * robot.patrolRange;
        randomPoint += robot.transform.position;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, robot.patrolRange, NavMesh.AllAreas))
        {
            robot.agent.SetDestination(hit.position);
        }
    }

    void LookAtTarget(Vector3 target)
    {
        Vector3 direction = target - robot.transform.position;
        direction.y = 0;
        robot.transform.rotation = Quaternion.LookRotation(direction);
    }
}