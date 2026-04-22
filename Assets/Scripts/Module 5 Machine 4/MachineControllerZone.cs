using System.Collections.Generic;
using UnityEngine;

public class MachineControllerZone : MonoBehaviour
{
    public List<GameObject> screen;

    int index = 0;

    void Start()
    {
        ShowScreen(0);
    }

    public void NextScreen()
    {
        index++;

        if (index >= screen.Count)
            index = 0;

        ShowScreen(index);
    }

    public void PrevScreen()
    {
        index--;

        if (index < 0)
            index = screen.Count - 1;

        ShowScreen(index);
    }

    void ShowScreen(int i)
    {
        for (int j = 0; j < screen.Count; j++)
        {
            screen[j].SetActive(false);
        }

        screen[i].SetActive(true);
    }
}