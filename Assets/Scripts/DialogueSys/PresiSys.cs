using UnityEngine;
using System.Collections.Generic;
using System;

public enum PresiDialogue 
{
    GoodMorningDay1 = 1, 
    GoToWorkDay1 = 2,
    MachineCompleted = 3
}

public class PresiSys : DialogueSys
{
    private Interlucutors presi = Interlucutors.Presi;
    public SO_Dialogue[] loadedDialogues;

    public int id;
    public int dialogueIdx;
    public SO_Dialogue currentDialogue;

    public void SysIni()
    {
        dialogueIdx = 0;
        ChargeDialogues(1);
    }

    public override void ChargeDialogues(int _id)
    {
        id = _id;
        dialogueIdx = 0;
        loadedDialogues = Resources.LoadAll<SO_Dialogue>("Dialogues/" + presi + "/" + id);
        currentDialogue = loadedDialogues[dialogueIdx];
    }

    public override void LoadNextIdx()
    {
        if (dialogueIdx + 1 > loadedDialogues.Length - 1) { return; }

        dialogueIdx++;
        currentDialogue = loadedDialogues[dialogueIdx];
    }
}
