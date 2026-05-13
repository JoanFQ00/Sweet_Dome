using TMPro;
using UnityEngine;

public class PresiSpeaker : MonoBehaviour
{
    [Header("Dialogue")]
    public GameObject interactionBubble;
    public TextMeshProUGUI interlocutor;
    public TextMeshProUGUI dialogueTxt;
    private PresiSys dialogueSys;

    private void Awake()
    {
        dialogueSys = GetComponent<PresiSys>();
    }

    private void OnEnable()
    {
        GameManager.Instance.presiSpeaker = this;
        DialogueIni();
        PresiSpeaks();
        dialogueSys.OnDialogueTerminated += EndDialogue;
        dialogueSys.OnDialogueChanged += UpdateUI;
    }

    private void OnDisable()
    {
        dialogueSys.OnDialogueTerminated -= EndDialogue;
        dialogueSys.OnDialogueChanged -= UpdateUI;
    }

    public void DialogueIni()
    {
        dialogueSys.SysIni();

        if (dialogueSys.loadedDialogues == null || dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.LogError("No dialogues loaded");
            return;
        }

        dialogueSys.StartStopDialogue(true);

        interactionBubble.SetActive(true);

        UpdateUI();
    }

    void UpdateUI()
    {
        var current = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx];

        interlocutor.text = current.speaker;
        dialogueTxt.text = current.text;
    }

    void EndDialogue()
    {
        interactionBubble.SetActive(false);

        dialogueSys.StartStopDialogue(false);
    }

    public void ChangeDialogueID(int id) 
    {
        dialogueSys.ChargeDialogues(id);
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

        interactionBubble.SetActive(true);

        UpdateUI();
    }
}
