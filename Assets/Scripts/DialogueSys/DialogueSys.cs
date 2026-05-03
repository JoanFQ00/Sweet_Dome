using UnityEngine;
using System.Collections.Generic;

public enum Interlucutors { Robo, President}

public abstract class DialogueSys : MonoBehaviour
{
    public abstract void ChargeDialogues(Interlucutors interlocutor, int id);

    public abstract void LoadNextIdx();
}
