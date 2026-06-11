using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingController : MonoBehaviour
{
    [Header("FinishAnimations")]
    public List<AnimationClip> endingAnimations;

    [Header("Canvas")]
    public GameObject canvasToMenu;
    public Button menuButton;

    [Header("Dialogue")]
    public GameObject dialogueUI;
    public TextMeshProUGUI interlocutorTMP;
    public TextMeshProUGUI dialogueTMP;

    SO_Dialogue[] loadedDialogues;
    SO_Dialogue currentDialogue;
    bool dialogueStarted = false;
    float currentDialogueTime;
    int dialogueIdx;
    int currentId;

    private int currentAnimation;
    private Animation an;

    private void Start()
    {
        an = GetComponent<Animation>();
        dialogueUI.SetActive(false);

        if (GameManager.Instance != null)
        {
            int finalId = GameManager.Instance.pendingFinalId;
            ActivateFinalAnimation(finalId);
        }
    }

    private void OnEnable()
    {
        GameManager.ActivateFinalAnimation += ActivateFinalAnimation;
        LevelManager.OnLevelLoaded += ThisLevelLoaded;
    }

    private void OnDisable()
    {
        GameManager.ActivateFinalAnimation -= ActivateFinalAnimation;
        LevelManager.OnLevelLoaded -= ThisLevelLoaded;
    }

    private void Update()
    {
        UpdateDialogues();
    }

    void ThisLevelLoaded()
    {
        if (menuButton == null && canvasToMenu != null)
        {
            menuButton = canvasToMenu.GetComponentInChildren<Button>();
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();

            menuButton.onClick.AddListener(GoToMenuAction);
        }
    }

    void ActivateFinalAnimation(int id)
    {
        Debug.Log(id);
        ChargeDialogueFinale(id);

        currentAnimation = id;
        AnimationClip targetClip = endingAnimations[id];

        if (targetClip != null)
        {
            if (an.GetClip(targetClip.name) == null)
            {
                an.AddClip(targetClip, targetClip.name);
            }

            an.clip = targetClip;
            an.Play();
        }
    }

    public void ActivateGoToMenu()
    {
        if (canvasToMenu != null)
        {
            canvasToMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void GoToMenuAction()
    {
        GameManager.Instance.levelManager.LoadMenu();
    }

    void UpdateDialogues()
    {
        if (!dialogueStarted) { return; }

        currentDialogueTime += Time.deltaTime;

        if (currentDialogueTime >= currentDialogue.timeOnScreen)
        {
            currentDialogueTime = 0;

            LoadNextIdx();
        }
    }

    void ChargeDialogueFinale(int id)
    {
        Debug.Log("ChargingDialogue");
        switch (id)
        {
            case 0:
                dialogueStarted = true;
                dialogueUI.SetActive(true);
                ChargeDialogues(id);
                break;

            case 1:
                break;

            case 2:
                break;

            case 3:
                break;
        }
    }

    void ChargeDialogues(int _id)
    {
        currentId = _id;
        dialogueIdx = 0;
        loadedDialogues = Resources.LoadAll<SO_Dialogue>("Dialogues/Final/" + currentId);
        currentDialogue = loadedDialogues[dialogueIdx];
        UpdateUI();
    }

    void LoadNextIdx()
    {
        if (dialogueIdx + 1 > loadedDialogues.Length - 1)
        {
            dialogueStarted = false;
            return;
        }

        dialogueIdx++;
        currentDialogue = loadedDialogues[dialogueIdx];
        UpdateUI();

        currentDialogueTime = 0;
    }

    void UpdateUI() 
    {
        interlocutorTMP.text = currentDialogue.speaker;
        interlocutorTMP.text = currentDialogue.text;
    }
}