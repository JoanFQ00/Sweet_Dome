using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MachineControllerZone : MonoBehaviour
{
    public List<GameObject> screen;
    public List<GameObject> tubeScreens;

    public Color defaultColor;
    public Color selectedColor;

    int indx = 0;

    void Awake()
    {
        ShowScreen(0);
    }

    public void NextScreen()
    {
        indx++;

        if (indx >= screen.Count)
            indx = 0;

        ShowScreen(indx);
    }

    public void PrevScreen()
    {
        indx--;

        if (indx < 0)
            indx = screen.Count - 1;

        ShowScreen(indx);
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
            Debug.Log(j + " " + indx);
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