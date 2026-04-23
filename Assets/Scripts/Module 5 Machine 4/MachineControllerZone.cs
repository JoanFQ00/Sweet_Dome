using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineControllerZone : MonoBehaviour
{
    public List<GameObject> screen;
    public List<GameObject> tubeScreens;

    public Color defaultColor;
    public Color selectedColor;

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

        ChangeColor(i);
    }

    void ChangeColor(int indx) 
    {
        for (int j = 0; j < tubeScreens.Count; j++) 
        {
            Image img = tubeScreens[j].GetComponentInChildren<Image>();

            if (j == indx)
            {
                img.GetComponentInChildren<Image>().color = selectedColor;
            }
            else
            { 
                img.GetComponentInChildren<Image>().color = defaultColor;
            }
        }
    }
}