using Mono.Cecil;
using UnityEngine;

public class Action_Eat : MonoBehaviour,IInteractable
{
    public Dec_Act eat;

    public GameObject foodPrefab;
    public float distanceFromPlayer = 1.5f;
    public float duration = 3f;

    private GameObject currentFood;
    private float timer;

    private bool isEating = false;


    void Start()
    {
        
    }

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

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        isEating = false;

        eat.Execute();
    }

    bool CheckHour()
    {
        int hour = DayTime.Instance.GetCurrentHour();

        if (hour < eat.minIntHour
            || hour > eat.maxIntHour)
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            return false;
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            return true;
        }
    }
}
