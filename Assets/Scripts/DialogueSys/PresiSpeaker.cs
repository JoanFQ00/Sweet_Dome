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

    private void Update()
    {
        if (speaking)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (dialogueSys.loadedDialogues[dialogueSys.dialogueIdx].next == null)
                {
                    interactionBubble.SetActive(false);
                    GameManager.Instance.playerController.LockPlayer(false);
                    speaking = false;
                }
                else 
                {
                    LoadNextDialogue();
                }
            }
        }
    }

    public void DialogueIni() 
    {
        dialogueSys = GetComponent<PresiSys>();
        dialogueSys.SysIni();

        if (dialogueSys.loadedDialogues == null || dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.LogError("No dialogues loaded");
            return;
        }

        UpdateUI(); return;
    }

    void UpdateUI()
    {
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
        if (dialogueSys == null ||
        dialogueSys.loadedDialogues == null ||
        dialogueSys.loadedDialogues.Length == 0)
        {
            Debug.Log("Dialogue not initialized");
            return;
        }

        GameManager.Instance.playerController.LockPlayer(true);
        speaking = true;
        interactionBubble.SetActive(true);

        UpdateUI();
    }
}
