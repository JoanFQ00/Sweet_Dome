using UnityEngine;

[RequireComponent(typeof(HL_Interact))]

public class BasicInt : MonoBehaviour, IInteractable, ILookable
{
    private PlayerController playerController;

    private enum Type {Lights, Radio, RecordPlayer}
    [SerializeField] private Type type;

    private bool isPlaying;

    [Header("Pop Up")]
    [SerializeField] private float disanceToPop = 1.6f;
    [SerializeField] private GameObject popUpGO;
    [SerializeField] private GameObject interactPopUpGO;
    private bool onSight;

    void Start()
    {
        playerController = GameManager.Instance.playerController;
        StartStop();
    }

    private void OnEnable()
    {
        GameManager.NewDay += StartStop;
        LevelManager.OnLevelLoaded += StartStop;
    }

    private void OnDisable()
    {
        GameManager.NewDay -= StartStop;
        LevelManager.OnLevelLoaded -= StartStop;
    }

    void Update() 
    {
        if (popUpGO != null)
        {
            PopUpCheck();
        }

        if (GameManager.Instance != null) 
        {
            if (GameManager.Instance.dayTime.currentHour == 15)
            {

                isPlaying = false; 
            }
        }
    }

    public void Interact() 
    {
        switch (type) 
        {
            case Type.Lights:
                break;

            case Type.Radio:
                break;

            case Type.RecordPlayer:
                StartStop();
                break;
        }
    }

    void StartStop() 
    {
        if (!isPlaying)
        {
            switch (GameManager.Instance.currentDay)  //Falta poner musicas
            {
                case Day.Day1:
                break;
                case Day.Day2:

                break;
                case Day.Day3:

                break;
                case Day.Day4:

                break;
                case Day.Day5:

                break;
            }

            isPlaying = true;
        }
        else 
        {
            // STOP Music
            isPlaying = false;
        }
    }

    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }

    private void PopUpCheck()
    {
        if (GameManager.Instance.currentState == GameManager.GameState.Gameplay)
        {
            if (playerController == null) return; 

            float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

            if (onSight)
            {
                popUpGO.SetActive(false);
                interactPopUpGO.SetActive(true);
                interactPopUpGO.transform.LookAt(playerController.transform);
            }
            else if (distanceToPlayer <= disanceToPop)
            {
                popUpGO.SetActive(true);
                interactPopUpGO.SetActive(false);
                popUpGO.transform.LookAt(playerController.transform);
            }
            else
            {
                popUpGO.SetActive(false);
                interactPopUpGO.SetActive(false);
            }
        }
        else
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
        }
    }
}
