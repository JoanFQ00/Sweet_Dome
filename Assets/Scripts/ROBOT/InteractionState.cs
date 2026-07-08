using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionState : IState
{
    private RobotBehaviour robot;

    private int defaultLayer;
    private int interactingLayer;

    private bool canExit;
    private bool canAdvanceDialogue;

    private float enterTimer;

    private InputAction interactAction;

    public InteractionState(RobotBehaviour robot)
    {
        this.robot = robot;

        defaultLayer = LayerMask.NameToLayer("Default");
        interactingLayer = LayerMask.NameToLayer("Interactable");
    }

    public void OnEnter()
    {
        GameManager.Instance.currentState = GameManager.GameState.Interacting;
        robot.ChangeGoToPlayer(false);

        robot.agent.isStopped = true;
        robot.agent.velocity = Vector3.zero;

        robot.gameObject.layer = defaultLayer;

        robot.animator.SetFloat("Speed", 0f);
        robot.animator.SetBool("IsTalking", true);

        robot.dialogueBubble.SetActive(true);
        robot.dialogueSkipIm.SetActive(true);

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        GameManager.Instance.currentState = GameManager.GameState.Interacting;

        robot.UpdateUI();

        canAdvanceDialogue = false;
        canExit = false;

        enterTimer = 0.25f;

        PlayerInput pInput = GameManager.Instance.playerController.GetComponent<PlayerInput>();
        if (pInput != null)
        {
            interactAction = pInput.actions["Interact"];
            // Nos suscribimos al evento para detectar la pulsación de avanzar diálogo
            interactAction.started += OnDialogueAdvanceInput;
        }

        robot.UpdateUI();
    }

    public void OnState()
    {
        LookAtPlayer();

        Vector3 dirToPlayer = (robot.player.transform.position - robot.speakPoint.position).normalized;
        Vector3 playerForward = robot.player.transform.forward;

        float angleToPlayer = Vector3.Dot(playerForward, dirToPlayer);

        if (angleToPlayer > robot.angleToSpeak)
        {
            GameManager.Instance.playerController.LookAtDorothy();
        }
        else 
        {
            GameManager.Instance.playerController.LockCamera(false);
        }

        if (enterTimer > 0)
        {
            enterTimer -= Time.deltaTime;
        }
        else
        {
            canAdvanceDialogue = true;
            canExit = true;
        }

        //if (canAdvanceDialogue && Input.GetKeyDown(KeyCode.E))
        //{
        //    var currentDialogue = robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx];
        //    if (currentDialogue.next == null)
        //    {
        //        robot.dialogueSys.ChargeDialogues(robot.dialogueSys.id);

        //        ExitDialogue();
        //    }
        //    else
        //    {
        //        robot.LoadNextDialogue();
        //    }
        //}
    }

    public void OnExit()
    {
        if (interactAction != null)
        {
            interactAction.started -= OnDialogueAdvanceInput;
        }

        GameManager.Instance.currentState = GameManager.GameState.Gameplay;
        robot.animator.SetBool("IsTalking", false);

        robot.dialogueBubble.SetActive(false);
        robot.dialogueSkipIm.SetActive(false);

        robot.gameObject.layer = interactingLayer;

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        GameManager.Instance.currentState = GameManager.GameState.Gameplay;
    }

    private void ExitDialogue()
    {
        robot.stateMachine.ChangeState(robot.patrolState);
    }

    private void OnDialogueAdvanceInput(InputAction.CallbackContext ctx)
    {
        if (!canAdvanceDialogue) return;

        var currentDialogue = robot.dialogueSys.loadedDialogues[robot.dialogueSys.dialogueIdx];

        if (currentDialogue.next == null)
        {
            //robot.dialogueSys.ChargeDialogues(robot.dialogueSys.id);
            ExitDialogue();
        }
        else
        {
            robot.LoadNextDialogue();
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = robot.player.position - robot.visualMesh.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        robot.visualMesh.rotation = Quaternion.Slerp(robot.visualMesh.rotation,targetRotation,Time.deltaTime * 8f);
    }
}