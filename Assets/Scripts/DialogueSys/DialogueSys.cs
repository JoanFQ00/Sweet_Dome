using UnityEngine;
using System.Collections.Generic;

public enum Interlucutors { Robo, Presi, RadioMen, RadioF1, RadioF2}

public abstract class DialogueSys : MonoBehaviour
{
    public abstract void ChargeDialogues(int id);

    public abstract void LoadNextIdx();
}
