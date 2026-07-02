using UnityEngine;

public class InteractablePopup : MonoBehaviour, ILookable
{
    [Header("Configuración de Distancia")]
    [Tooltip("Distancia máxima para mostrar el popup de cercanía.")]
    [SerializeField] private float distanceToPop = 5.0f;

    [Header("Referencias de UI (Popups)")]
    [SerializeField] private GameObject popUpGO;         // Imagen 1: El jugador está cerca
    [SerializeField] private GameObject interactPopUpGO; // Imagen 2: El jugador mira fijamente

    private PlayerController playerController;
    private Camera playerCamera;
    private bool onSight;

    void Start()
    {
        if (popUpGO != null)
        {
            popUpGO.SetActive(false);
            interactPopUpGO.SetActive(false);
        }

        // Inicializamos las referencias del jugador usando tu GameManager
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerController = GameManager.Instance.playerController;
            playerCamera = playerController.mainCamera;
        }
    }

    void Update()
    {
        if (popUpGO == null || interactPopUpGO == null) return;

        PopUpCheck();
    }

    private void PopUpCheck()
    {
        // Si el jugador no está asignado, lo buscamos de nuevo
        if (playerController == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.playerController != null)
            {
                playerController = GameManager.Instance.playerController;
                playerCamera = playerController.mainCamera;
            }
            return;
        }

        // Si el jugador está en menús, diálogos o interactuando, apagamos todo
        if (GameManager.Instance.currentState != GameManager.GameState.Gameplay)
        {
            SetPopUps(false, false);
            return;
        }

        // Calculamos la distancia exacta tal y como hace la silla
        float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

        // LOGICA DE FILTRADO (Tu sistema exacto)
        if (onSight)
        {
            SetPopUps(false, true);

            if (playerCamera != null)
                interactPopUpGO.transform.rotation = playerCamera.transform.rotation;
        }
        else if (distanceToPlayer <= distanceToPop)
        {
            SetPopUps(true, false);

            if (playerCamera != null)
                popUpGO.transform.rotation = playerCamera.transform.rotation;
        }
        else
        {
            SetPopUps(false, false);
        }
    }

    private void SetPopUps(bool showNormal, bool showInteract)
    {
        if (popUpGO.activeSelf != showNormal)
            popUpGO.SetActive(showNormal);

        if (interactPopUpGO.activeSelf != showInteract)
            interactPopUpGO.SetActive(showInteract);
    }

    // Este es el método que tu script 'Interact' del jugador llama obligatoriamente por Raycast
    public void OnLookStateChanged(bool isLooking)
    {
        onSight = isLooking;
    }
}