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
    CongratsDay1Presi = 100, //07
    CongratsAgainDay1Presi = 110, //11
    ForcesDay1Dorothy = 111, //08
    SleepDay1Presi = 112, //13
    AngryFunDay1Presi = 113, //10
    SemiAngryFunDay1Presi = 114, //09
    AngryAngryDay1Presi = 115, //12

    //DAY 2
    PleaseSchoolDay2Presi = 200, //27
    CrackAtSchoolDay2Presi = 201, //28
    GoodHappySchoolDay2Dorothy = 202, //32
    GoodSadDay2Dorothy = 203, //33
    CongratsDay2Presi = 204, //36
    SleepDay2Presi = 205, //37
    VeryAngryDay2Dorothy = 205, //29
    BadFinalDay2Presi = 206, //34
    SemiAngryDay2Dorothy = 207, //30
    AngryDay2Dorothy = 208, //31
    SemiAngryDay2Presi = 209, //35

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
    D0ay4 = 400,//78
    D1ay4 = 401,//79
    D2ay4 = 402,//80 //
    D3ay4 = 403,//81
    D4ay4 = 404,//82
    D5ay4 = 405,//83 //
    D6ay4 = 406,//84 //
    D7ay4 = 407,//85
    D8ay4 = 408,//86
    D9ay4 = 409,//87
    D10ay4 = 410,//88
    D11ay4 = 411,//89
    D12ay4 = 412,//90
    D13Day4 = 413,//91
    D14Day4 = 414,//92
    D15Day4 = 415,//93
    D16Day4 = 416,//94
    D17Day4 = 417,//95
    D18Day4 = 418,//96
    D19Day4 = 419,//97
    D20Day4 = 420,//98
    D21Day4 = 421,//99
    D22Day4 = 422,//100
    D23Day4 = 423,//101

    //DAY 5
    D0ay5 = 500,//115
    D1ay5 = 501,//116
    D2ay5 = 502,//117
    D3ay5 = 503,//118
    D4ay5 = 504,//119
    D5ay5 = 505,//120
    D6ay5 = 506,//121
    D7ay5 = 507,//122

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
