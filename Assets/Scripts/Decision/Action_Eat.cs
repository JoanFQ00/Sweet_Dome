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
            GameManager.Instance.playerController.
            GetComponent<Interact>().ChangeInteracting(false);

            ResetEatingState();
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

    public void ResetEatingState()
    {
        isEating = false;
        timer = 0f;

        if (currentFood != null)
        {
            Destroy(currentFood);
            currentFood = null;
        }

        GameManager.Instance.playerController.LockPlayer(false);
        GameManager.Instance.playerController.LockCamera(false);

        OnActionExecuted();
    }
}