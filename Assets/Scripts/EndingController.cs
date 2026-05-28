using System.Collections.Generic;
using UnityEngine;
using System;

public class EndingController : MonoBehaviour
{
    [Header("FinishAnimations")]
    public List<AnimationClip> endingAnimations;

    private int currentAnimation;
    private Animation an;

    private void Start()
    {
        an = GetComponent<Animation>();
    }

    private void OnEnable()
    {
        GameManager.ActivateFinalAnimation += ActivateFinalAnimation;
    }

    private void OnDisable()
    {
        GameManager.ActivateFinalAnimation -= ActivateFinalAnimation;
    }

    void ActivateFinalAnimation(int id)
    {

        Debug.Log("Here");
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
}