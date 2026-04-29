using UnityEngine;

public class Action_Eat : MonoBehaviour,IInteractable
{
    public Dec_Act eat;
    private bool actionCompleted = false;

    public GameObject foodPrefab;
    public float distanceFromPlayer = 1.5f;
    public float duration = 3f;

    private GameObject currentFood;
    private float timer;
    private int currentLayer = -1;

    private bool isEating = false;

    void Update()
    {
        CheckHour();
        if (isEating)
        {
            timer += Time.deltaTime;

            if (timer >= duration)
            {
                FinishEating();
            }
        }
    }

    public void Interact()
    {
        if (isEating) return;

        StartEating();
    }

    void StartEating()
    {
        Transform player = GameManager.Instance.playerController.transform;
        gameObject.layer = LayerMask.NameToLayer("Default");

        currentFood = Instantiate(foodPrefab);
        currentFood.transform.position = player.position + player.forward * distanceFromPlayer;

        GameManager.Instance.playerController.LockPlayer(true);
        GameManager.Instance.playerController.LockCamera(true);

        timer = 0f;
        isEating = true;
    }

    void FinishEating()
    {
        Destroy(currentFood);
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        isEating = false;

        eat.ActionExecuted();
    }

    void CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();
        int newLayer;

        if (hour < eat.minIntHour || hour >= eat.maxIntHour)
            newLayer = LayerMask.NameToLayer("Default");
        else
            newLayer = LayerMask.NameToLayer("Interactable");

        if (hour >= eat.maxIntHour && actionCompleted == false)
        {
            eat.ActionFailed();
        }


        if (newLayer != currentLayer)
        {
            gameObject.layer = newLayer;
            currentLayer = newLayer;
        }
    }
}
