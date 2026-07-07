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

    [Header("Animation Reference")]
    [SerializeField] private RedButtonAnims buttonVisualController;

    private bool canInteract = true;

    bool hasTriedSkipReadDay3;

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

        if (buttonVisualController != null)
        {
            buttonVisualController.TriggerButtonPress();
        }

        StartCoroutine(InteractionCooldown());
        int hour = GameManager.Instance.dayTime.currentHour;

        if (GameManager.Instance.currentDay == Day.Day1)
        {
            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day2)
        {
            if (hour < 10)
            {
                GameManager.Instance.dayTime.LerpToTime(10, 0, 3);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");
            }
            else if (hour < 11)
            {
                GameManager.Instance.dayTime.LerpToTime(11, 30);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");
            }
            else
            {
                GameManager.Instance.dayTime.LerpToTime(15, 0);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
            }

            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day3)
        {
            if (hour < 10)
            {
                GameManager.Instance.dayTime.LerpToTime(10, 0);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");
                GameManager.Instance.dayManager.D22 = true;
            }
            else if (hour < 11)
            {
                bool isAngry = GameManager.Instance.dayManager
                    .dorothyAngriedDay3.IsMet(GameManager.Instance.decManager);

                if (isAngry)
                {
                    GameManager.Instance.dayTime.LerpToTime(11, 30);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");

                    GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.EncourageRead, true);
                    hasTriedSkipReadDay3 = true;
                }
            }
            else if (hour < 11)
            {
                if (GameManager.Instance.dayManager.hasTriedToSkipTVDay3)
                {
                    GameManager.Instance.dayTime.LerpToTime(15, 0);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
                }
                else
                {
                    GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.EncourageTV, true);
                    GameManager.Instance.dayManager.D32 = true;
                    GameManager.Instance.dayManager.hasTriedToSkipTVDay3 = true;
                }
            }

            OnFirstDayButt?.Invoke(true);
        }
        else if (GameManager.Instance.currentDay == Day.Day4)
        {
            if (hour < 10)
            {
                if (GameManager.Instance.dayManager.D31)
                {
                    if (!GameManager.Instance.dayManager.trySkipEatingD4)
                    {
                        GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.PleaseEatMyLaunch, true);
                        GameManager.Instance.dayManager.trySkipEatingD4 = true;
                    }
                    else
                    {
                        GameManager.Instance.dayTime.LerpToTime(10, 00);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");
                        GameManager.Instance.dayManager.notLaunchDay4 = true;
                    }
                }
                else if (GameManager.Instance.dayManager.D32)
                {
                    GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.VeryMad, true);
                    GameManager.Instance.dayTime.LerpToTime(10, 00);

                    GameManager.Instance.dayManager.D53 = true;
                }
            }
            else if (hour < 11)
            {
                if (GameManager.Instance.dayManager.D31)
                {
                    GameManager.Instance.dayTime.LerpToTime(11, 30);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");

                    if (GameManager.Instance.dayManager.
                        dorothyLaunch2.IsMet(GameManager.Instance.decManager))
                    {
                        GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.ILoveYou, true);
                        GameManager.Instance.dayManager.D51 = true;
                    }
                    else
                    {
                        GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.MadDoro, true);
                        GameManager.Instance.dayManager.D52 = true;
                    }
                }
                else if (GameManager.Instance.dayManager.D32)
                {
                    GameManager.Instance.dayTime.LerpToTime(11, 30);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");
                }
            }
            else if (hour < 12)
            {
                GameManager.Instance.dayTime.LerpToTime(15, 0);
                GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
                GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoToWorkDay2);
            }
            else if (hour <= 15)
            {
                if (GameManager.Instance.dayManager.hasWatchedTv.IsMet(GameManager.Instance.decManager)
                    && GameManager.Instance.dayManager.triedToSkipMachine)
                {
                    GameManager.Instance.dayManager.PresidentSpeaks(PresiDialogue.MattewTraitorDay1);
                    GameManager.Instance.ExecuteFinishAnimation(0, 6);
                }
                else if (GameManager.Instance.dayManager.hasWatchedTv.IsMet(GameManager.Instance.decManager))
                {
                    GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.TryToSkipMachineDay1, true);
                    GameManager.Instance.dayManager.triedToSkipMachine = true;
                }
            }
        }
        else if (GameManager.Instance.currentDay == Day.Day5)
        {
            if (hour < 10)
            {
                if (GameManager.Instance.dayManager.D72)
                {
                    GameManager.Instance.dayTime.LerpToTime(10, 00);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");

                    if (!GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayManager.D71 = true;
                    }
                }
                else if (GameManager.Instance.dayManager.D71)
                {
                    if (GameManager.Instance.dayManager.D61 && !GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayTime.LerpToTime(10, 00);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");
                        GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.LastBookRec, true);
                        GameManager.Instance.dayManager.hasEated_SC = true;
                        GameManager.Instance.dayManager.dorothyBook.SetActive(true);
                        GameManager.Instance.dayManager.dorothyBook.GetComponent<Action_Reader>().ChangeTexture(Day.Day4);
                    }
                    else if (GameManager.Instance.dayManager.D62 && !GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayTime.LerpToTime(10, 00);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Lee");
                        GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.LastBookRec, true);
                        GameManager.Instance.dayManager.hasEated_SC = true;
                    }
                }
            }
            else if (hour < 11)
            {
                if (GameManager.Instance.dayManager.D72)
                {
                    if (GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayTime.LerpToTime(11, 30);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");
                    }
                }
                if (GameManager.Instance.dayManager.D71)
                {
                    if (GameManager.Instance.dayManager.D61 && !GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)DialoguesRobo.DorothyGoToWindow);
                    }
                    else if (GameManager.Instance.dayManager.D62 && !GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayTime.LerpToTime(11, 30);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Mira la tele");

                        GameManager.Instance.dayManager.D61 = true;
                        GameManager.Instance.dayManager.D62 = false;
                    }
                }
            }
            else if (hour < 12)
            {
                if (GameManager.Instance.dayManager.D72)
                {
                    if (GameManager.Instance.dayManager.D53)
                    {
                        GameManager.Instance.dayTime.LerpToTime(15, 0);
                        GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
                        GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoToWorkDay2);
                        GameManager.Instance.dayManager.D84 = true;
                    }
                }

                if (GameManager.Instance.dayManager.dorothyWaiting)
                {
                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)DialoguesRobo.InsistToGo);
                    GameManager.Instance.dayManager.makeDorothySad = true;
                }

                if (GameManager.Instance.dayManager.makeDorothySad)
                {
                    GameManager.Instance.dayManager.D82 = true;
                    GameManager.Instance.dayManager.ChangeDorothyDialogue((int)DialoguesRobo.DortohySad, true);
                    GameManager.Instance.dayTime.LerpToTime(15, 0);
                    GameManager.Instance.UI.GetComponent<UIManager>().ForceChangeObjectives("Completa la máquina");
                    GameManager.Instance.presiSpeaker.PresiSpeaks((int)PresiDialogue.GoToWorkDay2);
                }
            }
        }

        //onPress?.Invoke();
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