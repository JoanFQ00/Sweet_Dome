using TMPro;
using UnityEngine;

public class PresiSpeaker : MonoBehaviour
{
    [Header("Dialogue")]
    public GameObject interactionBubble;
    public TextMeshProUGUI interlocutor;
    public TextMeshProUGUI dialogueTxt;

    [HideInInspector]
    public PresiSys dialogueSys;

    private void Awake()
    {
        dialogueSys = GetComponent<PresiSys>();
    }

    private void OnEnable()
    {
        PresiSys.OnDialogueTerminated += EndDialogue;
        PresiSys.OnDialogueChanged += UpdateUI;
    }

    private void OnDisable()
    {
        PresiSys.OnDialogueTerminated -= EndDialogue;
        PresiSys.OnDialogueChanged -= UpdateUI;
    }

    public void DialogueIni()
    {
        if (dialogueSys == null)
            return;

        dialogueSys.SysIni();

        if (dialogueSys.loadedDialogues == null ||
            dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.LogError("No dialogues loaded");
            return;
        }

        PresiSpeaks();
    }

    public void PresiSpeaks()
    {
        if (dialogueSys == null ||
            dialogueSys.loadedDialogues == null ||
            dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.Log("Dialogue not initialized");
            return;
        }

        dialogueSys.StartStopDialogue(true);

        if (interactionBubble != null)
            interactionBubble.SetActive(true);

        UpdateUI();
    }

    public void PresiSpeaks(int dialogueID)
    {
        if (dialogueSys == null)
            return;

        dialogueSys.ChargeDialogues(dialogueID);

        if (dialogueSys.loadedDialogues == null ||
            dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.LogError("No dialogues loaded");
            return;
        }

        dialogueSys.StartStopDialogue(true);

        if (interactionBubble != null)
            interactionBubble.SetActive(true);

        UpdateUI();
    }
    void UpdateUI()
    {
        if (dialogueSys.loadedDialogues == null ||
            dialogueSys.loadedDialogues.Length == 0)
            return;

        if (dialogueSys.dialogueIdx < 0 ||
            dialogueSys.dialogueIdx >= dialogueSys.loadedDialogues.Length)
            return;

        var current = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx];

        interlocutor.text = current.speaker;
        dialogueTxt.text = current.text;
    }

    void EndDialogue()
    {
        if (interactionBubble != null)
            interactionBubble.SetActive(false);

        dialogueSys.StartStopDialogue(false);
    }

    public void ChangeDialogueID(int id)
    {
        dialogueSys.ChargeDialogues(id);
    }
}