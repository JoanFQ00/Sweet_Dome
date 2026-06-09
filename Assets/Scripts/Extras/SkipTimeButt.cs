using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class SkipTimeButt : MonoBehaviour, IInteractable
{
    public UnityEvent onPress;
    public static Action<bool> OnFirstDayButt;

    public Vector3 originalPos = new Vector3(2.9f, 1.3f, -8.14f);
    public Quaternion originalRot = new Quaternion(0, -142, 0, 0);

    [Header("Cooldown")]
    public float interactCooldown = 1f;

    private bool canInteract = true;

    private int defaultLayer;
    private int interactableLayer;

    private void Awake()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        interactableLayer = LayerMask.NameToLayer("Interactable");
    }

    private void OnEnable()
    {
        LevelManager.OnLevelLoaded += SetOriginalPosition;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelLoaded -= SetOriginalPosition;
    }

    void SetOriginalPosition() 
    {
        transform.position = originalPos;
        transform.rotation = originalRot;
    }

    public void Interact()
    {
        if (!canInteract)
            return;

        StartCoroutine(InteractionCooldown());
        int hour = GameManager.Instance.dayTime.currentHour;


        if (GameManager.Instance.currentDay == Day.Day6)
            onPress?.Invoke();
        else if (GameManager.Instance.currentDay == Day.Day1)
        {
            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day2)
        {

            if (hour < 10)
            {
                GameManager.Instance.dayTime.SetCurrentTime(10, 0);
            }
            else if (hour < 11)
            {
                GameManager.Instance.dayTime.SetCurrentTime(11, 30);
            }
            else
            {
                GameManager.Instance.dayTime.SetCurrentTime(15, 0);
            }

            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day3) 
        {
            if (hour < 10)
            {
                GameManager.Instance.dayTime.SetCurrentTime(10, 0);
            }
            else if (hour < 11)
            {
                if (GameManager.Instance.dayManager.
                    dorothyAngriedDay2.IsMet(GameManager.Instance.decManager) && GameManager.Instance.dayManager.
                    dorothyAngriedDay3.IsMet(GameManager.Instance.decManager))
                {
                    GameManager.Instance.dayTime.AddMinutes(30);
                }
                else 
                {
                    GameManager.Instance.dayTime.SetCurrentTime(11, 30);
                }
            }
            else
            {
                
            }
        }
    }

    private IEnumerator InteractionCooldown()
    {
        canInteract = false;

        gameObject.layer = defaultLayer;

        yield return new WaitForSeconds(interactCooldown);

        gameObject.layer = interactableLayer;

        canInteract = true;
    }
}