using UnityEngine;

public class Action_Eat : BaseAction
{
    public GameObject foodPrefab;
    public float distanceFromPlayer = 1.5f;
    public float duration = 3f;

    private GameObject currentFood;
    private float timer;
    private bool isEating = false;

    protected override void OnUpdateAction()
    {
        if (!isEating) return;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            FinishEating();
        }
    }

    public override void Interact()
    {
        if (isEating) return;

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

        OnActionExecuted();
    }
}