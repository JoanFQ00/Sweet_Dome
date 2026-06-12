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
    DorothyMissesBeBop = 18,

    DorothyExplainsButton = 9,
    DorothyCongratsotNotEat = 10,
    DorothyLaunch = 13,
    DorothyHappyLaunch = 14,
    DorothyAngryDay3 = 15,
    Angry3AndNoBook = 16,

    TryToSkipMachineDay1 = 17,

    PleaseEatMyLaunch = 72,
    ILoveYou = 74,
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
