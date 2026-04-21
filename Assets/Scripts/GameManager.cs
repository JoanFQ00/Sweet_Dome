using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public PlayerController playerController;

    public static Action OnChangeDay;
   
    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        BedInt.OnInteract += ChangeDay;
    }

    private void OnDisable()
    {
        BedInt.OnInteract -= ChangeDay;

    }

    public void SetPlayer(PlayerController controller) 
    {
        playerController = controller;
    }

    public void ChangeDay() 
    {
        OnChangeDay?.Invoke();    
    }
}
