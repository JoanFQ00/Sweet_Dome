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
        robot.animator.SetBool("IsTalking", false);
        robot.animator.SetBool("IsRecharging", false);
        robot.animator.SetBool("IsMoving", true);
        robot.agent.speed = 0.47f;
        robot.agent.acceleration = 8f;
        robot.animator.SetFloat("Speed", 0.47f);
        NextPatrolPoint();
        //if (robot.agent.hasPath)
        //{
        //    Vector3 direction = robot.agent.steeringTarget - robot.visualMesh.position;
        //    direction.y = 0;

        //    if (direction.sqrMagnitude > 0.01f)
        //    {
        //        robot.visualMesh.rotation = Quaternion.LookRotation(direction);
        //    }
        //}
    }

    public void OnState()
    {
        robot.animator.SetFloat("Speed", robot.agent.velocity.magnitude);
        if (robot.testGoToPlayer)
        {
            robot.stateMachine.ChangeState(robot.goToPlayerState);
            return;
        }

        if (robot.agent.hasPath)
        {
            robot.LookAtTarget(robot.agent.steeringTarget);
        }

        if (!robot.agent.pathPending && robot.agent.remainingDistance <= 1f)
        {
            robot.stateMachine.ChangeState(robot.idleState);
        }

        //float distance = Vector3.Distance(robot.transform.position, robot.player.position);

        //if (distance <= robot.interactionRadius)
        //{
        //}
    }

    public void OnExit()
    {
        robot.agent.ResetPath();
        robot.animator.SetBool("IsMoving", false);
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

    //void LookAtTarget(Vector3 target)
    //{
    //    Vector3 direction = target - robot.transform.position;
    //    direction.y = 0;
    //    robot.transform.rotation = Quaternion.LookRotation(direction);
    //}
}