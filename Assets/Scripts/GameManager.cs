using System;
using Unity.VisualScripting;
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

    private void Update()
    {
        // For debug
        if (Input.GetKeyDown(KeyCode.M)) 
        {
            ChangeDay();
        }

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
