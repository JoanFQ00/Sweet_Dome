using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class EndingController : MonoBehaviour
{
    [Header("FinishAnimations")]
    public List<AnimationClip> endingAnimations;

    [Header("Canvas")]
    public GameObject canvasToMenu;
    public Button menuButton;

    private int currentAnimation;
    private Animation an;

    private void Start()
    {
        an = GetComponent<Animation>();
    }

    private void OnEnable()
    {
        GameManager.ActivateFinalAnimation += ActivateFinalAnimation;
        LevelManager.OnLevelLoaded += ThisLevelLoaded;
    }

    private void OnDisable()
    {
        GameManager.ActivateFinalAnimation -= ActivateFinalAnimation;
        LevelManager.OnLevelLoaded -= ThisLevelLoaded;
    }

    void ThisLevelLoaded()
    {
        if (menuButton == null && canvasToMenu != null)
        {
            menuButton = canvasToMenu.GetComponentInChildren<Button>();
        }

        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();

            menuButton.onClick.AddListener(GoToMenuAction);
        }
    }

    void ActivateFinalAnimation(int id)
    {
        if (id < 0 || id >= endingAnimations.Count)
            return;

        currentAnimation = id;
        AnimationClip targetClip = endingAnimations[id];

        if (targetClip != null)
        {
            if (an.GetClip(targetClip.name) == null)
            {
                an.AddClip(targetClip, targetClip.name);
            }

            an.clip = targetClip;
            an.Play();
        }
    }

    public void ActivateGoToMenu()
    {
        if (canvasToMenu != null)
        {
            canvasToMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void GoToMenuAction()
    {
        GameManager.Instance.levelManager.LoadMenu();
    }
}