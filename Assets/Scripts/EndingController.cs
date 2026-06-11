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
    public float printSpeed;
    [Range(0f, 1f)]
    public float printTimePct = 0.8f;

    float lastCharTime;
    int charsPrinted;

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
        UpdateSlowPrinter();
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
        ChargeDialogueFinale(id);

        an.Play();
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

    void UpdateSlowPrinter() 
    {
        if (dialogueTMP.text.Length >= currentDialogue.text.Length)
            return;

        if (Time.time >= lastCharTime + printSpeed) 
        {
            charsPrinted++;
            dialogueTMP.text = currentDialogue.text.Substring(0, charsPrinted);
            lastCharTime = Time.time;   
        }
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
        dialogueStarted = true;
        dialogueUI.SetActive(true);
        ChargeDialogues(id);
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

        float targetTime = currentDialogue.timeOnScreen * printTimePct;
        int textLenght = currentDialogue.text.Length;

        if (textLenght > 0)
        {
            printSpeed = targetTime / textLenght;
        }
        else 
        {
            printSpeed = 0.05f;
        }

        charsPrinted = 0;
        dialogueTMP.text = "";
        lastCharTime = Time.time;
    }
}