using UnityEngine;
public class InteractionState : IState
{
    private RobotBehaviour robot;

    public InteractionState(RobotBehaviour robot)
    {
        this.robot = robot;
    }

    //public void OnEnter()
    //{
    //    robot.agent.isStopped = true;
    //    robot.agent.velocity = Vector3.zero;
    //    robot.interactionBubble.SetActive(true);

    //    GameManager.Instance.playerController.LockPlayer(true);

    //    //robot.dialogueSys.ChargeDialogues(1);
    //    InitDialogue();
    //}
    public void OnEnter()
    {
        robot.agent.isStopped = true;
        robot.agent.velocity = Vector3.zero;

        robot.interactionBubble.SetActive(true);

        GameManager.Instance.playerController.LockPlayer(true);

        robot.UpdateUI();
    }

    public void OnState()
    {
        LookAtPlayer();

        float distance = Vector3.Distance(robot.transform.position, robot.player.position);

        if (distance > robot.interactionRadius)
        {
            robot.stateMachine.ChangeState(robot.patrolState);
        }
    }

    public void OnExit()
    {
        robot.interactionBubble.SetActive(false);
        GameManager.Instance.playerController.LockPlayer(false);
    }

    void LookAtPlayer()
    {
        Vector3 direction = (robot.player.position - robot.transform.position).normalized;
        direction.y = 0;
        robot.transform.rotation = Quaternion.LookRotation(direction);
    }

    void InitDialogue()
    {
        robot.interlocutor.text = robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx].speaker;
        robot.dialogueTxt.text = robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx].text;
        GameManager.Instance.robotBehaviour.UpdateUI();
    }
}