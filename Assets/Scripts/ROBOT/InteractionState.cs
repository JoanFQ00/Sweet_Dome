using UnityEngine;

public class InteractionState : IState
{
    private RobotBehaviour robot;

    private int defaultLayer;
    private int interactingLayer;

    private bool canExit;
    private bool canAdvanceDialogue;

    private float enterTimer;

    public InteractionState(RobotBehaviour robot)
    {
        this.robot = robot;

        defaultLayer = LayerMask.NameToLayer("Default");
        interactingLayer = LayerMask.NameToLayer("Interactable");
    }

    public void OnEnter()
    {
        robot.ChangeGoToPlayer(false);

        robot.agent.isStopped = true;
        robot.agent.velocity = Vector3.zero;

        robot.gameObject.layer = defaultLayer;

        robot.animator.SetFloat("Speed", 0);
        robot.animator.SetBool("IsTalking", true);

        robot.dialogueBubble.SetActive(true);
        robot.dialogueSkipIm.SetActive(true);

        GameManager.Instance.playerController.LockPlayer(true);

        GameManager.Instance.playerController.GetComponent<Interact>().ChangeInteracting(true);

        robot.UpdateUI();

        canAdvanceDialogue = false;
        canExit = false;

        enterTimer = 0.25f;
    }

    public void OnState()
    {
        LookAtPlayer();

        if (enterTimer > 0)
        {
            enterTimer -= Time.deltaTime;
        }
        else
        {
            canAdvanceDialogue = true;
            canExit = true;
        }

        if (canExit)
        {
            float distance = Vector3.Distance(robot.transform.position,robot.player.position);

            if (distance > robot.interactionRadius)
            {
                ExitDialogue();
                return;
            }
        }

        if (canAdvanceDialogue && Input.GetKeyDown(KeyCode.E))
        {
            var currentDialogue = robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx];
            if (currentDialogue.next == null)
            {
                robot.dialogueSys.ChargeDialogues(robot.dialogueSys.id);

                ExitDialogue();
            }
            else
            {
                robot.LoadNextDialogue();
            }
        }
    }

    public void OnExit()
    {
        robot.animator.SetBool("IsTalking", false);

        robot.dialogueBubble.SetActive(false);
        robot.dialogueSkipIm.SetActive(false);

        robot.gameObject.layer = interactingLayer;

        GameManager.Instance.playerController.LockPlayer(false);

        GameManager.Instance.playerController.GetComponent<Interact>().ChangeInteracting(false);
    }

    private void ExitDialogue()
    {
        robot.stateMachine.ChangeState(robot.patrolState);
    }

    void LookAtPlayer()
    {
        Vector3 direction = (robot.player.position - robot.transform.position).normalized;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            robot.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}