using TMPro;
using UnityEngine;

public class PresiSpeaker : MonoBehaviour
{
    [Header("Dialogue")]
    public GameObject interactionBubble;
    public TextMeshProUGUI interlocutor;
    public TextMeshProUGUI dialogueTxt;
    private PresiSys dialogueSys;
    bool speaking = false;

    void Start()
    {
        DialogueIni();
    }

    private void Update()
    {
        if (speaking)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].next == null)
                {
                    interactionBubble.SetActive(false);
                }
                else 
                {
                    LoadNextDialogue();
                }
            }
        }
    }

    void DialogueIni() 
    {
        dialogueSys = GetComponent<PresiSys>();

        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }

    void LoadNextDialogue()
    {
        dialogueSys.LoadNextIdx();

        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }

    public void ChangeDialogueID(int id) 
    {
        dialogueSys.ChargeDialogues(id);
    } 

    public void PresiSpeaks() 
    {
        GameManager.Instance.playerController.LockPlayer(true);
        speaking = true;
        interactionBubble.SetActive(true);
        interlocutor.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].speaker;
        dialogueTxt.text = dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].text;
    }
}
