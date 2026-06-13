using System;
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    public SliderType type;
    public static Action<float, SliderType> OnRotatedMoved;

    [Header("Configuración de Control")]
    [Tooltip("Radio de zona muerta en píxeles. Si el ratón pasa muy cerca del centro al arrastrar, se ignora el salto para evitar el giro brusco.")]
    [SerializeField] private float centerDeadZonePixels = 35f;

    private bool isDragging;
    private float currentAngle;
    private Vector3 initialRotation;
    private float lastMouseAngle;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = transform.localEulerAngles.x;
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        // 1. Click inicial: guardamos la posición angular del ratón
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                isDragging = true;
                lastMouseAngle = GetMouseAngle();
            }
        }

        // 2. Soltar el click apaga el arrastre
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging)
            return;

        // 3. Calculamos la distancia actual del cursor al centro del volante en la pantalla
        Vector3 screenCenter = Camera.main.WorldToScreenPoint(transform.position);
        float distanceToCenter = Vector2.Distance(Input.mousePosition, (Vector2)screenCenter);

        // ¡LA CLAVE!: Si el jugador pasa el ratón por todo el medio cruzando de un lado a otro,
        // la distancia cae drásticamente. Congelamos el cálculo en ese frame para tragar el salto de 180°.
        if (distanceToCenter < centerDeadZonePixels)
        {
            return;
        }

        // 4. Calculamos el ángulo actual y su variación respecto al frame anterior
        float currentMouseAngle = GetMouseAngle();
        float deltaAngle = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);

        // Actualizamos el rastro para el siguiente frame
        lastMouseAngle = currentMouseAngle;

        // 5. Aplicamos la rotación incremental de forma suave sin teletransportes
        if (!Mathf.Approximately(deltaAngle, 0f))
        {
            // Amortiguamos cambios de ángulo absurdamente gigantescos en un solo frame (el salto de TP)
            if (Mathf.Abs(deltaAngle) < 120f)
            {
                currentAngle += deltaAngle;
                ApplyRotation();
                SendValue(deltaAngle);
            }
            else
            {
                // Si el cambio es un volantazo fantasma debido al cruce de ejes, 
                // simplemente actualizamos el ángulo de rastro sin mover la malla física.
                lastMouseAngle = currentMouseAngle;
            }
        }
    }

    // Calcula el ángulo circular (0 a 360) del cursor usando el centro en pantalla como pivote
    float GetMouseAngle()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 direction = (Vector2)Input.mousePosition - (Vector2)screenPos;

        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(
            currentAngle,
            initialRotation.y,
            initialRotation.z
        );
    }

    void SendValue(float d)
    {
        OnRotatedMoved?.Invoke(d, type);
    }
}