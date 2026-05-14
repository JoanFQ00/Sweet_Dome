using UnityEngine;

public class CentifugeMachine : MonoBehaviour
{
    [Header("Funnels")]
    [SerializeField] private FunnelBase blenderFunnel;
    [SerializeField] private FunnelBase purifiedStorage;

    [Header("ButtonLight")]
    [SerializeField] private GameObject buttonLight;

    bool buttonActivated = false;

    private void Start()
    {
        buttonLight.SetActive(false);
        buttonActivated = false;
    }

    private void Update()
    {
        if (!buttonActivated && blenderFunnel.CurrentCapacity > 0)
        {
            buttonLight.SetActive(true);
            buttonActivated = true;
        }
        else if (blenderFunnel.CurrentCapacity <= 0)
        {
            buttonLight.SetActive(false);
            buttonActivated = false;
        }
    }

    public void Check()
    {
        if (buttonActivated)
        {
            float availableSpace =
                purifiedStorage.MaxCapacity - purifiedStorage.CurrentCapacity;

            float amountToTransfer =
                Mathf.Min(blenderFunnel.CurrentCapacity, availableSpace);

            purifiedStorage.AddLiquid(amountToTransfer);

            blenderFunnel.RemoveLiquid(amountToTransfer);

            buttonActivated = false;
        }
    }
}