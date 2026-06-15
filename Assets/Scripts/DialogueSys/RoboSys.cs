using UnityEngine;

public enum DialoguesRobo
{
    FirstInteraction = 1,
    HasNotEated = 2,
    HasNotReaded = 3,
    HasNotWatchedTv = 4,
    HasNotEatedDay2 = 5,

    DorothyRecomendsBook1 = 6,
    YouReadDorothyBook = 7,
    DorothyAngryDay2 = 8,
    DontWatchThatProgram = 11,
    MattewSturdy = 12,
    DorothyMissesBeBop = 23,
    DorothySings = 24,

    DorothyExplainsButton = 9,
    DorothyCongratsotNotEat = 10,
    DorothyLaunch = 13,
    DorothyHappyLaunch = 14,
    DorothyAngryDay3 = 15,
    Angry3AndNoBook = 16,

    TryToSkipMachineDay1 = 17,

    EncourageRead = 43,
    EncourageTV = 45,

    PleaseEatMyLaunch = 72,
    VeryMad = 73,
    ILoveYou = 74,
    MadDoro = 75,

    SpecialLaunch = 104,
    AngriedAndNoLaunch = 105,
    LastBookRec = 106,
    WorriedBout = 107,
    YoureNotDif = 109,

    DorothyGoToWindow = 110,
    FinalRevelation = 111,
    InsistToGo = 112,
    DortohySad = 113,
}

public class RoboSys : DialogueSys
{
    private Interlucutors robo = Interlucutors.Robo;
    public SO_Dialogue[] loadedDialogues;

    public int id;
    public int dialogueIdx;

    public SO_Dialogue currentDialogue;

    public void SysIni() 
    {
        dialogueIdx = 0;
        ChargeDialogues(1);
    }

    public override void ChargeDialogues( int _id)
    {
        id = _id;
        dialogueIdx = 0;
        loadedDialogues = Resources.LoadAll<SO_Dialogue>("Dialogues/" + robo + "/" + id);
        currentDialogue = loadedDialogues[dialogueIdx];
    }

    public override void LoadNextIdx()
    {
        if (dialogueIdx + 1 > loadedDialogues.Length - 1) { return; }

        dialogueIdx++;
        currentDialogue = loadedDialogues[dialogueIdx];
    }
}
