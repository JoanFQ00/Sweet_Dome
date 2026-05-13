using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.ShaderGraph.Internal;

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

    public float dialoguePassTime;
    float currentDialogueTime;
    bool dialogueStarted;

    public event Action OnDialogueChanged;
    public event Action OnDialogueTerminated;

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

        if (currentDialogueTime >= dialoguePassTime)
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
