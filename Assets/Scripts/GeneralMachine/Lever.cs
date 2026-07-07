using UnityEngine;
using System.Collections;

public class Lever : MonoBehaviour, IResetable
{
    [Header("Referencies")]
    public Machine01 machine;

    [SerializeField] private PumpEmissionController purifiedPumpController;
    [SerializeField] private PumpEmissionController rawPumpController;

    [Header("Lever Angles")]
    public float suckAngle = -22f;
    public float neutralAngle = -70f;
    public float outAngle = -129f;

    [Header("Drag Settings")]
    public float dragSpeed = 2f;

    [Header("Return Settings")]
    public float returnSpeed = 10f;

    [Header("Configuración Estética de Luces (Palanca)")]
    [ColorUsage(true, true)]
    [SerializeField] private Color neutralColor = Color.white;

    [ColorUsage(true, true)]
    [SerializeField] private Color absorbColor = Color.blue;

    [ColorUsage(true, true)]
    [SerializeField] private Color expelColor = new Color(1f, 0.08f, 0.57f); // Rosa HDR

    public bool IsAbsorbing => currentState == LeverState.Absorb;
    public bool IsExpelling => currentState == LeverState.Expelling;
    public bool IsActualNeutral => currentState == LeverState.Neutral;
    public bool IsNeutral => currentState == LeverState.Neutral;

    private bool isDragging;
    public float currentAngle;
    private Vector3 initialRotation;

    private enum LeverState { Absorb, Neutral, Expelling }
    private LeverState currentState;

    private Material runtimeLeverMat;
    private Color currentLeverColor;
    private Coroutine leverColorCoroutine;

    private void Awake()
    {
        Renderer rend = GetComponent<Renderer>() ?? GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            runtimeLeverMat = rend.material;
            runtimeLeverMat.EnableKeyword("_EMISSION");
        }
        currentLeverColor = neutralColor;
    }

    private void Start()
    {
        initialRotation = transform.localEulerAngles;
        currentAngle = NormalizeAngle(initialRotation.x);

        if (GameManager.Instance != null) GameManager.Instance.RegisterResetable(this);

        ApplyLeverColorDirect(neutralColor);
        currentState = LeverState.Neutral;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.UnregisterResetable(this);
    }

    private void Update()
    {
        if (isDragging) DragLever();
        else ReturnToNeutral();

        CheckLeverState();
    }

    void DragLever()
    {
        float mouseY = Input.GetAxis("Mouse Y");
        currentAngle += mouseY * dragSpeed;
        currentAngle = Mathf.Clamp(currentAngle, outAngle, suckAngle);
        ApplyRotation();
    }

    void ReturnToNeutral()
    {
        currentAngle = Mathf.Lerp(currentAngle, neutralAngle, Time.deltaTime * returnSpeed);
        if (Mathf.Abs(currentAngle - neutralAngle) < 0.01f) currentAngle = neutralAngle;
        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(currentAngle, initialRotation.y, initialRotation.z);
    }

    void CheckLeverState()
    {
        float suckDist = Mathf.Abs(currentAngle - suckAngle);
        float neutralDist = Mathf.Abs(currentAngle - neutralAngle);
        float outDist = Mathf.Abs(currentAngle - outAngle);

        LeverState newState = LeverState.Neutral;
        if (suckDist < neutralDist && suckDist < outDist) newState = LeverState.Absorb;
        else if (neutralDist < outDist) newState = LeverState.Neutral;
        else newState = LeverState.Expelling;

        if (newState != currentState)
        {
            currentState = newState;
            UpdateEcosystemColors();
        }

        if (currentState == LeverState.Absorb) machine.GetActivePump().Absorbing();
        else if (currentState == LeverState.Expelling) machine.GetActivePump().Expelling();
    }

    private void UpdateEcosystemColors()
    {
        if (machine == null) return;

        PumpEmissionController activeController = machine.IsPurifiedMode ? purifiedPumpController : rawPumpController;

        switch (currentState)
        {
            case LeverState.Absorb:
                StartLeverColorLerp(absorbColor);
                if (activeController != null) activeController.OverridePumpColor(absorbColor);
                break;

            case LeverState.Expelling:
                StartLeverColorLerp(expelColor);
                if (activeController != null) activeController.OverridePumpColor(expelColor);
                break;

            case LeverState.Neutral:
                StartLeverColorLerp(neutralColor);
                if (purifiedPumpController != null) purifiedPumpController.RestoreDefaultColor();
                if (rawPumpController != null) rawPumpController.RestoreDefaultColor();
                break;
        }
    }

    private void StartLeverColorLerp(Color targetColor)
    {
        if (leverColorCoroutine != null) StopCoroutine(leverColorCoroutine);
        leverColorCoroutine = StartCoroutine(LerpLeverColor(targetColor));
    }

    private IEnumerator LerpLeverColor(Color targetColor)
    {
        Color startColor = currentLeverColor;
        float elapsedTime = 0f;
        float duration = 0.12f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentLeverColor = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            ApplyLeverColorDirect(currentLeverColor);
            yield return null;
        }
        currentLeverColor = targetColor;
        ApplyLeverColorDirect(currentLeverColor);
    }

    private void ApplyLeverColorDirect(Color col)
    {
        if (runtimeLeverMat != null) runtimeLeverMat.SetColor("_EmissionColor", col);
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        return angle;
    }

    private void OnMouseDown() => isDragging = true;
    private void OnMouseUp() => isDragging = false;

    public void ResetValues()
    {
        isDragging = false;
        currentAngle = neutralAngle;
        ApplyRotation();

        if (leverColorCoroutine != null) StopCoroutine(leverColorCoroutine);
        ApplyLeverColorDirect(neutralColor);
        currentLeverColor = neutralColor;
        currentState = LeverState.Neutral;

        if (purifiedPumpController != null) purifiedPumpController.ResetValues();
        if (rawPumpController != null) rawPumpController.ResetValues();
    }
}