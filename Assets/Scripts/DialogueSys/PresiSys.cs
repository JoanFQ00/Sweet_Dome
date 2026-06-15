using UnityEngine;
using System;

public enum PresiDialogue 
{
    GoodMorningDay1 = 1, 
    GoToWorkDay1 = 2,
    GoToWorkDay2 = 25,
    MachineCompleted = 3,

    ExplainMachineDay1 = 4,
    ExplainMachineDay2 = 5, 
    ExplainMachineDay3 = 6,
    ExplainMachineDay4 = 7,

    MachineOmitedDay1 = 8,
    MachineOmitedDay12ndTime = 9,

    PresiSaysYouCanRead = 10,
    PresiSaysYouCanWatchTv = 11,

    MattewTraitorDay1 = 50,

    GoodMornDayHappy = 68,
    GoodMorNoHappy = 69,

    //DAY 1
    CongratsDay1Presi = 100,
    CongratsAgainDay1Presi = 110,
    ForcesDay1Dorothy = 111,
    SleepDay1Presi = 112,
    AngryFunDay1Presi = 113,
    SemiAngryFunDay1Presi = 114,
    AngryAngryDay1Presi = 115,

    //DAY 2
    PleaseSchoolDay2Presi = 200,
    CrackAtSchoolDay2Presi = 201,
    GoodHappySchoolDay2Dorothy = 202,
    GoodSadDay2Dorothy = 203,
    CongratsDay2Presi = 204,
    SleepDay2Presi = 205,
    VeryAngryDay2Dorothy = 205,
    BadFinalDay2Presi = 206,
    SemiAngryDay2Dorothy = 207,
    AngryDay2Dorothy = 208,
    SemiAngryDay2Presi = 209,

    //DAY 3

    GoodMorDay5 = 102,
    BadMorDay5 = 103,

    DorothySights = 124,
}

public class PresiSys : DialogueSys
{
    private Interlucutors presi = Interlucutors.Presi;
    public SO_Dialogue[] loadedDialogues;

    public int id;
    public int dialogueIdx;
    public SO_Dialogue currentDialogue;

    float currentDialogueTime;
    bool dialogueStarted;

    public static event Action OnDialogueChanged;
    public static event Action OnDialogueTerminated;

    public void SysIni()
    {
        dialogueIdx = 0;
        ChargeDialogues(1);
    }

    private void Update()
    {
        PassDialogueOnTime();
    }

    void PassDialogueOnTime() 
    {
        if (!dialogueStarted) { return; }

        currentDialogueTime += Time.deltaTime;

        if (currentDialogueTime >= currentDialogue.timeOnScreen)
        {
            currentDialogueTime = 0;

            LoadNextIdx();
        }
    }

    public void StartStopDialogue(bool started) 
    {
        dialogueStarted = started;
    }

    public override void ChargeDialogues(int _id)
    {
        id = _id;
        dialogueIdx = 0;
        loadedDialogues = Resources.LoadAll<SO_Dialogue>("Dialogues/" + presi + "/" + id);
        currentDialogue = loadedDialogues[dialogueIdx];

        OnDialogueChanged?.Invoke();
    }

    public override void LoadNextIdx()
    {
        if (dialogueIdx + 1 > loadedDialogues.Length - 1) 
        {
            dialogueStarted = false;
            OnDialogueTerminated?.Invoke();
            return; 
        }

        dialogueIdx++;
        currentDialogue = loadedDialogues[dialogueIdx];

        currentDialogueTime = 0;

        OnDialogueChanged?.Invoke();
    }
}
