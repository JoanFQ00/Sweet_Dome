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
    LyingToMatthewDay3Dorothy = 300,//49
    FirstCongratsDay3Presi = 301, //50
    FirstAngryDay3Presi = 302, //51
    UZPriorDay3Presi = 303, //52
    HPriorDay3Dorothy = 304,//53
    HDownDay3Dorothy = 305,//54
    SecondAngryDay3Presi = 306, //55
    All0AngryhDay3Dorothy = 307,//56
    Two0AngryDay3Dorothy = 307,//57
    UZ0Day3Dorothy = 308,//58
    H0Day3Dorothy = 309,//59
    ThxDay3Dorothy = 310,//60
    SadDay3Dorothy = 311,//61
    F5Day3Presi = 312, //62
    Two0Day3Presi = 313, //63
    UZ0Day3Presi = 314, //64
    H0Day3Presi = 315, //65
    FinalCongratsDay3Presi = 316, //66
    SleepDay3Presi = 317, //67

    //DAY 4
    Day3Dorothy = 400,//49

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
