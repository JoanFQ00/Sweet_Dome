using UnityEngine;
public class InteractionState : IState
{
    private RobotBehaviour robot;

    private int defaultLayer;
    private int interactingLayer;

    bool canAdvanceDialogue;

    public InteractionState(RobotBehaviour robot)
    {
        this.robot = robot;

        defaultLayer = LayerMask.NameToLayer("Default");
        interactingLayer = LayerMask.NameToLayer("Interactable");
    }

    public void OnEnter()
    {
        robot.agent.isStopped = true;
        robot.agent.velocity = Vector3.zero;
        robot.gameObject.layer = defaultLayer;

        robot.dialogueBubble.SetActive(true);
        robot.dialogueSkipIm.SetActive(true);

        GameManager.Instance.playerController.LockPlayer(true);

        robot.UpdateUI();

        canAdvanceDialogue = false;
    }

    public void OnState()
    {
        LookAtPlayer();

        if (!canAdvanceDialogue)
        {
            if (!Input.GetKey(KeyCode.E))
            {
                canAdvanceDialogue = true;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            var currentDialogue =
                robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx];

            if (currentDialogue.next == null)
            {
                robot.dialogueSys.ChargeDialogues(robot.dialogueSys.id);
                robot.stateMachine.ChangeState(robot.patrolState);
                if (robot.testGoToPlayer) { robot.testGoToPlayer = false; }
            }
            else
            {
                robot.LoadNextDialogue();
            }
        }

        float distance = Vector3.Distance(robot.transform.position, robot.player.position);

        if (distance > robot.interactionRadius)
        {
            robot.stateMachine.ChangeState(robot.patrolState);
        }
    }

    public void OnExit()
    {
        robot.dialogueBubble.SetActive(false);
        robot.dialogueSkipIm.SetActive(false);

        robot.gameObject.layer = interactingLayer;
        GameManager.Instance.playerController.LockPlayer(false);

        GameManager.Instance.playerController.
        GetComponent<Interact>().ChangeInteracting(false);
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