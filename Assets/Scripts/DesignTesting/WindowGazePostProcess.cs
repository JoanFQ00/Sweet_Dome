using UnityEngine;
using UnityEngine.Rendering;

public class WindowGazePostProcess : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume windowVolume;

    [Tooltip("Arrastra aquí un GameObject vacío colocado justo enfrente de la ventana (en el exterior, donde está la ciudad)")]
    [SerializeField] private Transform windowFrontTarget;

    private Transform playerCameraTransform;

    [Header("Gaze Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float gazeThreshold = 0.5f; // Bájalo si quieres que se active más de reojo, súbelo si quieres que sea muy estricto de frente
    [SerializeField] private float transitionSpeed = 5f;

    private bool isPlayerInside = false;
    private float targetWeight = 0f;

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerController != null)
        {
            playerCameraTransform = GameManager.Instance.playerController.mainCamera.transform;
        }

        if (windowVolume != null)
        {
            windowVolume.isGlobal = false;
            windowVolume.weight = 0f;
        }
    }

    private void Update()
    {
        if (windowVolume == null) return;

        // Si no tenemos el Target guía o el jugador está fuera, el sol se apaga
        if (!isPlayerInside || playerCameraTransform == null || windowFrontTarget == null)
        {
            windowVolume.weight = Mathf.MoveTowards(windowVolume.weight, 0f, Time.deltaTime * transitionSpeed);
            return;
        }

        // 1. Conseguimos el vector hacia donde mira la cámara del jugador
        Vector3 cameraForward = playerCameraTransform.forward;

        // 2. Calculamos la dirección real desde la cabeza del jugador hacia el punto exterior (la ciudad)
        Vector3 directionToOutside = (windowFrontTarget.position - playerCameraTransform.position).normalized;

        // 3. Comparamos ambos vectores
        float dot = Vector3.Dot(cameraForward, directionToOutside);

        // Si el jugador mira en la dirección del punto exterior, se activa el sol
        if (dot >= gazeThreshold)
        {
            targetWeight = Mathf.InverseLerp(gazeThreshold, 1f, dot);
        }
        else
        {
            targetWeight = 0f;
        }

        windowVolume.weight = Mathf.MoveTowards(windowVolume.weight, targetWeight, Time.deltaTime * transitionSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
        {
            isPlayerInside = false;
        }
    }
}